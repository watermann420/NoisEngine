// ============================================
// Vital Synthesizer VST Script
// Mit Keyboard-Steuerung & Wave/OSC Switching
// ============================================

// === Konfiguration ===
bool enableMidi = true;          // MIDI aktivieren
double initialBpm = 120.0;       // BPM
int midiDeviceIndex = 0;         // MIDI-Geraet Index (0 = erstes Geraet)
audio.all.gain(0.7);             // Master-Lautstaerke auf 70%

// === Oscillator/Wave Mode ===
// Mode: "waves" = Wavetable Oscillators, "osc" = Classic Oscillators
string oscMode = "waves";        // Aktueller Modus ("waves" oder "osc")

SetBpm(initialBpm);
Start();

// === VST Plugins scannen und auflisten ===
Print("Scanning for VST plugins...");
vst.list();

// === Vital laden ===
Print("\nLoading Vital synthesizer...");
var vital = vst.load("Vital");

if (vital == null)
{
    Print("Vital not found by name, trying alternatives...");
    vital = vst.load("vital");
}

// === Wave/OSC Switching Funktionen ===
void SetWavesMode()
{
    oscMode = "waves";
    Print("\n>>> Switched to WAVES mode (Wavetable)");

    if (vital != null)
    {
        // Vital Wavetable Mode - aktiviere Wavetable OSCs
        // OSC 1: Wavetable Position aktivieren
        vital.param("osc_1_wave_frame", 0.0f);      // Wavetable Frame Position
        vital.param("osc_1_spectral_morph", 0.0f); // Spectral Morphing
        vital.param("osc_1_distortion_type", 0.0f); // Distortion off
        vital.param("osc_1_level", 0.8f);           // OSC 1 Level

        // OSC 2: Zweite Wavetable Layer
        vital.param("osc_2_wave_frame", 0.5f);      // Andere Position
        vital.param("osc_2_level", 0.4f);           // Leiser als OSC 1
        vital.param("osc_2_transpose", 0.0f);       // Keine Transposition
        vital.param("osc_2_tune", 0.05f);           // Leichtes Detune

        // Filter fuer Waves - etwas offener
        vital.param("filter_1_cutoff", 0.7f);
        vital.param("filter_1_resonance", 0.3f);

        Print("  OSC 1: Wavetable Primary");
        Print("  OSC 2: Wavetable Layer (detuned)");
        Print("  Filter: Open, mild resonance");
    }
}

void SetOscMode()
{
    oscMode = "osc";
    Print("\n>>> Switched to OSC mode (Classic)");

    if (vital != null)
    {
        // Vital Classic Oscillator Mode
        // OSC 1: Basic Waveform (Saw)
        vital.param("osc_1_wave_frame", 0.0f);      // Basic wave
        vital.param("osc_1_spectral_morph", 0.0f);  // No morphing
        vital.param("osc_1_distortion_type", 0.0f); // Clean
        vital.param("osc_1_level", 1.0f);           // Full level
        vital.param("osc_1_unison_voices", 0.2f);   // Some unison
        vital.param("osc_1_unison_detune", 0.3f);   // Detune spread

        // OSC 2: Sub Oscillator style
        vital.param("osc_2_wave_frame", 0.0f);      // Basic wave
        vital.param("osc_2_level", 0.5f);           // Sub level
        vital.param("osc_2_transpose", -12.0f);     // Octave down (sub)
        vital.param("osc_2_tune", 0.0f);            // No detune

        // Filter fuer OSC - klassischer Sound
        vital.param("filter_1_cutoff", 0.5f);
        vital.param("filter_1_resonance", 0.4f);

        Print("  OSC 1: Saw with Unison");
        Print("  OSC 2: Sub (-1 Octave)");
        Print("  Filter: Classic subtractive");
    }
}

void ToggleOscMode()
{
    if (oscMode == "waves")
        SetOscMode();
    else
        SetWavesMode();
}

// === Zusaetzliche Sound-Presets ===
void SetPadSound()
{
    Print("\n>>> PAD Sound");
    if (vital != null)
    {
        vital.param("osc_1_level", 0.6f);
        vital.param("osc_2_level", 0.6f);
        vital.param("osc_1_unison_voices", 0.5f);
        vital.param("osc_1_unison_detune", 0.4f);
        vital.param("filter_1_cutoff", 0.4f);
        vital.param("filter_1_resonance", 0.2f);
        // Attack langsam fuer Pad
        vital.param("env_1_attack", 0.4f);
        vital.param("env_1_release", 0.6f);
    }
}

