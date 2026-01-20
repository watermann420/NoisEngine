using NAudio.Wave;

namespace MusicEngine.Core.Effects.Dynamics;

/// <summary>
/// Dynamic range compressor effect.
/// Reduces the volume of loud sounds above a threshold, with configurable ratio,
/// attack, release, and makeup gain.
/// </summary>
public class CompressorEffect : EffectBase
{
    private float[] _envelope;    // Envelope follower state per channel
    private float[] _gainSmooth;  // Smoothed gain reduction per channel

    /// <summary>
    /// Creates a new compressor effect
    /// </summary>
    /// <param name="source">Audio source to compress</param>
    /// <param name="name">Effect name</param>
    public CompressorEffect(ISampleProvider source, string name)
        : base(source, name)
    {
        int channels = source.WaveFormat.Channels;
        _envelope = new float[channels];
        _gainSmooth = new float[channels];

        // Initialize parameters
        RegisterParameter("Threshold", -20f);     // -20 dB
        RegisterParameter("Ratio", 4f);           // 4:1 compression
        RegisterParameter("Attack", 0.005f);      // 5ms attack
        RegisterParameter("Release", 0.1f);       // 100ms release
        RegisterParameter("MakeupGain", 0f);      // 0 dB makeup gain
        RegisterParameter("KneeWidth", 0f);       // Hard knee (0 = hard, >0 = soft)
        RegisterParameter("AutoGain", 0f);        // Auto makeup gain off
        Mix = 1.0f;                               // 100% wet
    }

    /// <summary>
    /// Threshold in dB (-60 to 0)
    /// Signals above this level will be compressed
    /// </summary>
    public float Threshold
    {
        get => GetParameter("Threshold");
        set => SetParameter("Threshold", Math.Clamp(value, -60f, 0f));
    }

    /// <summary>
    /// Compression ratio (1.0 - 20.0)
    /// 1.0 = no compression, 4.0 = 4:1 ratio, 20.0 = limiting
    /// </summary>
    public float Ratio
    {
        get => GetParameter("Ratio");
        set => SetParameter("Ratio", Math.Clamp(value, 1f, 20f));
    }

    /// <summary>
    /// Attack time in seconds (0.0001 - 1.0)
    /// How fast the compressor responds to loud signals
    /// </summary>
    public float Attack
    {
        get => GetParameter("Attack");
        set => SetParameter("Attack", Math.Clamp(value, 0.0001f, 1f));
    }

    /// <summary>
    /// Release time in seconds (0.001 - 5.0)
    /// How fast the compressor returns to normal after loud signal ends
    /// </summary>
    public float Release
    {
        get => GetParameter("Release");
        set => SetParameter("Release", Math.Clamp(value, 0.001f, 5f));
    }

    /// <summary>
    /// Makeup gain in dB (0 - 48)
    /// Compensates for volume loss from compression
    /// </summary>
    public float MakeupGain
    {
        get => GetParameter("MakeupGain");
        set => SetParameter("MakeupGain", Math.Clamp(value, 0f, 48f));
    }

    /// <summary>
    /// Knee width in dB (0 - 20)
    /// 0 = hard knee (abrupt), >0 = soft knee (smooth transition)
    /// </summary>
    public float KneeWidth
    {
        get => GetParameter("KneeWidth");
        set => SetParameter("KneeWidth", Math.Clamp(value, 0f, 20f));
    }

    /// <summary>
    /// Auto makeup gain (0 = off, 1 = full auto)
    /// Automatically calculates makeup gain to compensate for compression
    /// </summary>
    public float AutoGain
    {
        get => GetParameter("AutoGain");
        set => SetParameter("AutoGain", Math.Clamp(value, 0f, 1f));
    }

    /// <summary>
    /// Dry/Wet mix (0.0 - 1.0)
    /// 0.0 = fully dry (no compression), 1.0 = fully wet (full compression)
    /// </summary>
    public float DryWet
    {
        get => Mix;
        set => Mix = value;
    }

    protected override void ProcessBuffer(float[] sourceBuffer, float[] destBuffer, int offset, int count)
    {
        int channels = Channels;
        int sampleRate = SampleRate;

        float threshold = Threshold;
        float ratio = Ratio;
        float attack = Attack;
        float release = Release;
        float makeupGain = MakeupGain;
        float kneeWidth = KneeWidth;
        float autoGain = AutoGain;

        // Calculate attack and release coefficients
        float attackCoeff = MathF.Exp(-1f / (attack * sampleRate));
        float releaseCoeff = MathF.Exp(-1f / (release * sampleRate));

        // Convert makeup gain from dB to linear
        float makeupGainLinear = MathF.Pow(10f, makeupGain / 20f);

        // Calculate auto makeup gain if enabled
        if (autoGain > 0f)
        {
            float autoMakeup = (threshold * (1f - 1f / ratio)) * autoGain;
            makeupGainLinear *= MathF.Pow(10f, autoMakeup / 20f);
        }

        for (int i = 0; i < count; i += channels)
        {
            for (int ch = 0; ch < channels; ch++)
            {
                float input = sourceBuffer[i + ch];

                // Envelope detection (peak detector)
                float inputAbs = MathF.Abs(input);
                float coeff = inputAbs > _envelope[ch] ? attackCoeff : releaseCoeff;
                _envelope[ch] = inputAbs + coeff * (_envelope[ch] - inputAbs);

                // Convert to dB
                float inputDb = 20f * MathF.Log10(_envelope[ch] + 1e-6f);

                // Calculate gain reduction
                float gainReductionDb = 0f;

                if (kneeWidth > 0f)
                {
                    // Soft knee
                    float kneeMin = threshold - kneeWidth / 2f;
                    float kneeMax = threshold + kneeWidth / 2f;

                    if (inputDb > kneeMin && inputDb < kneeMax)
                    {
                        // In the knee region
                        float kneeInput = inputDb - kneeMin;
                        float kneeFactor = kneeInput / kneeWidth;
                        gainReductionDb = kneeFactor * kneeFactor * (threshold - inputDb + kneeWidth / 2f) * (1f - 1f / ratio);
                    }
                    else if (inputDb >= kneeMax)
                    {
                        // Above knee
                        gainReductionDb = (threshold - inputDb) * (1f - 1f / ratio);
                    }
                }
                else
                {
                    // Hard knee
                    if (inputDb > threshold)
                    {
                        gainReductionDb = (threshold - inputDb) * (1f - 1f / ratio);
                    }
                }

                // Convert gain reduction to linear and smooth it
                float targetGain = MathF.Pow(10f, gainReductionDb / 20f);
                float smoothCoeff = targetGain < _gainSmooth[ch] ? attackCoeff : releaseCoeff;
                _gainSmooth[ch] = targetGain + smoothCoeff * (_gainSmooth[ch] - targetGain);

                // Apply compression and makeup gain
                float output = input * _gainSmooth[ch] * makeupGainLinear;

                destBuffer[offset + i + ch] = output;
            }
        }
    }
}
