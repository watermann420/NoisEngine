var synth1 = CreateSynth(); 
var synth2 = CreateSynth();

// Variables for configuration
bool enableMidi = true; // Toggle MIDI mappings
bool startRunning = false; // Variable to control transport directly of the "Pattern"
double initialBpm = 140.0; // Set initial BPM
string targetParam = "cutoff"; // Target parameter for MIDI CC mapping
audio.all.gain(0.5); //Audio gain set to 10%


SetBpm(initialBpm); // Set initial BPM
Start(); // Start the transport


synth1.Waveform = WaveType.Sawtooth; // Set waveform for synth 1
synth1.SetParameter(targetParam, 0.5f); // Set initial cutoff to mid value

if (enableMidi) 
{
    Print("Enabling MIDI Mappings...");
    
    // NEW SIMPLIFIED MIDI API
    midi.device(0).route(synth1);
    
    // Using names (if you know the device name, e.g., "LoopBe Internal MIDI")
    // midi.device("LoopBe").route(synth1);

    // CC Mapping with the new fluent API
    midi.device(0).cc(1).to(synth1, targetParam); // Modulation wheel to cutoff
    midi.device(0).cc(2).to(synth1, targetParam); // CC2 to cutoff
    midi.device(0).cc(7).to(synth1, "resonance"); // CC7 to resonance
    midi.device(0).pitchbend().to(synth1, targetParam); // Pitch bend to cutoff

   // Range Mapping (PlayableKeys - maps MIDI note range to synth)
   midi.playablekeys.range(21, 108).low.to.high.map(synth1);
    
  
}

// Configure Synth 2 with random values
synth2.Waveform = (WaveType)RandomInt(0, 4);
Print($"Synth 2 Waveform set to: {synth2.Waveform}");

// Create a simple pattern on synth 2
var pattern = CreatePattern(synth2);
pattern.Events.Add(new NoteEvent {  Note = 60, Velocity = 100 });






// Control pattern based on startRunning variable
if (startRunning) patterns.start(pattern); // Start pattern if running is enabled
else patterns.stop(pattern); // Ensure pattern is stopped if not starting running



Print("Setup Complete.");
