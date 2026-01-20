// ============================================================================
// 11 - AUDIO EFFECTS
// ============================================================================
// This script demonstrates all audio effects available in MusicEngine
// SYNTAX TO REVIEW: Effect creation, parameter control, effect chaining
// ============================================================================

Print("");
Print("=== AUDIO EFFECTS TEST ===");
Print("");

// ============================================================================
// 1. FILTER EFFECTS
// ============================================================================
Print("1. Filter effects:");

var synth1 = CreateSynth("filter-test");
var pattern1 = CreatePattern(synth1, "test-pattern");
pattern1.AddNote(0.0, 60, 100, 0.5);
pattern1.AddNote(1.0, 64, 100, 0.5);
pattern1.AddNote(2.0, 67, 100, 0.5);
pattern1.AddNote(3.0, 72, 100, 1.0);
pattern1.Loop = true;

// Create lowpass filter
var filter = fx.Filter(synth1, "lowpass")
    .Cutoff(500)
    .Resonance(2.0)
    .Type(FilterType.Lowpass)
    .DryWet(1.0)
    .Build();

Print("   Created lowpass filter:");
Print($"   - Cutoff: {filter.Cutoff} Hz");
Print($"   - Resonance: {filter.Resonance}");
Print($"   - Type: {filter.Type}");
Print("");

// ============================================================================
// 2. PARAMETRIC EQ
// ============================================================================
Print("2. Parametric EQ:");

var synth2 = CreateSynth("eq-test");

var eq = fx.EQ(synth2, "vocal-eq")
    .Low(100, 6, 0.707)      // Boost bass by 6dB
    .Mid(1000, 0, 0.707)     // No change
    .High(8000, -3, 0.707)   // Cut treble by 3dB
    .DryWet(1.0)
    .Build();

Print("   Created 3-band EQ:");
Print($"   - Low: {eq.LowFrequency} Hz, {eq.LowGain} dB");
Print($"   - Mid: {eq.MidFrequency} Hz, {eq.MidGain} dB");
Print($"   - High: {eq.HighFrequency} Hz, {eq.HighGain} dB");
Print("");

// ============================================================================
// 3. COMPRESSOR
// ============================================================================
Print("3. Compressor:");

var drums = CreateSampleInstrument("drums");

var comp = fx.Compressor(drums, "drum-comp")
    .Threshold(-20)
    .Ratio(4)
    .Attack(0.005)
    .Release(0.1)
    .MakeupGain(6)
    .Knee(6)
    .DryWet(1.0)
    .Build();

Print("   Created compressor:");
Print($"   - Threshold: {comp.Threshold} dB");
Print($"   - Ratio: {comp.Ratio}:1");
Print($"   - Attack: {comp.Attack * 1000} ms");
Print($"   - Release: {comp.Release * 1000} ms");
Print($"   - Makeup Gain: {comp.MakeupGain} dB");
Print("");

// ============================================================================
// 4. LIMITER
// ============================================================================
Print("4. Limiter:");

var synth3 = CreateSynth("limiter-test");

var limiter = fx.Limiter(synth3, "safety-limiter")
    .Ceiling(-0.3)
    .Release(0.05)
    .Lookahead(0.005)
    .DryWet(1.0)
    .Build();

Print("   Created limiter:");
Print($"   - Ceiling: {limiter.Ceiling} dB");
Print($"   - Release: {limiter.Release * 1000} ms");
Print($"   - Lookahead: {limiter.Lookahead * 1000} ms");
Print("");

// ============================================================================
// 5. NOISE GATE
// ============================================================================
Print("5. Noise gate:");

var vocal = CreateSynth("vocal");

var gate = fx.Gate(vocal, "vocal-gate")
    .Threshold(-40)
    .Ratio(10)
    .Attack(0.001)
    .Hold(0.05)
    .Release(0.2)
    .Range(-60)
    .DryWet(1.0)
    .Build();

