# MusicEngine Project - Claude Code Context

## Project Overview
Two C# .NET 10 projects for audio/music production:

### 1. MusicEngine (Engine/Library)
**Path:** `C:\Users\null\RiderProjects\MusicEngine`

- Audio engine with 45+ synthesizers
- VST2/VST3/CLAP plugin hosting via VST.NET
- Sequencer with pattern-based composition
- MIDI Input/Output with NAudio.Midi, MPE, MIDI 2.0
- 100+ Effects (Reverb, Delay, Chorus, Distortion, AI/ML, etc.)
- Music Theory (Notes, Chords, Scales, Arpeggiator)
- Session Management (Save/Load as JSON)
- AI/ML Features (Denoiser, Declip, MixAssistant, MasteringAssistant)

### 2. MusicEngineEditor (Desktop App)
**Path:** `C:\Users\null\RiderProjects\MusicEditor\MusicEngineEditor`

- WPF Desktop application
- Code editor with Roslyn integration for live-coding
- MVVM pattern with CommunityToolkit.Mvvm
- References MusicEngine as project dependency

## Technology Stack
| Component | Technology |
|-----------|------------|
| Framework | .NET 10, C# 13 |
| UI | WPF (Windows only) |
| Audio | NAudio 2.2.1 |
| VST Hosting | VST.NET |
| Testing | xUnit 2.9.0, FluentAssertions 6.12.0, Moq 4.20.72 |
| Logging | Microsoft.Extensions.Logging + Serilog |
| DI | Microsoft.Extensions.DependencyInjection |
| Config | Microsoft.Extensions.Configuration.Json |
| MVVM | CommunityToolkit.Mvvm |

## Project Structure

```
MusicEngine/
├── Core/
│   ├── AudioEngine.cs          # Main audio engine with Mixer & PDC
│   ├── Sequencer.cs            # Pattern-Sequencing, BPM, Transport
│   ├── Arrangement.cs          # Timeline with AudioClip, MidiClip, Region
│   ├── Pattern.cs              # Note Events Container
│   │
│   ├── Synthesizers/
│   │   ├── SimpleSynth.cs       # Monophonic synthesizer
│   │   ├── PolySynth.cs         # Polyphonic synthesizer with voice stealing
│   │   ├── FMSynth.cs           # FM Synthesis (6 Operators)
│   │   ├── GranularSynth.cs     # Granular Synthesis
│   │   ├── WavetableSynth.cs    # Wavetable Synthesis
│   │   ├── AdvancedSynth.cs     # Multi-Oscillator Synth
│   │   ├── PhysicalModeling.cs  # Karplus-Strong, Waveguide
│   │   ├── SampleSynth.cs       # Multi-Sample, Velocity Layers, Round-Robin
│   │   ├── SpeechSynth.cs       # Formant Synthesis, TTS, Singing Mode
│   │   ├── SupersawSynth.cs     # JP-8000 Style Supersaw, 1-16 Unison
│   │   ├── AdditiveSynth.cs     # Harmonic Series, Hammond Drawbars
│   │   ├── VectorSynth.cs       # XY Crossfading between 4 Oscillators
│   │   └── NoiseGenerator.cs    # White, Pink, Brown, Blue, Violet Noise
│   │
│   ├── Effects/
│   │   ├── Dynamics/           # Compressor, MultibandCompressor, Gate, Limiter, etc.
│   │   ├── TimeBased/          # Reverb, ConvolutionReverb, Delay, etc.
│   │   ├── Modulation/         # Chorus, Flanger, Phaser, Tremolo, Vibrato
│   │   ├── Distortion/         # Distortion, Bitcrusher, TapeSaturation, HarmonicEnhancer
│   │   ├── Filters/            # Filter, ParametricEQ
│   │   ├── Special/            # Exciter, StereoWidener, Vocoder, SubBassGenerator, Dither
│   │   └── EffectBase.cs       # Base class for effects
│   │
│   ├── Sequencing/
│   │   ├── StepSequencer.cs     # Drum Machine Style, Multi-Row
│   │   ├── ProbabilitySequencer.cs # Per-Step Probability, Ratchet
│   │   ├── EuclideanRhythm.cs   # Bjorklund Algorithm
│   │   └── Humanizer.cs         # Timing/Velocity Randomization
│   │
│   ├── Vst/                    # VST2/VST3 Plugin Hosting
│   ├── Analysis/               # Spectrum, Correlation, TruePeak, TempoDetection
│   ├── AudioEncoding/          # FLAC, OGG, AIFF Encoders
│   ├── PDC/                    # Plugin Delay Compensation
│   ├── Freeze/                 # Track Freeze/Bounce
│   ├── Groove/                 # Groove Extraction & Templates (16 Built-in)
│   ├── Routing/                # Send/Return Buses
│   ├── UndoRedo/               # Command Pattern
│   └── MusicTheory/            # Note, Chord, Scale, Arpeggiator
│
├── Infrastructure/
│   ├── Logging/                # Serilog Configuration
│   ├── DependencyInjection/    # IAudioEngine, ISequencer, IVstHost
│   ├── Configuration/          # MusicEngineOptions, appsettings.json
│   └── Memory/                 # AudioBufferPool
│
└── MusicEngine.Tests/          # Unit Tests (774+ Tests)
```

