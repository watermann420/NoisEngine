// ============================================
// Vital Synthesizer VST Script
// Mit Keyboard-Steuerung
// ============================================

// === Konfiguration ===
bool enableMidi = true;          // MIDI aktivieren
bool enableKeyboard = true;      // Computer-Keyboard aktivieren (falls unterstuetzt)
double initialBpm = 120.0;       // BPM
int midiDeviceIndex = 0;         // MIDI-Geraet Index (0 = erstes Geraet)
audio.all.gain(0.7);             // Master-Lautstaerke auf 70%

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
    // Falls "Vital" nicht gefunden, versuche alternative Namen
    Print("Vital not found by name, trying to find it...");
    vital = vst.load("vital");

    if (vital == null)
    {
        // Zeige verfuegbare Plugins und lade manuell
        Print("Could not auto-load Vital. Please check the VST plugin list above.");
        Print("You can manually load by index: vst.load(INDEX_NUMBER)");
    }
}

if (vital != null)
{
    Print("Vital loaded successfully!");

    // === MIDI Keyboard Routing ===
    if (enableMidi)
    {
        Print("\nConfiguring MIDI routing...");

        // Route MIDI-Geraet zu Vital
        vital.from(midiDeviceIndex);
        Print($"MIDI device {midiDeviceIndex} routed to Vital");

        // Volle Keyboard-Range (A0 bis C8)
        midi.playablekeys.range(21, 108).low.to.high.map(vital.Plugin);
        Print("Full keyboard range (A0-C8) mapped");

        // === MIDI CC Mappings fuer Vital ===
        // Modulation Wheel (CC1) -> Macro 1
        midi.device(midiDeviceIndex).cc(1).to(vital.Plugin, "macro1");

        // Expression (CC11) -> Filter Cutoff
        midi.device(midiDeviceIndex).cc(11).to(vital.Plugin, "cutoff");

        // Sustain Pedal (CC64) wird automatisch vom Plugin verarbeitet

        // Pitch Bend
        midi.device(midiDeviceIndex).pitchbend().to(vital.Plugin, "pitchbend");

        Print("CC mappings configured:");
        Print("  - CC1 (Mod Wheel) -> Macro 1");
        Print("  - CC11 (Expression) -> Filter Cutoff");
        Print("  - Pitch Bend -> Pitch");
    }

    // === Initial Preset/Sound konfigurieren ===
    // Setze initiale Parameter (optional)
    vital.volume(0.8f);  // 80% Volume

    Print("\n=== Vital is ready! ===");
    Print("Play your MIDI keyboard to make sound!");
}
else
{
    // Fallback: Verwende den eingebauten SimpleSynth
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

// === Hilfreiche Funktionen ===
Print("\n=== Quick Commands ===");
Print("vst.list()           - Show all VST plugins");
Print("vst.loaded()         - Show loaded plugins");
Print("vital.noteOn(60,100) - Test note C4");
Print("vital.noteOff(60)    - Stop note");
Print("vital.allNotesOff()  - Panic/Stop all");

Print("\nSetup Complete.");