Print("   Created noise gate:");
Print($"   - Threshold: {gate.Threshold} dB");
Print($"   - Ratio: {gate.Ratio}:1");
Print($"   - Hold: {gate.Hold * 1000} ms");
Print($"   - Range: {gate.Range} dB");
Print("");

// ============================================================================
// 6. DELAY
// ============================================================================
Print("6. Delay effect:");

var lead = CreateSynth("lead");

var delay = fx.Delay(lead, "lead-delay")
    .Time(0.375)         // Dotted eighth at 120 BPM
    .Feedback(0.5)
    .CrossFeedback(0.3)
    .Damping(0.5)
    .StereoSpread(0.2)
    .PingPong(0.8)
    .DryWet(0.4)
    .Build();

Print("   Created delay effect:");
Print($"   - Time: {delay.DelayTime * 1000} ms");
Print($"   - Feedback: {delay.Feedback * 100}%");
Print($"   - Ping-Pong: {delay.PingPong * 100}%");
Print($"   - Dry/Wet: {delay.DryWet * 100}% wet");
Print("");

// ============================================================================
// 7. REVERB
// ============================================================================
Print("7. Reverb effect:");

var pad = CreateSynth("pad");

var reverb = fx.Reverb(pad, "room-reverb")
    .RoomSize(0.7)
    .Damping(0.5)
    .Width(1.0)
    .EarlyLevel(0.3)
    .LateLevel(0.7)
    .Predelay(0.0)
    .DryWet(0.3)
    .Build();

Print("   Created reverb effect:");
Print($"   - Room Size: {reverb.RoomSize * 100}%");
Print($"   - Damping: {reverb.Damping * 100}%");
Print($"   - Early/Late: {reverb.EarlyLevel}/{reverb.LateLevel}");
Print($"   - Dry/Wet: {reverb.DryWet * 100}% wet");
Print("");

// ============================================================================
// 8. CHORUS
// ============================================================================
Print("8. Chorus effect:");

var synth4 = CreateSynth("chorus-test");

var chorus = fx.Chorus(synth4, "lush-chorus")
    .Rate(0.8)
    .Depth(0.003)
    .BaseDelay(0.02)
    .Voices(3)
    .Spread(0.5)
    .Feedback(0.2)
    .DryWet(0.5)
    .Build();

Print("   Created chorus effect:");
Print($"   - Rate: {chorus.Rate} Hz");
Print($"   - Voices: {chorus.Voices}");
Print($"   - Spread: {chorus.Spread * 100}%");
Print($"   - Dry/Wet: {chorus.DryWet * 100}% wet");
Print("");

// ============================================================================
// 9. FLANGER
// ============================================================================
Print("9. Flanger effect:");

var synth5 = CreateSynth("flanger-test");

var flanger = fx.Flanger(synth5, "jet-flanger")
    .Rate(0.3)
    .Depth(0.005)
    .Feedback(0.7)
    .BaseDelay(0.003)
    .Stereo(0.5)
    .DryWet(0.5)
    .Build();

Print("   Created flanger effect:");
Print($"   - Rate: {flanger.Rate} Hz");
Print($"   - Depth: {flanger.Depth * 1000} ms");
Print($"   - Feedback: {flanger.Feedback * 100}%");
Print("");

// ============================================================================
// 10. PHASER
// ============================================================================
Print("10. Phaser effect:");

var synth6 = CreateSynth("phaser-test");

var phaser = fx.Phaser(synth6, "sweep-phaser")
    .Rate(0.5)
    .Depth(1.0)
    .Feedback(0.7)
    .MinFrequency(200)
    .MaxFrequency(2000)
    .Stages(6)
    .Stereo(0.5)
    .DryWet(0.5)
    .Build();

Print("   Created phaser effect:");
Print($"   - Rate: {phaser.Rate} Hz");
Print($"   - Frequency Range: {phaser.MinFrequency}-{phaser.MaxFrequency} Hz");
Print($"   - Stages: {phaser.Stages}");
Print("");