## Build Status (January 2026)
```
MusicEngine:       0 Errors, 2 Warnings (NetAnalyzers)
MusicEngine.Tests: 760 Tests passed, 14 failed (pre-existing)
MusicEngineEditor: 0 Errors, 43 Warnings (MVVM Toolkit field references)
Phase 2 Update:    73 new features implemented
Phase 5 Update:    4 critical editor features implemented
UI Tier 1:         8 critical UI features - COMPLETE
UI Tier 2:         7 Synthesizer UIs - COMPLETE
UI Tier 3:         4 Effect UIs - COMPLETE
UI Tier 4:         4 Analysis UIs - COMPLETE
UI Tier 5:         3 MIDI UIs - COMPLETE
UI Tier 6:         4 Performance UIs - COMPLETE
```

## UI Implementation Status (January 2026)

### Tier 1 - Critical Features (COMPLETE)
| Feature | Files | Status |
|---------|-------|--------|
| Session View / Clip Launcher | `Views/SessionView.xaml`, `Controls/ClipLauncherGrid.xaml` | Complete |
| Step Sequencer / Drum Machine | `Controls/StepSequencerControl.xaml` | Complete |
| Modular Synth Patch Editor | `Views/ModularSynthView.xaml`, `Controls/Synths/ModuleControl.xaml` | Complete |
| Polyphonic Pitch Editor | `Views/PolyphonicPitchView.xaml` | Complete |
| Spectral Editor | `Views/SpectralEditorView.xaml` | Complete |
| AI Features Panel | `Controls/AIFeaturesPanel.xaml` | Complete |
| Modulation Matrix | `Controls/ModulationMatrixControl.xaml` | Complete |
| Sidechain Matrix | `Controls/SidechainMatrixControl.xaml` | Complete |

### Tier 2 - Synthesizer UIs (COMPLETE)
| Feature | File | Description |
|---------|------|-------------|
| FM Synth Editor | `Controls/Synths/FMSynthControl.xaml` | 6-operator matrix, algorithm selector, modulation routing |
| Granular Synth Editor | `Controls/Synths/GranularSynthControl.xaml` | Grain visualization, position/size/density controls |
| Wavetable Synth Editor | `Controls/Synths/WavetableSynthControl.xaml` | Wavetable display, morph slider, position automation |
| Drum Synth Editor | `Controls/Synths/DrumSynthControl.xaml` | Kick/snare/hi-hat/clap models, 808/909 style |
| PadSynth Editor | `Controls/Synths/PadSynthControl.xaml` | Harmonic spectrum, bandwidth, detune controls |
| Vector Synth Editor | `Controls/Synths/VectorSynthControl.xaml` | XY pad, 4 oscillator crossfade, path automation |
| Additive Synth Editor | `Controls/Synths/AdditiveSynthControl.xaml` | Harmonic bars, Hammond drawbars, waveform preview |

### Tier 3 - Effect UIs (COMPLETE)
| Feature | File | Description |
|---------|------|-------------|
| Convolution Reverb | `Controls/Effects/ConvolutionReverbControl.xaml` | IR waveform display, file browser, pre-delay, decay, filters, dry/wet |
| Multiband Compressor | `Controls/Effects/MultibandCompressorControl.xaml` | 4-band spectrum display, draggable crossovers, per-band controls |
| Vocoder | `Controls/Effects/VocoderControl.xaml` | Band visualization, carrier source selector, formant shift |
| Spectral Gate | `Controls/Effects/SpectralGateControl.xaml` | Drawable threshold curve, gate activity visualization |

