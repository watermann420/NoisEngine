using NAudio.Wave;

namespace MusicEngine.Core.Effects.Filters;

/// <summary>
/// 3-band parametric equalizer effect.
/// Each band has independent frequency, gain, and Q (bandwidth) controls.
/// </summary>
public class ParametricEQEffect : EffectBase
{
    private EQBand[] _lowBands;    // Low band (per channel)
    private EQBand[] _midBands;    // Mid band (per channel)
    private EQBand[] _highBands;   // High band (per channel)

    /// <summary>
    /// Creates a new 3-band parametric EQ effect
    /// </summary>
    /// <param name="source">Audio source to process</param>
    /// <param name="name">Effect name</param>
    public ParametricEQEffect(ISampleProvider source, string name)
        : base(source, name)
    {
        int channels = source.WaveFormat.Channels;

        _lowBands = new EQBand[channels];
        _midBands = new EQBand[channels];
        _highBands = new EQBand[channels];

        for (int i = 0; i < channels; i++)
        {
            _lowBands[i] = new EQBand();
            _midBands[i] = new EQBand();
            _highBands[i] = new EQBand();
        }

        // Initialize parameters
        // Low band (bass)
        RegisterParameter("LowFreq", 100f);      // 100 Hz
        RegisterParameter("LowGain", 0f);        // 0 dB (no boost/cut)
        RegisterParameter("LowQ", 0.707f);       // Bandwidth

        // Mid band
        RegisterParameter("MidFreq", 1000f);     // 1 kHz
        RegisterParameter("MidGain", 0f);        // 0 dB
        RegisterParameter("MidQ", 0.707f);

        // High band (treble)
        RegisterParameter("HighFreq", 10000f);   // 10 kHz
        RegisterParameter("HighGain", 0f);       // 0 dB
        RegisterParameter("HighQ", 0.707f);

        Mix = 1.0f;       // 100% wet
    }

    #region Low Band Properties

    /// <summary>
    /// Low band center frequency in Hz (20 - 500)
    /// </summary>
    public float LowFrequency
    {
        get => GetParameter("LowFreq");
        set => SetParameter("LowFreq", Math.Clamp(value, 20f, 500f));
    }

    /// <summary>
    /// Low band gain in dB (-24 to +24)
    /// </summary>
    public float LowGain
    {
        get => GetParameter("LowGain");
        set => SetParameter("LowGain", Math.Clamp(value, -24f, 24f));
    }

    /// <summary>
    /// Low band Q (bandwidth) (0.1 - 10.0)
    /// </summary>
    public float LowQ
    {
        get => GetParameter("LowQ");
        set => SetParameter("LowQ", Math.Clamp(value, 0.1f, 10f));
    }

    #endregion

    #region Mid Band Properties

    /// <summary>
    /// Mid band center frequency in Hz (200 - 5000)
    /// </summary>
    public float MidFrequency
    {
        get => GetParameter("MidFreq");
        set => SetParameter("MidFreq", Math.Clamp(value, 200f, 5000f));
    }

    /// <summary>
    /// Mid band gain in dB (-24 to +24)
    /// </summary>
    public float MidGain
    {
        get => GetParameter("MidGain");
        set => SetParameter("MidGain", Math.Clamp(value, -24f, 24f));
    }

    /// <summary>
    /// Mid band Q (bandwidth) (0.1 - 10.0)
    /// </summary>
    public float MidQ
    {
        get => GetParameter("MidQ");
        set => SetParameter("MidQ", Math.Clamp(value, 0.1f, 10f));
    }

    #endregion

    #region High Band Properties

    /// <summary>
    /// High band center frequency in Hz (2000 - 20000)
    /// </summary>
    public float HighFrequency
    {
        get => GetParameter("HighFreq");
        set => SetParameter("HighFreq", Math.Clamp(value, 2000f, 20000f));
    }

    /// <summary>
    /// High band gain in dB (-24 to +24)
    /// </summary>
    public float HighGain
    {
        get => GetParameter("HighGain");
        set => SetParameter("HighGain", Math.Clamp(value, -24f, 24f));
    }

    /// <summary>
    /// High band Q (bandwidth) (0.1 - 10.0)
    /// </summary>
    public float HighQ
    {
        get => GetParameter("HighQ");
        set => SetParameter("HighQ", Math.Clamp(value, 0.1f, 10f));
    }

    #endregion

    /// <summary>
    /// Dry/wet mix (0.0 = 100% dry, 1.0 = 100% wet)
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

        // Update filter coefficients for all bands
        UpdateBandCoefficients(_lowBands, LowFrequency, LowGain, LowQ, sampleRate);
        UpdateBandCoefficients(_midBands, MidFrequency, MidGain, MidQ, sampleRate);
        UpdateBandCoefficients(_highBands, HighFrequency, HighGain, HighQ, sampleRate);

        for (int i = 0; i < count; i += channels)
        {
            for (int ch = 0; ch < channels; ch++)
            {
                int index = i + ch;
                float input = sourceBuffer[index];

                // Process through all three bands
                float output = input;
                output = ProcessBand(ref _lowBands[ch], output);
                output = ProcessBand(ref _midBands[ch], output);
                output = ProcessBand(ref _highBands[ch], output);

                destBuffer[offset + index] = output;
            }
        }
    }

    /// <summary>
    /// Update filter coefficients for a band (Cookbook EQ algorithm)
    /// </summary>
    private void UpdateBandCoefficients(EQBand[] bands, float freq, float gainDb, float q, int sampleRate)
    {
        // Convert gain from dB to linear
        float A = MathF.Pow(10f, gainDb / 40f); // sqrt of gain

        // Calculate angular frequency
        float w0 = 2f * MathF.PI * freq / sampleRate;
        float cosW0 = MathF.Cos(w0);
        float sinW0 = MathF.Sin(w0);
        float alpha = sinW0 / (2f * q);

        // Peaking EQ filter coefficients (RBJ Audio EQ Cookbook)
        float b0 = 1f + alpha * A;
        float b1 = -2f * cosW0;
        float b2 = 1f - alpha * A;
        float a0 = 1f + alpha / A;
        float a1 = -2f * cosW0;
        float a2 = 1f - alpha / A;

        // Normalize coefficients
        foreach (ref EQBand band in bands.AsSpan())
        {
            band.b0 = b0 / a0;
            band.b1 = b1 / a0;
            band.b2 = b2 / a0;
            band.a1 = a1 / a0;
            band.a2 = a2 / a0;
        }
    }

    /// <summary>
    /// Process a single sample through a biquad filter
    /// </summary>
    private float ProcessBand(ref EQBand band, float input)
    {
        // Biquad filter (Direct Form II Transposed)
        float output = band.b0 * input + band.z1;
        band.z1 = band.b1 * input - band.a1 * output + band.z2;
        band.z2 = band.b2 * input - band.a2 * output;

        return output;
    }

    /// <summary>
    /// EQ band state (biquad filter)
    /// </summary>
    private struct EQBand
    {
        // Filter coefficients
        public float b0, b1, b2; // Numerator (feedforward)
        public float a1, a2;     // Denominator (feedback) - a0 normalized to 1

        // Filter state
        public float z1, z2;     // Delay line
    }
}