// ============================================================================
// 11. TREMOLO
// ============================================================================
Print("11. Tremolo effect:");

var organ = CreateSynth("organ");

var tremolo = fx.Tremolo(organ, "organ-tremolo")
    .Rate(5)
    .Depth(0.5)
    .Waveform(0)  // Sine wave
    .Stereo(0)
    .DryWet(1.0)
    .Build();

Print("   Created tremolo effect:");
Print($"   - Rate: {tremolo.Rate} Hz");
Print($"   - Depth: {tremolo.Depth * 100}%");
Print($"   - Waveform: {tremolo.Waveform} (0=sine, 1=tri, 2=square)");
Print("");

// ============================================================================
// 12. VIBRATO
// ============================================================================
Print("12. Vibrato effect:");

var string = CreateSynth("string");

var vibrato = fx.Vibrato(string, "string-vibrato")
    .Rate(5)
    .Depth(0.002)
    .BaseDelay(0.003)
    .Waveform(0)
    .DryWet(1.0)
    .Build();

Print("   Created vibrato effect:");
Print($"   - Rate: {vibrato.Rate} Hz");
Print($"   - Depth: {vibrato.Depth * 1000} ms");
Print("");

// ============================================================================
// 13. DISTORTION
// ============================================================================
Print("13. Distortion effect:");

var bass = CreateSynth("bass");

var dist = fx.Distortion(bass, "bass-dist")
    .Drive(10)
    .Tone(0.6)
    .OutputGain(0.5)
    .Type(DistortionType.Overdrive)
    .DryWet(1.0)
    .Build();

Print("   Created distortion effect:");
Print($"   - Drive: {dist.Drive}");
Print($"   - Tone: {dist.Tone * 100}%");
Print($"   - Type: {dist.Type}");
Print("");

// ============================================================================
// 14. BITCRUSHER
// ============================================================================
Print("14. Bitcrusher effect:");

var synth7 = CreateSynth("lofi");

var crusher = fx.Bitcrusher(synth7, "lofi-crusher")
    .BitDepth(8)
    .TargetSampleRate(8000)
    .DryWet(0.7)
    .Build();

Print("   Created bitcrusher effect:");
Print($"   - Bit Depth: {crusher.BitDepth} bits");
Print($"   - Target Sample Rate: {crusher.TargetSampleRate} Hz");
Print("");

// ============================================================================
// 15. SIDE-CHAIN COMPRESSION
// ============================================================================
Print("15. Side-chain compression:");

var music = CreateSynth("music");
var vocalTrigger = CreateSynth("vocal-trigger");

var sidechain = fx.SideChainCompressor(music, "ducking")
    .SideChain(vocalTrigger)
    .Threshold(-20)
    .Ratio(6)
    .Attack(0.01)
    .Release(0.3)
    .DryWet(1.0)
    .Build();

Print("   Created side-chain compressor:");
Print($"   - Threshold: {sidechain.Threshold} dB");
Print($"   - Ratio: {sidechain.Ratio}:1");
Print("   → Music ducks when vocal plays");
Print("");

// ============================================================================
// 16. EFFECT CHAINING EXAMPLE
// ============================================================================
Print("16. Effect chaining example:");

var synthChain = CreateSynth("chain-test");

// Chain multiple effects
var filtered = fx.Filter(synthChain, "chain-filter")
    .Cutoff(1000)
    .Resonance(2)
    .Type(FilterType.Lowpass)
    .Build();

var chorused = fx.Chorus(filtered, "chain-chorus")
    .Rate(0.8)
    .Voices(3)
    .Build();

var reverbed = fx.Reverb(chorused, "chain-reverb")
    .RoomSize(0.7)
    .DryWet(0.4)
    .Build();

Print("   Created effect chain:");
Print("   Synth → Filter → Chorus → Reverb");
Print("");