### Tier 4 - Analysis UIs (COMPLETE)
| Feature | File | Description |
|---------|------|-------------|
| Spectrogram 3D | `Controls/Analysis/Spectrogram3DControl.xaml` | 3D waterfall display with rotation/zoom, color maps |
| Frequency Collision | `Controls/Analysis/FrequencyCollisionControl.xaml` | Multi-track spectrum overlay, collision detection, EQ suggestions |
| Mix Radar | `Controls/Analysis/MixRadarControl.xaml` | Radar/spider chart, 8 frequency bands, reference curves |
| Phase Analyzer | `Controls/Analysis/PhaseAnalyzerControl.xaml` | Phase correlation display, mono compatibility meter |

### Tier 5 - MIDI UIs (COMPLETE)
| Feature | File | Description |
|---------|------|-------------|
| MPE Control | `Controls/MIDI/MPEControl.xaml` | Per-note pitch bend, pressure, slide lanes, zone config |
| Expression Map Editor | `Controls/MIDI/ExpressionMapControl.xaml` | Articulation list, keyswitch assignment, orchestra presets |
| Probability Sequencer | `Controls/MIDI/ProbabilitySequencerControl.xaml` | Step probability, ratchets, conditions, per-step settings |

### Tier 6 - Performance UIs (COMPLETE)
| Feature | File | Description |
|---------|------|-------------|
| Live Looper | `Controls/Performance/LiveLooperControl.xaml` | 8-layer looper, waveform display, transport, overdub |
| Performance Mode | `Views/PerformanceModeView.xaml` | Scene manager, crossfade, MIDI mapping, cue points |
| DJ Effects | `Controls/Performance/DJEffectsControl.xaml` | Filter XY pad, beat repeat, brake/spin effects |
| GrooveBox | `Controls/Performance/GrooveBoxControl.xaml` | 4x4 drum pads, pattern selector, tempo, swing |

### UI Bug Fixes (January 2026)
| File | Fix |
|------|-----|
| `FrequencyCollisionControl.xaml.cs` | Added `System.Windows.Controls.CheckBox` namespace, fixed `Array.Resize` ref property |
| `ExpressionMapControl.xaml.cs` | Added `System.Windows.Controls.ContextMenu` namespace, used `MusicEngine.Core.Midi.Articulation` |
| `VocoderControl.xaml.cs` | Removed invalid `IsEnabled` property access on `EnhancedVocoder` |
| `FMSynthControl.xaml.cs` | Added 5 converters for algorithm display, carrier visualization |
| `GranularSynthControl.xaml.cs` | Added `GranularInverseBoolToVisibilityConverter` |
| `VectorSynthControl.xaml.cs` | Added path editor and oscillator event handlers |
| `AdditiveSynthControl.xaml.cs` | Added `Additive` prefix to all converters to avoid conflicts |

## Massive Feature Update Phase 2 (January 2026)
73 additional features implemented:

### Phase 2 Engine Features (34)
| Feature | Path | Description |
|---------|------|--------------|
| LoudnessNormalizerEnhanced | Effects/Dynamics/ | EBU R128, ATSC A/85 Compliance |
| BWFMetadata | AudioEncoding/ | Broadcast WAV with iXML, bext |
| InputGainProcessor | Routing/ | Pre-insert Gain, Phase Invert |
| PadSynth | Synthesizers/ | Paul Nasca's Algorithm |
| ConvolutionAmpCab | Effects/Special/ | Guitar Amp/Cab IR Loader |
| VocalRider | Effects/Dynamics/ | Auto Gain Riding |
| AudioQuantize | Analysis/ | Snap Transients to Grid |
| ClipGainEnvelope | Clips/ | Per-Clip Gain Automation |
| FrequencyCollisionDetector | Analysis/ | Masking Detection |
| DrumSynth | Synthesizers/ | 808/909 Kick, Snare, Hi-Hat |
| StringResonator | Effects/Special/ | Sympathetic Resonance |
| EnhancedVocoder | Effects/Special/ | Formant Shifting |
| BinauralRenderer | Routing/ | HRTF 3D Audio |
| MachineControl | Sync/ | MMC/MTC External Sync |
| KarplusStrongEnhanced | Synthesizers/ | Body Resonance Modeling |
| Spectrogram3D | Analysis/ | 3D Waterfall Spectrum |
| TransferFunctionAnalyzer | Analysis/ | EQ/Compressor Curves |
| MixRadarAnalyzer | Analysis/ | Frequency Balance |
| PhaseAnalyzer | Analysis/ | Detailed Phase Analysis |
| StretchMarkerProcessor | Clips/ | Multiple Stretch Points |
| DrumToMIDI | Analysis/ | Drum Audio to MIDI |
| CompTakeProcessor | Clips/ | Advanced Comping |
| OMFExporter | Export/ | Open Media Framework |
| AutoTune | Effects/Special/ | Real-time Pitch Correction |
| RoomCorrection | Analysis/ | Acoustic Measurement |
| StemSeparation | Analysis/ | AI Source Separation |
| AAFExporter | Export/ | Advanced Authoring Format |
| MIDI2Processor | Midi/ | MIDI 2.0 Spec |
| CLAPHost | Vst/ | CLAP Plugin Format |
| AIDenoiser | Effects/AI/ | Neural Network Denoising |
| AIDeclip | Effects/AI/ | ML Clipping Restoration |
| ChordSuggestionEngine | AI/ | AI Chord Suggestions |
| MelodyGenerator | AI/ | AI Melody Completion |
| MixAssistant | AI/ | Auto EQ/Compression |
| MasteringAssistant | AI/ | One-Click AI Mastering |

