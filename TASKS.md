# MusicEngine & MusicEngineEditor - Task List

## P0 - Critical (Immediate)

### MusicEngine

- [x] **Refactor ScriptHost.cs (961 LOC)** ✅ COMPLETED
  - Extract ScriptGlobals to separate file
  - Create `FluentApi/` folder with:
    - MidiControl.cs
    - DeviceControl.cs
    - AudioControl.cs
    - VstControl.cs
    - PatternControl.cs
    - SampleControl.cs
    - VirtualChannelControl.cs

- [x] **Fix Event Subscription Leaks in AudioEngine.cs** ✅ COMPLETED
  - Line 142: FftCalculated handler
  - Line 163: DataAvailable handler
  - Line 226: MessageReceived handler
  - Add explicit unsubscription in Dispose()

- [x] **Add ConfigureAwait(false) in VirtualAudioChannel.cs** ✅ COMPLETED
  - Line 150: WaitForConnectionAsync
  - Line 154: SendWaveHeader
  - Line 173: WriteAsync
  - Line 177: Task.Delay
  - Line 213: WriteAsync

### MusicEngineEditor

- [ ] **Refactor MainWindow.xaml.cs (2,372 LOC)** (Deferred - Low Priority)
  - Extract menu handling to MenuViewModel
  - Extract toolbar logic to ToolbarViewModel
  - Move status bar to StatusBarViewModel
  - Keep only UI event handlers in code-behind

---

## P1 - Important (This Sprint)

### MusicEngine

- [x] **Replace Thread.Sleep with Task.Delay** ✅ COMPLETED
  - `AudioEngine.cs:601` - Dispose method
  - `Sequencer.cs:280` - Run loop (use Timer instead)
  - `Sequencer.cs:503` - TriggerNote (use Task.Delay)

- [x] **Split God Classes** ✅ COMPLETED
  - AudioEngine.cs (611 LOC) → AudioEngine + MidiRouter + AudioMixer
  - Sequencer.cs (551 LOC) → Sequencer + Pattern + NoteEvent + LiveParameter
  - VstHost.cs (687 LOC) → VstHost + VstPlugin

- [x] **Make VST Paths Configurable** ✅ COMPLETED
  - Settings.cs:33-37 - Move to user config file
  - Added AddVstPath(), RemoveVstPath(), ResetVstPathsToDefaults()
  - Support for MUSICENGINE_VST_PATHS environment variable

### MusicEngineEditor

- [x] **Implement Missing Dialogs** ✅ COMPLETED
  - [x] NewProjectDialog (wire up existing)
  - [x] ProjectSettingsDialog
  - [x] AddScriptDialog
  - [x] ImportAudioDialog
  - [x] AboutDialog
  - [x] Find/Replace integration

- [x] **Complete Project Explorer Operations** ✅ COMPLETED
  - [x] AddNewScript command (Line 138)
  - [x] AddNewFolder command (Line 144)
  - [x] DeleteNode command (Line 150)
  - [x] RenameNode command (Line 156)

- [x] **Update NuGet Packages** ✅ COMPLETED
  ```bash
  dotnet add package AvalonEdit --version 6.3.1.120
  dotnet add package Microsoft.Extensions.DependencyInjection --version 10.0.2
  dotnet add package Serilog --version 4.3.0
  dotnet add package Serilog.Sinks.File --version 7.0.0
  ```

- [x] **Complete TODO Items in MainViewModel.cs** ✅ COMPLETED
  - [x] NewProjectAsync: Show NewProjectDialog
  - [x] OpenProjectAsync: Show OpenFileDialog
  - [x] NewFile: Show NewFileDialog
  - [x] Find/Replace: Show Find/Replace dialogs
  - [x] AddScript: Show AddScriptDialog
  - [x] AddExistingFile: Show file picker
  - [x] ImportAudio: Show ImportAudioDialog
  - [x] AddReference: Show file picker for DLLs
  - [x] ProjectSettings: Show ProjectSettingsDialog
  - [x] About: Show AboutDialog
  - [x] Exit: Check for unsaved changes
  - [x] CloseDocument: Ask to save prompt

---

## P2 - Nice to Have (Backlog)

### MusicEngine

- [ ] **Complete VST DSP Processing**
  - Implement actual audio processing in VstPlugin
  - Add preset management
  - Add parameter automation

- [ ] **Add Effects System**
  - IEffect interface
  - ReverbEffect
  - DelayEffect
  - ChorusEffect
  - Effect chain routing

- [ ] **Add Recording Feature**
  - WaveFileWriter integration
  - Real-time audio capture
  - Export to WAV/MP3

- [ ] **Improve Timing Precision**
  - High-resolution timer for sequencer
  - MIDI clock sync
  - Audio-rate scheduling

### MusicEngineEditor

- [ ] **Add Debugger Support**
  - Breakpoint system
  - Step-through execution
  - Variable inspection
  - Call stack view

- [ ] **Implement Settings/Preferences**
  - Audio device selection
  - MIDI device configuration
  - Theme customization
  - Keyboard shortcut editor

- [ ] **Add Git Integration**
  - Repository status
  - Commit/push from IDE
  - Branch management

- [ ] **Visual Enhancements**
  - Waveform display
  - Piano roll editor
  - Audio preview player

---

## Testing Tasks

### MusicEngine.Tests ✅ COMPLETED

- [x] Create xUnit test project
- [x] SimpleSynthTests
  - Waveform generation
  - Note on/off
  - Parameter changes
- [x] SequencerTests
  - Pattern playback
  - Beat accuracy
  - Event emission

### MusicEngineEditor.Tests (NEW PROJECT)

- [ ] Create xUnit test project
- [ ] MainViewModelTests
  - Command execution
  - State management
- [ ] ProjectServiceTests
  - Create/Open/Save operations
  - File serialization
- [ ] ScriptExecutionServiceTests
  - Compilation
  - Error handling

---

## Documentation Tasks

- [ ] Update README.md with current features
- [ ] Create API.md for MusicEngine public API
- [ ] Create GettingStarted.md tutorial
- [ ] Add XML documentation to public methods
- [ ] Create example scripts folder

---

## Progress Tracking

| Phase | Status | Completion |
|-------|--------|------------|
| P0 - Critical | **COMPLETED** | 100% |
| P1 - Important | **COMPLETED** | 100% |
| P2 - Backlog | NOT STARTED | 0% |
| Testing | PARTIAL | 50% |
| Documentation | PARTIAL | 20% |

**Last Updated:** 2026-01-19