void SetLeadSound()
{
    Print("\n>>> LEAD Sound");
    if (vital != null)
    {
        vital.param("osc_1_level", 1.0f);
        vital.param("osc_2_level", 0.3f);
        vital.param("osc_1_unison_voices", 0.3f);
        vital.param("osc_1_unison_detune", 0.2f);
        vital.param("filter_1_cutoff", 0.6f);
        vital.param("filter_1_resonance", 0.5f);
        // Schneller Attack fuer Lead
        vital.param("env_1_attack", 0.0f);
        vital.param("env_1_release", 0.3f);
    }
}

void SetBassSound()
{
    Print("\n>>> BASS Sound");
    if (vital != null)
    {
        vital.param("osc_1_level", 0.8f);
        vital.param("osc_2_level", 0.7f);
        vital.param("osc_2_transpose", -12.0f);     // Sub
        vital.param("osc_1_unison_voices", 0.1f);
        vital.param("filter_1_cutoff", 0.35f);
        vital.param("filter_1_resonance", 0.3f);
        vital.param("env_1_attack", 0.0f);
        vital.param("env_1_decay", 0.3f);
        vital.param("env_1_sustain", 0.7f);
        vital.param("env_1_release", 0.2f);
    }
}

// === Main Setup ===
if (vital != null)
{
    Print("Vital loaded successfully!");

    // === MIDI Keyboard Routing ===
    if (enableMidi)
    {
        Print("\nConfiguring MIDI routing...");

        vital.from(midiDeviceIndex);
        Print($"MIDI device {midiDeviceIndex} routed to Vital");

        // Volle Keyboard-Range (A0 bis C8)
        midi.playablekeys.range(21, 108).low.to.high.map(vital.Plugin);
        Print("Full keyboard range (A0-C8) mapped");

        // === MIDI CC Mappings ===
        midi.device(midiDeviceIndex).cc(1).to(vital.Plugin, "macro1");      // Mod Wheel
        midi.device(midiDeviceIndex).cc(11).to(vital.Plugin, "cutoff");     // Expression
        midi.device(midiDeviceIndex).cc(74).to(vital.Plugin, "filter_1_cutoff"); // Filter Cutoff (Standard)
        midi.device(midiDeviceIndex).cc(71).to(vital.Plugin, "filter_1_resonance"); // Resonance
        midi.device(midiDeviceIndex).pitchbend().to(vital.Plugin, "pitchbend");

        Print("CC mappings configured:");
        Print("  - CC1  (Mod Wheel)  -> Macro 1");
        Print("  - CC11 (Expression) -> Cutoff");
        Print("  - CC74 (Brightness) -> Filter Cutoff");
        Print("  - CC71 (Timbre)     -> Filter Resonance");
        Print("  - Pitch Bend        -> Pitch");
    }

    vital.volume(0.8f);

    // Starte im WAVES Modus
    SetWavesMode();

    Print("\n=== Vital is ready! ===");
}
else
{
    // Fallback: SimpleSynth
    Print("\nFalling back to built-in SimpleSynth...");

    var synth = CreateSynth();
    synth.Waveform = WaveType.Sawtooth;
    synth.SetParameter("cutoff", 0.6f);
    synth.SetParameter("resonance", 0.3f);

    if (enableMidi)
    {
        midi.device(midiDeviceIndex).route(synth);
        midi.playablekeys.range(21, 108).low.to.high.map(synth);
        midi.device(midiDeviceIndex).cc(1).to(synth, "cutoff");
        midi.device(midiDeviceIndex).pitchbend().to(synth, "pitchbend");
        Print("SimpleSynth configured with MIDI");
    }
}

// === Hilfe und Befehle ===
Print("\n========================================");
Print("         QUICK COMMANDS");
Print("========================================");
Print("");
Print("--- Mode Switching ---");
Print("SetWavesMode()    - Wavetable Oscillators");
Print("SetOscMode()      - Classic Oscillators");
Print("ToggleOscMode()   - Toggle between modes");
Print("");
Print("--- Sound Presets ---");
Print("SetPadSound()     - Ambient Pad");
Print("SetLeadSound()    - Cutting Lead");
Print("SetBassSound()    - Deep Bass");
Print("");
Print("--- VST Control ---");
Print("vst.list()        - Show all plugins");
Print("vital.noteOn(60,100) - Test note");
Print("vital.allNotesOff()  - Panic");
Print("");
Print($"Current Mode: {oscMode.ToUpper()}");
Print("========================================");

Print("\nSetup Complete.");