### Phase 2 Editor Features (39)
| Feature | Path | Description |
|---------|------|--------------|
| MarkerListView | Views/ | Sortable Table with Jump-To |
| TempoListEditor | Views/ | Edit Tempo Changes as List |
| TrackSearchPanel | Controls/ | Find Tracks by Name |
| PluginSearchPanel | Controls/ | Search Plugins in Chain |
| SoloClearButton | Controls/ | One-Click Clear All Solos |
| TrackNotesPanel | Controls/ | Per-Track Text Notes |
| ColorPaletteService | Services/ | Track Color Schemes |
| ExportPresetService | Services/ | Save Export Settings |
| MixerUndoService | Services/ | Separate Mixer Undo Stack |
| RecentFilesPanel | Controls/ | Quick Access Thumbnails |
| MackieControlService | Services/ | MCU/HUI Protocol |
| OSCControlSurfaceService | Services/ | TouchOSC/Lemur Mapping |
| MIDIControlSurfaceService | Services/ | Generic MIDI CC |
| BatchProcessorDialog | Views/Dialogs/ | Multi-File Effects |
| AudioFileBrowser | Controls/ | Preview Before Import |
| PluginManagerDialog | Views/Dialogs/ | Favorites, Tags, Search |
| SlipEditControl | Controls/ | Move Under Fixed Boundaries |
| ShuffleEditService | Services/ | Ripple Edit Mode |
| GroupEditingService | Services/ | Edit Multiple Tracks |
| BackupManagerService | Services/ | Auto-Backup with Restore |
| Spectrogram3DView | Controls/ | 3D Waterfall Display |
| PhaseScopeView | Controls/ | Phase Analysis View |
| TransferFunctionView | Controls/ | EQ/Compressor Curves |
| ReferenceOverlayControl | Controls/ | Reference Spectrum Overlay |
| LoudnessGraphView | Controls/ | LUFS History |
| MixRadarView | Controls/ | Frequency Balance Radar |
| ProjectStatisticsDialog | Views/Dialogs/ | Track Count, CPU |
| ProjectCompareDialog | Views/Dialogs/ | Diff Two Versions |
| StretchMarkersControl | Controls/ | Visual Stretch Editing |
| CompTakesView | Views/ | Advanced Comping UI |
| TrackVersioningService | Services/ | Multiple Track Versions |
| TrackImportDialog | Views/Dialogs/ | Import From Projects |
| SessionNotesPanel | Controls/ | Timestamped Notes |
| KeyboardShortcutsDialog | Views/Dialogs/ | Custom Key Mappings |
| TouchOptimization | Themes/ | Large Touch Targets |
| MacroRecorderService | Services/ | Record/Playback Actions |
| LyricSyncEditor | Controls/ | Sync Lyrics to Timeline |
| TranscriptionView | Views/ | Audio to Notation |

### Phase 5 Editor Features (4 critical features)
| Feature | Path | Description |
|---------|------|-------------|
| Tap Tempo | ViewModels/TransportViewModel.cs | Calculate BPM from taps, 2-8 tap average, auto-reset after 2s |
| MIDI Note Preview | Services/NotePreviewService.cs | SimpleSynth preview, low-latency, Shift for sustain |
| Project Browser Operations | ViewModels/ProjectBrowserViewModel.cs | Export, Delete, Rename, Duplicate, Archive (ZIP) |
| Track Management | MainWindow.xaml.cs | Duplicate/Delete/Freeze integration, FreezeTrackData metadata |

### Phase 6: Dark Theme Visual Overhaul (January 2026)
Complete visual update with modern DAW style (inspired by Ableton/FL Studio/Bitwig):

