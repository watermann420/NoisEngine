# MusicEngine Scripting API Documentation

This documentation describes the complete scripting API of the MusicEngine. The scripting environment is based on C# and enables control of the audio engine, sequencer, MIDI devices, VST plugins, and sample instruments.

---

## Table of Contents

1. [Basic Variables](#1-basic-variables)
2. [Synth Creation](#2-synth-creation)
3. [Sample Instrument API](#3-sample-instrument-api)
4. [Transport Controls](#4-transport-controls)
5. [Pattern Control](#5-pattern-control)
6. [MIDI Fluent API](#6-midi-fluent-api)
7. [Audio Controls](#7-audio-controls)
8. [VST Plugin API](#8-vst-plugin-api)
9. [Sample Fluent API](#9-sample-fluent-api)
10. [Helper Functions](#10-helper-functions)
11. [Frequency Triggers](#11-frequency-triggers)
12. [Virtual Audio Channels](#12-virtual-audio-channels)

---

## 1. Basic Variables

The scripting environment provides two central objects that are available in every script:

| Variable | Type | Description |
|----------|------|-------------|
| `Engine` / `engine` | `AudioEngine` | The audio engine instance for sound output and MIDI routing |
| `Sequencer` / `sequencer` | `Sequencer` | The sequencer for pattern playback and timing |

### Example

```csharp
// Both notations are equivalent
engine.AddSampleProvider(synth);
Engine.AddSampleProvider(synth);

// Start sequencer
sequencer.Start();
Sequencer.Start();
```

---

## 2. Synth Creation

### CreateSynth

**Aliases:** `CreateSynth()`, `synth()`, `s()`, `newSynth()`

Creates a new `SimpleSynth` and automatically adds it to the audio engine.

```csharp
SimpleSynth CreateSynth()
```

**Returns:** A new `SimpleSynth` instance

### CreatePattern

**Aliases:** `CreatePattern(synth)`

Creates a new `Pattern` for a synthesizer and adds it to the sequencer.

```csharp
Pattern CreatePattern(ISynth synth)
```

**Parameters:**
- `synth` - The synthesizer that is controlled by the pattern

**Returns:** A new `Pattern` instance

### Example

```csharp
// Create synth and add pattern
var synth = CreateSynth();
var pattern = CreatePattern(synth);

// Fill pattern with notes
pattern.AddNote(0, 60, 100, 0.5);    // Beat 0: C4
pattern.AddNote(1, 64, 100, 0.5);    // Beat 1: E4
pattern.AddNote(2, 67, 100, 0.5);    // Beat 2: G4
pattern.AddNote(3, 72, 100, 0.5);    // Beat 3: C5

// Start sequencer
SetBpm(120);
Start();
```

---

## 3. Sample Instrument API

The Sample Instrument API enables the creation of sample-based synthesizers.

### CreateSampler

**Aliases:** `CreateSampler(name)`, `samples.create(name)`

Creates a new `SampleInstrument` and adds it to the audio engine.

```csharp
SampleInstrument CreateSampler(string? name = null)
```

**Parameters:**
- `name` (optional) - Name for the sampler

**Returns:** A new `SampleInstrument` instance

### CreateSamplerFromFile

**Aliases:** `CreateSamplerFromFile(filePath, rootNote)`, `samples.load(filePath, rootNote)`

Creates a sampler and loads a single audio file. The sample is mapped to all notes with pitch-shifting from the root note.

```csharp
SampleInstrument CreateSamplerFromFile(string filePath, int rootNote = 60)
```

**Parameters:**
- `filePath` - Path to the audio file (WAV, MP3, etc.)
- `rootNote` (optional) - MIDI note at which the sample is played unchanged (default: 60 = C4)

**Returns:** A new `SampleInstrument` instance

### CreateSamplerFromDirectory

**Aliases:** `CreateSamplerFromDirectory(directoryPath)`, `samples.fromDirectory(directoryPath)`

Creates a sampler and loads all samples from a directory.

```csharp
SampleInstrument CreateSamplerFromDirectory(string directoryPath)
```

**Parameters:**
- `directoryPath` - Path to the sample directory

**Returns:** A new `SampleInstrument` instance

### LoadSampleToNote

**Aliases:** `LoadSampleToNote(sampler, filePath, note)`

Loads a sample into an existing sampler and maps it to a specific note. Ideal for drum pads.

```csharp
Sample? LoadSampleToNote(SampleInstrument sampler, string filePath, int note)
```

**Parameters:**
- `sampler` - The target sampler
- `filePath` - Path to the audio file
- `note` - MIDI note to which the sample is mapped

**Returns:** The loaded `Sample` or `null` on error

### Example

```csharp
// Load simple piano sample
var piano = CreateSamplerFromFile("C:/Samples/piano_c4.wav", 60);
var pianoPattern = CreatePattern(piano);

// Create drum kit from individual samples
var drums = CreateSampler("DrumKit");
LoadSampleToNote(drums, "C:/Samples/kick.wav", 36);
LoadSampleToNote(drums, "C:/Samples/snare.wav", 38);
LoadSampleToNote(drums, "C:/Samples/hihat.wav", 42);

// Load sampler from directory
var strings = CreateSamplerFromDirectory("C:/Samples/Strings/");

SetBpm(120);
Start();
```

---

## 4. Transport Controls

Functions for controlling playback.

### Start

**Aliases:** `Start()`, `play()`, `run()`, `go()`

Starts the sequencer.

```csharp
void Start()
```

### Stop

**Aliases:** `Stop()`, `pause()`, `halt()`

Stops the sequencer.

```csharp
void Stop()
```

### SetBpm

**Aliases:** `SetBpm(bpm)`, `SetBPM(bpm)`, `bpm(bpm)`, `tempo(bpm)`

Sets the tempo in beats per minute.

```csharp
void SetBpm(double bpm)
```

**Parameters:**
- `bpm` - Tempo in BPM (e.g. 120.0)

### Skip

**Aliases:** `Skip(beats)`, `jump(beats)`, `seek(beats)`

Skips forward or backward by a specific number of beats.

```csharp
void Skip(double beats)
```

**Parameters:**
- `beats` - Number of beats (positive = forward, negative = backward)

### SetScratching

**Aliases:** `SetScratching(scratching)`

Enables or disables scratching mode.

```csharp
void SetScratching(bool scratching)
```

### Example

```csharp
SetBpm(140);
Start();

// Skip forward 8 beats
Skip(8);

// Back to the beginning
Skip(-8);

Stop();
```

---

## 5. Pattern Control

Functions for controlling individual patterns.

### StartPattern

**Aliases:** `StartPattern(pattern)`, `patterns.start(pattern)`

Starts a specific pattern.

```csharp
void StartPattern(Pattern p)
```

### StopPattern

**Aliases:** `StopPattern(pattern)`, `patterns.stop(pattern)`

Stops a specific pattern.

```csharp
void StopPattern(Pattern p)
```

### patterns Object

**Aliases:** `patterns.start(pattern)`, `patterns.stop(pattern)`, `patterns.toggle(pattern)`

Access to extended pattern control:

```csharp
patterns.start(pattern)   // Start pattern
patterns.stop(pattern)    // Stop pattern
patterns.toggle(pattern)  // Toggle pattern on/off
```

### Example

```csharp
var synth1 = CreateSynth();
var synth2 = CreateSynth();

var melody = CreatePattern(synth1);
var bass = CreatePattern(synth2);

// Start only melody
StartPattern(melody);
Start();

// Later: add bass
patterns.start(bass);

// Toggle pattern
patterns.toggle(melody);
```

---

## 6. MIDI Fluent API

The MIDI API provides a fluent syntax for MIDI configuration.

### midi Object

**Aliases:** `midi.device(index)`, `midi.input(index)` (for input devices), `midi.output(index)` (for output devices)

Main entry point for all MIDI operations.

### Device Access

**Aliases:** `midi.device(0)` or `midi.input(0)` for MIDI input; `midi.output(0)` for MIDI output

```csharp
// By index
midi.device(0)                    // First MIDI device
midi.input(0)                     // Alias for device()

// By name
midi.device("Akai MPK")           // Device by name
midi.input("Arturia KeyLab")      // Alias for device()
```

### MIDI Routing

**Aliases:** `midi.device(index).route(synth)`

```csharp
// Route MIDI to synth
midi.device(0).route(synth)       // All notes from device to synth
```

### Control Change (CC) Mapping

**Aliases:** `midi.device(index).cc(number).to(synth, parameter)`

```csharp
// Map CC to parameter
midi.device(0).cc(1).to(synth, "filterCutoff")   // CC1 (Modwheel) to filter
midi.device(0).cc(7).to(synth, "volume")         // CC7 to volume
midi.device(0).cc(74).to(synth, "filterRes")     // CC74 to resonance
```

### Pitch Bend Mapping

**Aliases:** `midi.device(index).pitchbend().to(synth, parameter)`

```csharp
// Map pitch bend to parameter
midi.device(0).pitchbend().to(synth, "pitch")
```

### Playable Keys (Key Ranges)

**Aliases:** `midi.playablekeys.range(start, end).map(synth)`, `midi.playablekeys.range(start, end).from(index).map(synth)`

```csharp
// Map key range to synth
midi.playablekeys.range(21, 108).map(synth)              // Full piano range
midi.playablekeys.range(36, 60).from(0).map(synth)       // Lower half
midi.playablekeys.range(60, 84).from("Akai").map(synth)  // Upper half

// With direction specification
midi.playablekeys.range(21, 108).low.to.high.map(synth)  // Normal (default)
midi.playablekeys.range(21, 108).high.to.low.map(synth)  // Reversed

// Alternative syntax
midi.playablekeys.range(21, 108).low_to_high().map(synth)
midi.playablekeys.range(21, 108).high_to_low().map(synth)
```

### MIDI Output

**Aliases:** `midi.output(index).noteOn(note, velocity, channel)`, `midi.output(index).noteOff(note, channel)`, `midi.output(index).cc(controller, value, channel)`

```csharp
// MIDI output by index or name
midi.output(0).noteOn(60, 100)      // Note on (note, velocity)
midi.output(0).noteOff(60)          // Note off
midi.output(0).cc(1, 64)            // Send control change

// With channel
midi.output(0).noteOn(60, 100, 1)   // Channel 1 (0-based)
midi.output("External Synth").cc(7, 100, 0)  // By name
```

### Example

```csharp
var lead = CreateSynth();
var bass = CreateSynth();

// Split keyboard: lower half = bass, upper = lead
midi.playablekeys.range(21, 59).from(0).map(bass);
midi.playablekeys.range(60, 108).from(0).map(lead);

// Map controllers
midi.device(0).cc(1).to(lead, "filterCutoff");
midi.device(0).cc(74).to(lead, "filterRes");
midi.device(0).pitchbend().to(lead, "pitch");

// Simple routing for second keyboard
midi.device(1).route(bass);

Start();
```

---

## 7. Audio Controls

Functions for controlling audio output.

### audio Object

**Aliases:** `audio.channel(index).gain(value)`, `audio.all.gain(value)`, `audio.input(index).onFrequency(low, high).threshold(value).trigger(action)`

Main entry point for audio control.

### Channel Volume

**Aliases:** `audio.channel(index).gain(value)`, `audio.all.gain(value)`

```csharp
// Single channel
audio.channel(0).gain(0.8)    // Channel 0 to 80%
audio.channel(1).gain(1.2)    // Channel 1 to 120%

// All channels
audio.all.gain(0.5)           // Master volume to 50%
```

### Audio Input (Frequency Triggers)

**Aliases:** `audio.input(index).onFrequency(low, high).threshold(value).trigger(action)`, `AddFrequencyTrigger(deviceIndex, low, high, threshold, action)`

```csharp
// Monitor frequency range
audio.input(0).onFrequency(100, 200)          // 100-200 Hz
    .threshold(0.3)                            // Threshold
    .trigger(synth, 36, 100);                  // Trigger note 36

// With custom action
audio.input(0).onFrequency(80, 120)
    .threshold(0.5)
    .trigger(magnitude => {
        Print($"Kick detected! Strength: {magnitude}");
    });
```

### Example

```csharp
var synth = CreateSynth();
var pattern = CreatePattern(synth);

// Set volume balance
audio.channel(0).gain(0.8);
audio.channel(1).gain(1.0);

// Master to 70%
audio.all.gain(0.7);

Start();
```

---

## 8. VST Plugin API

Functions for loading and controlling VST plugins.

### vst Object

**Aliases:** `vst.load(name)`, `vst.get(name)`, `vst.plugin(name)`, `vst.list()`, `vst.loaded()`

Main entry point for VST operations.

### Plugin Management

**Aliases:** `vst.list()` or `ListVstPlugins()`; `vst.loaded()` or `ListLoadedVstPlugins()`

```csharp
// List available plugins
vst.list()                    // Output all discovered plugins
vst.loaded()                  // Output all loaded plugins
```

### Load Plugin

**Aliases:** `vst.load(name)`, `LoadVst(name)`, `LoadVstByIndex(index)`

```csharp
// By name or path
var plugin = vst.load("Serum")                          // By name
var plugin = vst.load("C:/VST/Serum.dll")               // By path

// By index
var plugin = vst.load(0)                                // First plugin
```

### Get Plugin

**Aliases:** `vst.get(name)`, `vst.plugin(name)`, `GetVst(name)`

```csharp
// Get already loaded plugin
var serum = vst.get("Serum")
var serum = vst.plugin("Serum")    // Alias
```

### Plugin Control (Fluent API)

**Aliases:** `plugin.from(index)`, `plugin.param(name, value)`, `plugin.volume(value)`, `plugin.noteOn(note, velocity)`, `plugin.noteOff(note)`, `plugin.allNotesOff()`, `plugin.cc(controller, value, channel)`, `plugin.program(number, channel)`, `plugin.pitchBend(value, channel)`

```csharp
// MIDI routing
plugin.from(0)                     // MIDI from device 0
plugin.from("Akai MPK")            // MIDI by device name

// Set parameters
plugin.param("cutoff", 0.5)        // By parameter name
plugin.param(12, 0.7)              // By parameter index
plugin.volume(0.8)                 // Volume

// Send notes
plugin.noteOn(60, 100)             // Note on
plugin.noteOff(60)                 // Note off
plugin.allNotesOff()               // All notes off

// MIDI messages
plugin.cc(1, 64)                   // Control change
plugin.cc(74, 100, 1)              // CC on channel 1
plugin.program(5)                  // Program change
plugin.pitchBend(8192)             // Pitch bend (center = 8192)
```

### Direct Functions

**Aliases:** `LoadVst(name)`, `LoadVstByIndex(index)`, `GetVst(name)`, `RouteToVst(deviceIndex, plugin)`, `ListVstPlugins()`, `ListLoadedVstPlugins()`

```csharp
// Alternative direct calls
var plugin = LoadVst("Serum")           // Load plugin
var plugin = LoadVstByIndex(0)          // Load by index
var plugin = GetVst("Serum")            // Get loaded
RouteToVst(0, plugin)                   // Route MIDI
ListVstPlugins()                        // List plugins
ListLoadedVstPlugins()                  // List loaded
```

### Example

```csharp
// Show all plugins
vst.list();

// Load and configure plugin
var serum = vst.load("Serum");
if (serum != null)
{
    serum.from(0)                    // MIDI from first keyboard
         .param("cutoff", 0.7)       // Set filter
         .param("resonance", 0.3)
         .volume(0.8);               // Volume

    // Play test tone
    serum.noteOn(60, 100);

    // CC mapping
    midi.device(0).cc(1).to(serum.Plugin, "cutoff");
}

Start();
```

---

## 9. Sample Fluent API

The Sample Fluent API provides an elegant syntax for sample instruments.

### samples Object

**Aliases:** `samples.create(name)`, `samples.load(filePath, rootNote)`, `samples.fromDirectory(directoryPath)`

Main entry point for sample operations.

### Create Sampler

**Aliases:** `samples.create(name)`, `CreateSampler(name)`

```csharp
// Create empty sampler
var sampler = samples.create()
var sampler = samples.create("MySampler")    // With name
```

### Load Sample

**Aliases:** `samples.load(filePath, rootNote)`, `CreateSamplerFromFile(filePath, rootNote)`

```csharp
// Single sample as instrument
var piano = samples.load("C:/Samples/piano.wav")           // Default root = C4 (60)
var piano = samples.load("C:/Samples/piano.wav", 48)       // Root = C3 (48)
```

### Load from Directory

**Aliases:** `samples.fromDirectory(directoryPath)`, `CreateSamplerFromDirectory(directoryPath)`

```csharp
// All samples from a folder
var kit = samples.fromDirectory("C:/Samples/DrumKit/")
```

### Builder Methods (chainable)

**Aliases:** `.map(filePath, note)`, `.directory(path)`, `.volume(value)`, `.name(name)`, `.pattern()`

```csharp
// Map sample to note
.map("kick.wav", 36)              // Sample to note 36 (C2)
.map("snare.wav", 38)             // Sample to note 38 (D2)

// Set directory
.directory("C:/Samples/")         // Base directory for relative paths

// Volume
.volume(0.8)                      // 80% volume

// Name
.name("DrumKit")                  // Set name

// Create pattern
.pattern()                        // Returns pattern
```

### Get Sampler Object

```csharp
// Get the SampleInstrument object
var sampler = samples.create().Sampler

// Implicit conversion
SampleInstrument sampler = samples.create();
```

### Example

```csharp
// Create simple drum kit
var drums = samples.create("DrumKit")
    .directory("C:/Samples/Drums/")
    .map("kick.wav", 36)
    .map("snare.wav", 38)
    .map("hihat_closed.wav", 42)
    .map("hihat_open.wav", 46)
    .volume(0.9);

// Pattern for drums
var drumPattern = drums.pattern();
drumPattern.AddNote(0, 36, 100, 0.25);    // Kick on beat 1
drumPattern.AddNote(1, 38, 100, 0.25);    // Snare on beat 2
drumPattern.AddNote(2, 36, 100, 0.25);    // Kick on beat 3
drumPattern.AddNote(3, 38, 100, 0.25);    // Snare on beat 4

// HiHat pattern
for (int i = 0; i < 8; i++)
{
    drumPattern.AddNote(i * 0.5, 42, 80, 0.1);  // HiHat on 8th notes
}

// Piano from sample file
var piano = samples.load("C:/Samples/grand_piano_c4.wav", 60)
    .name("GrandPiano")
    .volume(0.7);

// Route MIDI to piano
midi.device(0).route(piano.Sampler);

SetBpm(100);
Start();
```

---

## 10. Helper Functions

Useful helpers for scripts.

### Print

**Aliases:** `Print(message)`

Outputs a message to the console.

```csharp
void Print(string message)
```

### Random

**Aliases:** `Random(min, max)`

Generates a random floating-point number.

```csharp
float Random(float min, float max)
```

### RandomInt

**Aliases:** `RandomInt(min, max)`

Generates a random integer.

```csharp
int RandomInt(int min, int max)
```

### Example

```csharp
Print("Script started!");

// Generate random notes
var synth = CreateSynth();
var pattern = CreatePattern(synth);

for (int i = 0; i < 16; i++)
{
    int note = RandomInt(48, 72);           // Random note C3-C5
    int velocity = RandomInt(60, 127);      // Random velocity
    float duration = Random(0.1f, 0.5f);    // Random duration

    pattern.AddNote(i * 0.25, note, velocity, duration);
}

Print($"Pattern created with 16 random notes");
SetBpm(120);
Start();
```

---

## 11. Frequency Triggers

Advanced functions for frequency-based triggers from audio inputs.

### AddFrequencyTrigger

**Aliases:** `AddFrequencyTrigger(deviceIndex, low, high, threshold, action)`, `audio.input(deviceIndex).onFrequency(low, high).threshold(value).trigger(action)`

Adds a frequency trigger that responds to specific frequency ranges.

```csharp
void AddFrequencyTrigger(int deviceIndex, float low, float high, float threshold, Action<float> action)
```

**Parameters:**
- `deviceIndex` - Audio input device index
- `low` - Lower frequency limit in Hz
- `high` - Upper frequency limit in Hz
- `threshold` - Threshold for triggering (0.0 - 1.0)
- `action` - Action called with the magnitude

### Fluent API Syntax

```csharp
audio.input(0)
    .onFrequency(lowHz, highHz)
    .threshold(value)
    .trigger(synth, note, velocity)    // Or custom action
```

### Example: Drum Triggers

```csharp
var drums = samples.create("DrumTrigger")
    .map("kick_sample.wav", 36)
    .map("snare_sample.wav", 38);

// Trigger kick drum on low frequencies
audio.input(0).onFrequency(50, 150)
    .threshold(0.4)
    .trigger(drums.Sampler, 36, 100);

// Trigger snare on mid frequencies
audio.input(0).onFrequency(200, 400)
    .threshold(0.3)
    .trigger(drums.Sampler, 38, 80);

// Custom action for high frequencies
audio.input(0).onFrequency(2000, 5000)
    .threshold(0.2)
    .trigger(magnitude => {
        Print($"High frequency detected: {magnitude:F2}");
        // Additional actions could follow here
    });

Start();
```

---

## MIDI Transport Mapping

Functions for mapping MIDI controllers to transport functions.

### MapBpm

**Aliases:** `MapBpm(deviceIndex, cc)`

Maps a MIDI CC to BPM control (60-200 BPM).

```csharp
void MapBpm(int deviceIndex, int cc)
```

### MapStart

**Aliases:** `MapStart(deviceIndex, note)`

Maps a MIDI note to start the sequencer.

```csharp
void MapStart(int deviceIndex, int note)
```

### MapStop

**Aliases:** `MapStop(deviceIndex, note)`

Maps a MIDI note to stop the sequencer.

```csharp
void MapStop(int deviceIndex, int note)
```

### MapSkip

**Aliases:** `MapSkip(deviceIndex, cc, beats)`

Maps a MIDI CC to skip beats.

```csharp
void MapSkip(int deviceIndex, int cc, double beats)
```

### MapScratch

**Aliases:** `MapScratch(deviceIndex, cc, scale)`

Maps a MIDI CC for scratching behavior.

```csharp
void MapScratch(int deviceIndex, int cc, double scale = 16.0)
```

### Example

```csharp
// Transport control via MIDI
MapStart(0, 60);        // C4 starts sequencer
MapStop(0, 61);         // C#4 stops sequencer
MapBpm(0, 20);          // CC20 controls BPM
MapSkip(0, 21, 4);      // CC21 skips 4 beats

// Scratching with jog wheel
MapScratch(0, 22, 32);  // CC22 for scratch, 32 beat range

Start();
```

---

## Direct MIDI Routing Functions

Lower level for direct MIDI routing.

### RouteMidi

**Aliases:** `RouteMidi(deviceIndex, synth)`, `midi.device(deviceIndex).route(synth)`

Routes MIDI input directly to a synthesizer.

```csharp
void RouteMidi(int deviceIndex, ISynth synth)
```

### MapControl

**Aliases:** `MapControl(deviceIndex, cc, synth, param)`, `midi.device(deviceIndex).cc(cc).to(synth, param)`

Maps a MIDI CC directly to a synth parameter.

```csharp
void MapControl(int deviceIndex, int cc, ISynth synth, string param)
```

### MapPitchBend

**Aliases:** `MapPitchBend(deviceIndex, synth, param)`, `midi.device(deviceIndex).pitchbend().to(synth, param)`

Maps pitch bend to a synth parameter.

```csharp
void MapPitchBend(int deviceIndex, ISynth synth, string param)
```

---

## Complete Example

```csharp
// ============================================
// MusicEngine Live Performance Setup
// ============================================

// === Create instruments ===
var lead = CreateSynth();
var bass = CreateSynth();
var pad = CreateSynth();

// Drum kit from samples
var drums = samples.create("DrumKit")
    .directory("C:/Samples/Drums/")
    .map("kick.wav", 36)
    .map("snare.wav", 38)
    .map("hihat.wav", 42)
    .map("crash.wav", 49)
    .volume(0.9);

// Load VST plugin
var reverb = vst.load("ValhallaRoom");
if (reverb != null)
{
    reverb.param("decay", 0.6)
          .param("mix", 0.3);
}

// === MIDI routing ===
// Keyboard 1: Split - bass left, lead right
midi.playablekeys.range(21, 59).from(0).map(bass);
midi.playablekeys.range(60, 108).from(0).map(lead);

// Keyboard 2: Pad
midi.device(1).route(pad);

// Drum pad: Drums
midi.device("Akai MPD").route(drums.Sampler);

// === CC mapping ===
midi.device(0).cc(1).to(lead, "filterCutoff");
midi.device(0).cc(74).to(lead, "filterRes");
midi.device(0).pitchbend().to(lead, "pitch");

// === Transport ===
MapStart(0, 120);   // Pad button starts
MapStop(0, 121);    // Pad button stops
MapBpm(0, 20);      // Fader for BPM

// === Patterns ===
var drumPattern = drums.pattern();
// 4/4 beat
drumPattern.AddNote(0, 36, 100, 0.1);     // Kick
drumPattern.AddNote(1, 38, 100, 0.1);     // Snare
drumPattern.AddNote(2, 36, 100, 0.1);     // Kick
drumPattern.AddNote(3, 38, 100, 0.1);     // Snare

// HiHat on 8th notes
for (int i = 0; i < 8; i++)
{
    drumPattern.AddNote(i * 0.5, 42, 70, 0.05);
}

// === Audio settings ===
audio.channel(0).gain(0.8);   // Lead
audio.channel(1).gain(0.9);   // Bass
audio.channel(2).gain(0.6);   // Pad
audio.channel(3).gain(1.0);   // Drums
audio.all.gain(0.85);         // Master

// === Start ===
SetBpm(128);
Print("Setup complete! Press start button to play.");
```

---

## Notes

- All paths should be absolute paths or relative to the working directory.
- MIDI devices are numbered starting at index 0.
- BPM range: typically 60-200 BPM.
- Velocity range: 0-127 (MIDI standard).
- Note range: 0-127 (MIDI standard, C4 = 60).
- Gain/volume values: 0.0 = mute, 1.0 = normal, >1.0 = amplification.

---

## 12. Virtual Audio Channels

The Virtual Audio Channel API enables routing audio to other applications via Named Pipes. This can be used as a virtual microphone or audio input for other programs.

### virtualChannels Object

**Aliases:** `virtualChannels.create(name)`, `virtualChannels.list()`

Main entry point for Virtual Audio Channel operations.

### Create Channel

**Aliases:** `virtualChannels.create(name)`, `CreateVirtualChannel(name)`

```csharp
// Create new virtual channel
var channel = virtualChannels.create("MyChannel")

// With chaining
var channel = virtualChannels.create("Output")
    .volume(0.8)
    .start();
```

### List Channels

**Aliases:** `virtualChannels.list()`, `ListVirtualChannels()`

```csharp
// Show all virtual channels
virtualChannels.list()
```

### Builder Methods

**Aliases:** `.volume(value)`, `.start()`, `.stop()`, `.pipeName`

```csharp
// Set volume
.volume(0.8)              // 80% volume

// Start channel
.start()                  // Starts the pipe server

// Stop channel
.stop()                   // Stops the pipe server

// Get pipe name
.pipeName                 // Returns the pipe name
```

### Direct Access

```csharp
// Get channel object
var channel = virtualChannels.create("Audio").Channel

// Implicit conversion
VirtualAudioChannel channel = virtualChannels.create("Audio");
```

### Direct Functions

**Aliases:** `CreateVirtualChannel(name)`, `ListVirtualChannels()`

```csharp
// Alternative direct calls
var channel = CreateVirtualChannel("MyChannel")
ListVirtualChannels()
```

### Usage with Other Programs

The virtual channel creates a Named Pipe with the name `MusicEngine_[ChannelName]`.

Other programs can connect to this pipe to receive audio:

**Python Example:**
```python
import struct

pipe_path = r'\\.\pipe\MusicEngine_MyChannel'
with open(pipe_path, 'rb') as pipe:
    # Read header (SampleRate, Channels, BitsPerSample)
    header = pipe.read(12)
    sample_rate, channels, bits = struct.unpack('III', header)

    # Read audio data
    while True:
        data = pipe.read(4096)
        if not data:
            break
        # Process float samples...
```

**C# Example:**
```csharp
using var pipe = new NamedPipeClientStream(".", "MusicEngine_MyChannel", PipeDirection.In);
pipe.Connect();

// Read header
var headerBuffer = new byte[12];
pipe.Read(headerBuffer, 0, 12);
int sampleRate = BitConverter.ToInt32(headerBuffer, 0);
int channels = BitConverter.ToInt32(headerBuffer, 4);

// Read audio
var buffer = new byte[4096];
while (pipe.Read(buffer, 0, buffer.Length) > 0)
{
    // Process float samples...
}
```

### Example: Route Audio to Discord/OBS

```csharp
// Create virtual channel for streaming
var streamOutput = virtualChannels.create("StreamAudio")
    .volume(1.0)
    .start();

Print($"Stream channel created: {streamOutput.pipeName}");
Print("Connect your streaming tool to this pipe!");

// Normal audio instruments
var synth = CreateSynth();
var pattern = CreatePattern(synth);

pattern.AddNote(0, 60, 100, 0.5);
pattern.AddNote(1, 64, 100, 0.5);
pattern.AddNote(2, 67, 100, 0.5);
pattern.AddNote(3, 72, 100, 0.5);

SetBpm(120);
Start();
```

### Example: Multiple Channels for Multitrack

```csharp
// Separate channels for different instruments
var drumsChannel = virtualChannels.create("Drums").start();
var bassChannel = virtualChannels.create("Bass").start();
var leadsChannel = virtualChannels.create("Leads").start();

virtualChannels.list();  // Show all channels

// Other programs can now record individual tracks
```

---

*MusicEngine Scripting API - Version 2026*
*Copyright (c) 2026 MusicEngine Watermann420 and Contributors*