// ============================================================================
// 17. PRACTICAL EXAMPLE - MIXING SETUP
// ============================================================================
Print("17. Practical mixing example:");

// Create instruments
var kick = CreateSampleInstrument("kick");
var snare = CreateSampleInstrument("snare");
var hihat = CreateSampleInstrument("hihat");

// Add compression to drums
var kickComp = fx.Compressor(kick, "kick-comp")
    .Threshold(-15)
    .Ratio(6)
    .Attack(0.001)
    .Release(0.05)
    .Build();

var snareComp = fx.Compressor(snare, "snare-comp")
    .Threshold(-12)
    .Ratio(4)
    .Attack(0.003)
    .Release(0.08)
    .Build();

// Add EQ to hihat
var hihatEQ = fx.EQ(hihat, "hihat-eq")
    .High(8000, 3, 0.707)  // Boost highs for sparkle
    .Build();

Print("   Created complete drum mix:");
Print("   - Kick: compressed (6:1)");
Print("   - Snare: compressed (4:1)");
Print("   - Hihat: EQ boosted at 8kHz");
Print("");

// ============================================================================
// 18. EFFECT PARAMETER AUTOMATION
// ============================================================================
Print("18. Effect parameter automation:");

var autoSynth = CreateSynth("auto-test");

var autoFilter = fx.Filter(autoSynth, "auto-filter")
    .Cutoff(200)
    .Resonance(5.0)
    .Type(FilterType.Lowpass)
    .Build();

Print("   Filter automation example:");
Print("   // Sweep filter cutoff over time:");
Print("   for (int i = 0; i < 100; i++) {");
Print("       autoFilter.Cutoff = 200 + i * 20;  // 200 Hz → 2200 Hz");
Print("       await Task.Delay(50);");
Print("   }");
Print("");

// ============================================================================
// 19. DELAY TIME CALCULATION HELPER
// ============================================================================
Print("19. Delay time calculation:");

float bpm = 120;
float quarter = 60f / bpm;
float eighth = quarter / 2f;
float dottedEighth = eighth * 1.5f;

Print($"   At {bpm} BPM:");
Print($"   - Quarter note: {quarter:F3} seconds");
Print($"   - Eighth note: {eighth:F3} seconds");
Print($"   - Dotted eighth: {dottedEighth:F3} seconds");
Print("");

// ============================================================================
// 20. EFFECT TYPES SUMMARY
// ============================================================================
Print("20. Available effect types:");

Print("   FILTERS:");
Print("   - FilterEffect (Lowpass, Highpass, Bandpass, Bandreject, Allpass)");
Print("   - ParametricEQEffect (3-band EQ)");
Print("");

Print("   DYNAMICS:");
Print("   - CompressorEffect");
Print("   - LimiterEffect");
Print("   - GateEffect");
Print("   - SideChainCompressorEffect");
Print("");

Print("   MODULATION:");
Print("   - ChorusEffect");
Print("   - FlangerEffect");
Print("   - PhaserEffect");
Print("   - TremoloEffect");
Print("   - VibratoEffect");
Print("");

Print("   TIME-BASED:");
Print("   - EnhancedDelayEffect");
Print("   - EnhancedReverbEffect");
Print("");

Print("   DISTORTION:");
Print("   - DistortionEffect (HardClip, SoftClip, Overdrive, Fuzz, Waveshaper)");
Print("   - BitcrusherEffect");
Print("");

Print("=== AUDIO EFFECTS TEST COMPLETED ===");

// ============================================================================
// NOTES:
// ============================================================================
// - All effects support DryWet mixing (0.0 = dry, 1.0 = wet)
// - Effects can be chained by passing one effect as input to another
// - Use fx.EffectType() to create effects with fluent API
// - See EFFECTS_REFERENCE.md for complete parameter documentation
// - See AUDIO_ROUTING.md for routing and bus examples
// ============================================================================