**New Color Palette:**
| Element | Old | New | Description |
|---------|-----|-----|--------------|
| Main Background | #1E1F22 | #0D0D0D | True black for depth |
| Editor Background | #1E1F22 | #121212 | Slightly lighter |
| Panel Background | #2B2D30 | #181818 | Dark gray panels |
| Hover State | #3C3F41 | #252525 | Hover effect |
| Borders | #393B40 | #2A2A2A | Subtle borders |
| Primary Accent | #4B6EAF (Blue) | #00D9FF (Cyan) | Vibrant accent |
| Success/Run | #4CAF50 | #00CC66 | Bright green |
| Bright Green | #6AAB73 | #00FF88 | Glowing green |
| Error/Stop | #F44336 | #FF4757 | Bright red |
| Warning | #E8B339 | #FFB800 | Bright yellow |
| Primary Text | #BCBEC4 | #E0E0E0 | Lighter text |
| Secondary Text | #6F737A | #808080 | Standard gray |

**Updated Files:**
- `Themes/DarkTheme.xaml` - Complete color palette updated
- `Themes/TouchTheme.xaml` - Touch-optimized styles updated
- `MainWindow.xaml` - 27+ hardcoded colors replaced
- 100+ Control XAML files in `/Controls/`
- All Views and Dialogs in `/Views/`
- 35+ C# files with Color.FromRgb defaults
- Services and Models with color values

**Visual Improvements:**
- Subtle glow effects on accent elements (Run button: green glow)
- Improved shadows for panel depth
- Refined border-radius and spacing
- Smooth hover transitions with animations

---

## Feature Update Phase 1 (January 2026)
60 new features implemented across Engine and Editor:

### New Engine Features (21)
| Feature | Path | Description |
|---------|------|-------------|
| DCOffsetRemoval | Effects/Restoration/ | High-pass filter at 5-20Hz |
| ClickRemoval | Effects/Restoration/ | Derivative Analysis + Interpolation |
| HumRemoval | Effects/Restoration/ | Adaptive Notch at 50/60Hz |
| BreathRemoval | Effects/Restoration/ | Auto Vocal Breath Detection |
| VCAFaders | Routing/VCAGroup.cs | Linked Gain without Audio Routing |
| ChannelStripPresets | Presets/ | Save/Load Effect Chains as JSON |
| MPE Support | Midi/MPEProcessor.cs | Per-Note Pitch Bend, Pressure, Slide |
| ModulationMatrix | Modulation/ | Global LFO/Envelope Routing |
| SidechainMatrix | Routing/ | Flexible Sidechain Source Routing |
| MixerSnapshots | Routing/ | A/B Mixer States, Interpolation |
| SamplerSlicer | Synthesizers/ | REX-style Beat Slicing |
| ExpressionMaps | Midi/ | Keyswitch Management |
| SoundVariations | Midi/ | Articulation Switching |
| VideoTrack | Video/ | SMPTE Timecode Sync |
| ClipLauncher | Session/ | Ableton-style Session View |
| SpectralEditor | Analysis/ | FFT Frequency Editing |
| AudioAlignment | Analysis/ | VocAlign-style DTW |
| SurroundPanner | Routing/ | 5.1/7.1/Atmos VBAP |
| ModularSynth | Synthesizers/Modular/ | Patch-based Synthesis |
| VSTLinkBridge | Integration/ | DAW Inter-Operation |
| PolyphonicPitchEdit | Analysis/ | Melodyne DNA-style |

### New Editor Features (39)
- ArrangerTrack, TempoCurveEditor, DrumEditor, EventListEditor
- QuickControlsPanel, LoudnessReportDialog, BounceInPlaceCommand
- AutomationModes, BezierCurves, CrossfadeEditorDialog
- TrackTemplateService, CrashRecovery, MIDIMonitorPanel
- ChordTrackControl, VideoTrackControl, MultiTrackPianoRollView
- NoteExpressionLane, WorkspaceService, RenderQueueDialog, ScoreEditorView
- Plus 19 more QoL features

## Code Quality Fixes (January 2026)
The following critical issues were fixed:

### Thread-Safety Fixes
1. **PolySynth.cs** - Thread-safe Random for noise waveform
   - Removed `new Random()` in hot-path
   - Introduced ThreadStatic Random (no allocations in audio thread)

2. **EffectChain.cs** - Lock-free Read() method
   - Drastically reduced lock time
   - Copy-on-Write pattern for effect-chain reference

3. **Sequencer.cs** - Volatile fields for thread-safety
   - Marked `_running`, `_isScratching`, `_disposed` as volatile

4. **OggVorbisEncoder.cs** - Thread-safe Random for stream-ID
   - Static Random instance with lock

### Memory Leak & Exception Handling Fixes
5. **AudioEngine.cs** - StartInputCapture() memory leak fix
   - Cleanup on exceptions (waveIn?.Dispose(), handler-dictionaries cleared)
   - Event handlers registered only after successful initialization

6. **AudioEngine.cs** - Robust Dispose pattern
   - `_disposed` flag prevents double dispose
   - Try-Catch around all Dispose calls
   - Logging on Dispose errors

7. **EffectChain.cs** - GetVstEffects() as IEnumerable
   - No more List allocation on every call
   - yield return pattern for lazy evaluation

## Important Conventions

### Code Style
- File-scoped namespaces (`namespace MusicEngine.Core;`)
- German commit messages are OK
- No emojis in code/comments

### Known Workarounds
- NAudio.Midi: `NoteOnEvent` with Velocity 0 for Note-Off
- WPF Shapes: `using Shapes = System.Windows.Shapes;` due to conflicts
- MIDI Export: Custom WriteMidiFile() method (MidiFile.Export doesn't exist)
- VST3 SafeScanMode: `VstHost.SafeScanMode = true` (default) prevents AccessViolationException
- TrackType ambiguity: Use `Models.TrackType` in MainWindow.xaml.cs (conflict between Views.Dialogs.TrackType and Models.TrackType)

### Build Commands (Git Bash)
```bash
# Build Engine
"/c/Program Files/dotnet/dotnet.exe" build "C:/Users/null/RiderProjects/MusicEngine/MusicEngine.csproj"

# Build Editor
"/c/Program Files/dotnet/dotnet.exe" build "C:/Users/null/RiderProjects/MusicEditor/MusicEngineEditor/MusicEngineEditor.csproj"

# Run Tests
"/c/Program Files/dotnet/dotnet.exe" test "C:/Users/null/RiderProjects/MusicEngine/MusicEngine.Tests/MusicEngine.Tests.csproj"
```

## Example Engine Usage

```csharp
// Create audio engine
var engine = new AudioEngine();

// Add synth
var synth = new PolySynth();
synth.Waveform = WaveType.Sawtooth;
synth.Attack = 0.01;
synth.Release = 0.3;
engine.AddChannel(synth);

// Create pattern
var pattern = new Pattern("Bass", 4.0);
pattern.Note(Note.FromString("C3"), 0.0, 0.5, 100);
pattern.Note(Note.FromString("E3"), 0.5, 0.5, 100);
pattern.Note(Note.FromString("G3"), 1.0, 0.5, 100);

// Start sequencer
var sequencer = new Sequencer(engine);
sequencer.BPM = 120;
sequencer.AddPattern(pattern, synth);
sequencer.Start();
```

## Implemented Features (Complete)

### Effects (60+)
| Category | Effects |
|-----------|---------|
| Dynamics | Compressor, MultibandCompressor, SideChain, Gate, Limiter, TransientShaper, DeEsser, DynamicEQ, SpectralGate, VocalRider |
| Time-Based | Reverb, EnhancedReverb, ConvolutionReverb, ShimmerReverb, ReverseReverb, Delay |
| Modulation | Chorus, Flanger, Phaser, Tremolo, Vibrato, AutoPan |
| Distortion | Distortion, Bitcrusher, TapeSaturation, HarmonicEnhancer |
| Filters | Filter, ParametricEQ |
| Special | Exciter, StereoWidener, Vocoder, EnhancedVocoder, RingModulator, TapeStop, PitchShifter, SubBassGenerator, Dither, SampleRateConverter, StringResonator, ConvolutionAmpCab, BinauralRenderer, AutoTune |
| Restoration | NoiseReduction, Declipping, DCOffsetRemoval, ClickRemoval, HumRemoval, BreathRemoval |
| AI/ML | AIDenoiser, AIDeclip, ChordSuggestionEngine, MelodyGenerator, MixAssistant, MasteringAssistant |

### Synthesizers (18)
| Synth | Description |
|-------|-------------|
| SimpleSynth | Monophonic with ADSR |
| PolySynth | Polyphonic with Voice Stealing |
| FMSynth | 6-Operator FM |
| GranularSynth | Granular Synthesis |
| WavetableSynth | Wavetable with Morphing |
| AdvancedSynth | Multi-Oscillator |
| PhysicalModeling | Karplus-Strong, Waveguide |
| SampleSynth | Multi-Sample, Velocity, Loop |
| SpeechSynth | Formant, TTS, Singing |
| SupersawSynth | JP-8000 Style, Unison |
| AdditiveSynth | Hammond Drawbars |
| VectorSynth | XY Pad Crossfade |
| NoiseGenerator | 5 Noise Types |
| ModularSynth | Patch-based with VCO/VCF/VCA/LFO/ADSR Modules |
| SamplerSlicer | REX-style Beat Slicing |
| PadSynth (NEW) | Paul Nasca's Algorithm for Evolving Pads |
| DrumSynth (NEW) | 808/909 Style Kick, Snare, Hi-Hat, Clap |
| KarplusStrongEnhanced (NEW) | Body Resonance Modeling |

### Routing & Mixing (NEW)
| Feature | Description |
|---------|--------------|
| VCAFaders | Linked Gain without Audio Routing |
| SidechainMatrix | Flexible Sidechain Routing |
| MixerSnapshots | A/B Compare, Interpolation, Crossfade |
| SurroundPanner | 5.1/7.1/Atmos VBAP Panning |
| ChannelStripPresets | Save/Load Effect Chains |

### Analysis (NEW)
| Feature | Description |
|---------|--------------|
| SpectralEditor | FFT-based Frequency Editing, STFT, Undo/Redo |
| AudioAlignment | VocAlign-style DTW, Formant Preservation |
| PolyphonicPitchEdit | Melodyne DNA-style, HPS Multi-Pitch Detection |

### MIDI & Expression (NEW)
| Feature | Description |
|---------|--------------|
| MPEProcessor | Per-Note Pitch Bend, Pressure, Slide |
| ModulationMatrix | Global LFO/Envelope Routing |
| ExpressionMaps | Keyswitch Management |
| SoundVariations | Articulation Switching |

### Session & Video (NEW)
| Feature | Description |
|---------|--------------|
| ClipLauncher | Ableton-style Session View, Scenes |
| VideoTrack | SMPTE Timecode Sync |
| VSTLinkBridge | DAW Inter-Operation |

### MIDI & Sequencing
| Feature | Description |
|---------|--------------|
| StepSequencer | Drum Machine Style, Multi-Row, Direction Modes |
| ProbabilitySequencer | Per-Step Probability, Ratchet, Conditions |
| EuclideanRhythm | Bjorklund Algorithm, 17 Presets |
| Humanizer | Timing/Velocity Randomization |
| ScaleQuantizer | Force MIDI to Scale |
| MidiEffects | MidiDelay, MidiArpeggiator, MidiChord |

### Audio Processing
| Feature | Description |
|---------|--------------|
| TimeStretch | Phase Vocoder |
| PitchShifter | Phase Vocoder |
| Audio-to-MIDI | YIN Algorithm |
| ChordDetection | Real-time |
| KeyDetection | Krumhansl-Schmuckler |
| Dithering | RPDF/TPDF/HP-TPDF, Noise Shaping |
| SampleRateConverter | Linear/Cubic/Sinc interpolation, Anti-aliasing |
| Audio Warping (NEW) | Elastic Audio with Warp Markers, Phase Vocoder, Quantize to Grid |

### Advanced Features
| Feature | Description |
|---------|--------------|
| VST2/VST3 Hosting | Full COM Interfaces |
| PDC | Plugin Delay Compensation |
| Track Freeze | Offline Rendering |
| Send/Return Buses | Pre/Post Fader |
| Groove Templates | 16 Built-in (MPC, Shuffle, etc.) |
| Stem Export | Async with Progress |
| TempoTrack (NEW) | Tempo Changes, Linear/S-Curve Ramps |
| TimeSignatureTrack (NEW) | Mixed Meters, Bar/Beat Calculation |
| TrackGroups (NEW) | Nested Folders, Solo/Mute Propagation |
| TakeLanes (NEW) | Comping, Multi-Take, Crossfades |
| PunchRecording (NEW) | Punch In/Out, Pre/Post-Roll |
| AudioPool (NEW) | Media Management, BPM/Key Detection |
| ReferenceTrack (NEW) | A/B Comparison, LUFS Matching |
| MacroControls (NEW) | 8 Macros, Parameter Mapping, MIDI Learn |

### Network & Collaboration (NEW)
| Feature | Description |
|---------|--------------|
| LinkSync | Ableton Link-style Tempo Sync (UDP Multicast) |
| NetworkMIDI (NEW) | RTP-MIDI Style, TCP/UDP, Peer Discovery, Latency Compensation |
| CloudStorage (NEW) | Provider Abstraction, Auto-Sync, Offline Queue, Conflict Resolution |
| Collaboration (NEW) | Real-time Multi-User, OT Algorithm, TCP Server/Client, Vector Clocks |

### Editor UI Features (January 2026)
| Feature | Description |
|---------|-------------|
| Note Velocity Colors | Blue→Green→Red Gradient based on Velocity (0-127) |
| Grid Customization | Triplet (1/4T, 1/8T, 1/16T) and Dotted (1/4., 1/8., 1/16.) with visual distinction |
| Command Palette | Ctrl+P for quick access to all commands (Fuzzy Search) |
| Drag & Drop Audio | Drag audio files directly into Arrangement (WAV, MP3, FLAC, OGG, AIFF) |
| LUFS Meter | Integrated/Short-term/Momentary Loudness + True Peak in Mixer |
| Track Group Headers | Nested Folders, Solo/Mute Propagation, Volume, Collapse |
| Take Lane Control | Comping UI, Multi-Take Recording, Rating Stars, Flatten |
| Punch Locator | Punch In/Out Markers, Pre/Post-Roll Settings, Visual Feedback |
| Audio Pool View | Media Browser, Search/Tags, BPM/Key Detection, Consolidate |
| Reference Track | A/B Comparison, LUFS Level Matching, Loop Region, Transport Sync |
| Macro Bank | 8 Macro Knobs, Parameter Mapping, Curves, MIDI Learn |
| Time Signature Marker | Meter Changes on Timeline, Custom Time Signatures |
| Warp Marker Control (NEW) | Elastic Audio Editor, Draggable Warp Markers, Auto-Warp |
| Elastic Audio Editor (NEW) | BPM Detection, Tempo Matching, Waveform Warping UI |
| Tap Tempo (NEW) | Calculate BPM from Taps, 2-8 Tap Average, Auto-Reset |
| MIDI Note Preview (NEW) | Piano Roll Note Preview with SimpleSynth, Shift for Sustain |
| Project Browser Ops (NEW) | Export, Delete, Rename, Duplicate, Archive with ZIP Support |
| Track Freeze/Unfreeze (NEW) | Track Management with Metadata Storage, Audio Render |

### Project Management Features (January 2026)
| Feature | Description |
|---------|-------------|
| TempoTrack | Tempo Changes over Time, Linear/S-Curve Ramping, Beat↔Time Conversion |
| TimeSignatureTrack | Time Signature Changes, Bar/Beat Calculation, Mixed Meters Support |
| TrackGroups | Hierarchical Track Organization, Solo/Mute Propagation, Group Volume |
| TakeLanes | Multi-Take Recording, Comp Regions, Crossfades, Flatten to AudioClip |
| PunchRecording | Punch In/Out Points, Pre-Roll/Post-Roll, Auto-Punch Mode |
| AudioPool | Media Management, Usage Tracking, Consolidate, BPM/Key Analysis |
| ReferenceTrack | A/B Comparison, LUFS Analysis, Auto Level Matching, Loop/Sync |
| MacroControls | 8 Macros with Multi-Parameter Mapping, Curves, MIDI Learn |

---

## Synthesizer Documentation

Each synthesizer has its own README file with complete documentation:

| Synth | README | Content |
|-------|--------|---------|
| SimpleSynth | `README_SimpleSynth.md` | Monophonic, ADSR, 5 Waveforms |
| PolySynth | `README_PolySynth.md` | 16-Voice, Voice Stealing Modes |
| AdvancedSynth | `README_AdvancedSynth.md` | Multi-Oscillator, 5 Filter Types |
| FMSynth | `README_FMSynth.md` | 6-Operator, 20 Algorithms |
| AdditiveSynth | `README_AdditiveSynth.md` | 64 Partials, Hammond Drawbars |
| WavetableSynth | `README_WavetableSynth.md` | Wavetable Morphing, Custom Loading |
| GranularSynth | `README_GranularSynth.md` | Grain Synthesis, 5 Envelope Shapes |
| SampleSynth | `README_SampleSynth.md` | Multi-Sample, Velocity Layers |
| SpeechSynth | `README_SpeechSynth.md` | Formant/TTS, Singing Mode |
| SupersawSynth | `README_SupersawSynth.md` | JP-8000 Style, 1-16 Unison |
| VectorSynth | `README_VectorSynth.md` | XY Pad, 4 Oscillators |
| PhysicalModeling | `README_PhysicalModeling.md` | Karplus-Strong, Waveguide |
| NoiseGenerator | `README_NoiseGenerator.md` | 5 Noise Colors, Filtering |

Each README contains:
- Parameter tables with Types, Ranges, Defaults
- C# Code examples
- Signal Flow diagrams
- Sound Design tips
- Factory Presets

---

## Session History

For detailed session history and implementation details see:
`MusicEngine/CLAUDE_CONTEXT_HISTORY.md`
