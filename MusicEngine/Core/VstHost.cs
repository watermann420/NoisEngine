//Engine License (MEL) – Honor-Based Commercial Support
// copyright (c) 2026 MusicEngine Watermann420 and Contributors
// Created by Watermann420
// Description: VST Plugin Host for loading, managing, and processing VST plugins with MIDI support.


using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using NAudio.Wave;


namespace MusicEngine.Core;


/// <summary>
/// Represents information about a discovered VST plugin
/// </summary>
public class VstPluginInfo
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public string Vendor { get; set; } = "";
    public string Version { get; set; } = "";
    public int UniqueId { get; set; }
    public bool IsInstrument { get; set; } // True for VSTi (instruments), false for effects
    public bool IsLoaded { get; set; }
    public int NumInputs { get; set; }
    public int NumOutputs { get; set; }
    public int NumParameters { get; set; }
}


/// <summary>
/// VST Plugin wrapper that implements ISynth for seamless integration
/// </summary>
public class VstPlugin : ISynth, IDisposable
{
    private readonly VstPluginInfo _info;
    private readonly WaveFormat _waveFormat;
    private readonly float[] _outputBuffer;
    private readonly float[] _inputBuffer;
    private readonly object _lock = new();
    private readonly List<(int note, int velocity)> _activeNotes = new();
    private readonly Queue<VstMidiEvent> _midiEventQueue = new();
    private IntPtr _pluginHandle = IntPtr.Zero;
    private bool _isDisposed;
    private float _masterVolume = 1.0f;

    // MIDI event structure for VST
    private struct VstMidiEvent
    {
        public int DeltaFrames;
        public byte Status;
        public byte Data1;
        public byte Data2;

        // Pack MIDI data for VST processing
        public readonly int MidiData => Status | (Data1 << 8) | (Data2 << 16);
    }

    public VstPluginInfo Info => _info;
    public WaveFormat WaveFormat => _waveFormat;
    public string Name => _info.Name;
    public bool IsInstrument => _info.IsInstrument;

    public VstPlugin(VstPluginInfo info, int sampleRate = 0)
    {
        _info = info;
        int rate = sampleRate > 0 ? sampleRate : Settings.SampleRate;
        _waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(rate, Settings.Channels);
        _outputBuffer = new float[Settings.VstBufferSize * Settings.Channels];
        _inputBuffer = new float[Settings.VstBufferSize * Settings.Channels];
    }

    /// <summary>
    /// Send a MIDI note on event to the VST plugin
    /// </summary>
    public void NoteOn(int note, int velocity)
    {
        lock (_lock)
        {
            _activeNotes.Add((note, velocity));
            _midiEventQueue.Enqueue(new VstMidiEvent
            {
                DeltaFrames = 0,
                Status = 0x90, // Note On, channel 1
                Data1 = (byte)note,
                Data2 = (byte)velocity
            });
        }
    }

    /// <summary>
    /// Send a MIDI note off event to the VST plugin
    /// </summary>
    public void NoteOff(int note)
    {
        lock (_lock)
        {
            _activeNotes.RemoveAll(n => n.note == note);
            _midiEventQueue.Enqueue(new VstMidiEvent
            {
                DeltaFrames = 0,
                Status = 0x80, // Note Off, channel 1
                Data1 = (byte)note,
                Data2 = 0
            });
        }
    }

    /// <summary>
    /// Stop all playing notes
    /// </summary>
    public void AllNotesOff()
    {
        lock (_lock)
        {
            foreach (var (note, _) in _activeNotes.ToArray())
            {
                NoteOff(note);
            }
            _activeNotes.Clear();

            // Send All Notes Off CC (CC 123)
            _midiEventQueue.Enqueue(new VstMidiEvent
            {
                DeltaFrames = 0,
                Status = 0xB0, // Control Change, channel 1
                Data1 = 123,   // All Notes Off
                Data2 = 0
            });
        }
    }

    /// <summary>
    /// Send a Control Change message to the VST plugin
    /// </summary>
    public void SendControlChange(int channel, int controller, int value)
    {
        lock (_lock)
        {
            _midiEventQueue.Enqueue(new VstMidiEvent
            {
                DeltaFrames = 0,
                Status = (byte)(0xB0 | (channel & 0x0F)),
                Data1 = (byte)controller,
                Data2 = (byte)value
            });
        }
    }

    /// <summary>
    /// Send a Pitch Bend message to the VST plugin
    /// </summary>
    public void SendPitchBend(int channel, int value)
    {
        lock (_lock)
        {
            _midiEventQueue.Enqueue(new VstMidiEvent
            {
                DeltaFrames = 0,
                Status = (byte)(0xE0 | (channel & 0x0F)),
                Data1 = (byte)(value & 0x7F),
                Data2 = (byte)((value >> 7) & 0x7F)
            });
        }
    }

    /// <summary>
    /// Send a Program Change message to the VST plugin
    /// </summary>
    public void SendProgramChange(int channel, int program)
    {
        lock (_lock)
        {
            _midiEventQueue.Enqueue(new VstMidiEvent
            {
                DeltaFrames = 0,
                Status = (byte)(0xC0 | (channel & 0x0F)),
                Data1 = (byte)program,
                Data2 = 0
            });
        }
    }

    /// <summary>
    /// Set a VST parameter by name or index
    /// </summary>
    public void SetParameter(string name, float value)
    {
        lock (_lock)
        {
            // Handle common parameters
            switch (name.ToLowerInvariant())
            {
                case "volume":
                case "gain":
                case "level":
                    _masterVolume = Math.Clamp(value, 0f, 1f);
                    break;
                case "pitchbend":
                    int bendValue = (int)(value * 16383);
                    SendPitchBend(0, bendValue);
                    break;
                default:
                    // Try to parse as parameter index
                    if (int.TryParse(name, out int paramIndex) && paramIndex >= 0)
                    {
                        SetParameterByIndex(paramIndex, value);
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Set a VST parameter by index
    /// </summary>
    public void SetParameterByIndex(int index, float value)
    {
        // This would call into the actual VST plugin via P/Invoke
        // For now, we store the intent
        lock (_lock)
        {
            // Implementation would use the plugin handle to set parameters
        }
    }

    /// <summary>
    /// Read audio samples from the VST plugin
    /// </summary>
    public int Read(float[] buffer, int offset, int count)
    {
        lock (_lock)
        {
            if (_isDisposed) return 0;

            // Process any pending MIDI events
            ProcessMidiEvents();

            // Generate audio (placeholder - actual VST processing would happen here)
            int samples = Math.Min(count, _outputBuffer.Length);

            // For now, generate silence or pass through
            // In real implementation, this would call the VST's processReplacing
            for (int i = 0; i < samples; i++)
            {
                buffer[offset + i] = _outputBuffer[i % _outputBuffer.Length] * _masterVolume;
            }

            return samples;
        }
    }

    /// <summary>
    /// Process audio input through the VST effect
    /// </summary>
    public void ProcessInput(float[] input, float[] output, int sampleCount)
    {
        lock (_lock)
        {
            if (_isDisposed) return;

            // Copy input to processing buffer
            Array.Copy(input, _inputBuffer, Math.Min(input.Length, _inputBuffer.Length));

            // Process MIDI events
            ProcessMidiEvents();

            // In real implementation, call VST's processReplacing with input/output
            // For effects, input is processed and written to output
            // For instruments, input is ignored and output is generated from MIDI

            if (!IsInstrument)
            {
                // Effect: process input
                for (int i = 0; i < sampleCount && i < output.Length; i++)
                {
                    output[i] = _inputBuffer[i % _inputBuffer.Length] * _masterVolume;
                }
            }
            else
            {
                // Instrument: generate from MIDI (output buffer already filled by MIDI processing)
                for (int i = 0; i < sampleCount && i < output.Length; i++)
                {
                    output[i] = _outputBuffer[i % _outputBuffer.Length] * _masterVolume;
                }
            }
        }
    }

    private void ProcessMidiEvents()
    {
        while (_midiEventQueue.Count > 0)
        {
            var evt = _midiEventQueue.Dequeue();
            // In real implementation, send to VST plugin via processEvents
        }
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        lock (_lock)
        {
            _isDisposed = true;
            AllNotesOff();

            if (_pluginHandle != IntPtr.Zero)
            {
                // Unload the VST plugin
                _pluginHandle = IntPtr.Zero;
            }
        }

        GC.SuppressFinalize(this);
    }
}


/// <summary>
/// Central VST Host that manages plugin discovery, loading, and lifecycle
/// </summary>
public class VstHost : IDisposable
{
    private readonly List<VstPluginInfo> _discoveredPlugins = new();
    private readonly Dictionary<string, VstPlugin> _loadedPlugins = new();
    private readonly object _lock = new();
    private bool _isDisposed;

    public IReadOnlyList<VstPluginInfo> DiscoveredPlugins => _discoveredPlugins.AsReadOnly();
    public IReadOnlyDictionary<string, VstPlugin> LoadedPlugins => _loadedPlugins;

    /// <summary>
    /// Scan for VST plugins in configured paths and return discovered plugins
    /// </summary>
    public List<VstPluginInfo> ScanForPlugins()
    {
        lock (_lock)
        {
            _discoveredPlugins.Clear();

            var searchPaths = new List<string>(Settings.VstPluginSearchPaths);

            // Add custom path if configured
            if (!string.IsNullOrEmpty(Settings.VstPluginPath))
            {
                searchPaths.Insert(0, Settings.VstPluginPath);
            }

            foreach (var basePath in searchPaths)
            {
                if (!Directory.Exists(basePath)) continue;

                try
                {
                    // Scan for VST2 plugins (.dll)
                    foreach (var file in Directory.GetFiles(basePath, "*.dll", SearchOption.AllDirectories))
                    {
                        var pluginInfo = ProbePlugin(file, false);
                        if (pluginInfo != null)
                        {
                            _discoveredPlugins.Add(pluginInfo);
                        }
                    }

                    // Scan for VST3 plugins (.vst3)
                    foreach (var file in Directory.GetFiles(basePath, "*.vst3", SearchOption.AllDirectories))
                    {
                        var pluginInfo = ProbePlugin(file, true);
                        if (pluginInfo != null)
                        {
                            _discoveredPlugins.Add(pluginInfo);
                        }
                    }

                    // VST3 bundles (folders)
                    foreach (var dir in Directory.GetDirectories(basePath, "*.vst3", SearchOption.AllDirectories))
                    {
                        var pluginInfo = ProbeVst3Bundle(dir);
                        if (pluginInfo != null)
                        {
                            _discoveredPlugins.Add(pluginInfo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Error scanning VST path '{basePath}': {ex.Message}");
                }
            }

            return new List<VstPluginInfo>(_discoveredPlugins);
        }
    }

    /// <summary>
    /// Probe a potential VST plugin file to get its info
    /// </summary>
    private VstPluginInfo? ProbePlugin(string path, bool isVst3)
    {
        try
        {
            // Basic validation - check if file exists and is accessible
            if (!File.Exists(path)) return null;

            var info = new FileInfo(path);
            if (info.Length < 1024) return null; // Too small to be a VST

            // For now, create basic info from filename
            // Real implementation would load and query the plugin
            var name = Path.GetFileNameWithoutExtension(path);

            return new VstPluginInfo
            {
                Name = name,
                Path = path,
                Vendor = "Unknown",
                Version = "1.0",
                UniqueId = path.GetHashCode(),
                IsInstrument = GuessIsInstrument(name),
                IsLoaded = false,
                NumInputs = 2,
                NumOutputs = 2,
                NumParameters = 0
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Probe a VST3 bundle directory
    /// </summary>
    private VstPluginInfo? ProbeVst3Bundle(string bundlePath)
    {
        try
        {
            if (!Directory.Exists(bundlePath)) return null;

            var name = Path.GetFileNameWithoutExtension(bundlePath);

            // Look for the actual plugin binary inside the bundle
            var contentsPath = Path.Combine(bundlePath, "Contents", "x86_64-win");
            if (!Directory.Exists(contentsPath))
            {
                contentsPath = Path.Combine(bundlePath, "Contents", "Win64");
            }

            string? pluginBinary = null;
            if (Directory.Exists(contentsPath))
            {
                var dlls = Directory.GetFiles(contentsPath, "*.vst3");
                if (dlls.Length > 0) pluginBinary = dlls[0];
            }

            return new VstPluginInfo
            {
                Name = name,
                Path = pluginBinary ?? bundlePath,
                Vendor = "Unknown",
                Version = "1.0",
                UniqueId = bundlePath.GetHashCode(),
                IsInstrument = GuessIsInstrument(name),
                IsLoaded = false,
                NumInputs = 2,
                NumOutputs = 2,
                NumParameters = 0
            };
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Guess if a plugin is an instrument based on its name
    /// </summary>
    private bool GuessIsInstrument(string name)
    {
        var lowerName = name.ToLowerInvariant();
        var instrumentKeywords = new[] { "synth", "piano", "organ", "bass", "lead", "pad", "strings", "brass", "drum", "sampler", "rompler", "keys", "vsti" };
        foreach (var keyword in instrumentKeywords)
        {
            if (lowerName.Contains(keyword)) return true;
        }
        return false;
    }

    /// <summary>
    /// Load a VST plugin by name (partial match supported)
    /// </summary>
    public VstPlugin? LoadPlugin(string nameOrPath)
    {
        lock (_lock)
        {
            // Check if already loaded
            if (_loadedPlugins.TryGetValue(nameOrPath, out var existing))
            {
                return existing;
            }

            // Find the plugin
            VstPluginInfo? info = null;

            // First try exact path match
            if (File.Exists(nameOrPath))
            {
                info = _discoveredPlugins.Find(p => p.Path.Equals(nameOrPath, StringComparison.OrdinalIgnoreCase));
                if (info == null)
                {
                    // Not in discovered list, probe it
                    info = ProbePlugin(nameOrPath, nameOrPath.EndsWith(".vst3"));
                    if (info != null) _discoveredPlugins.Add(info);
                }
            }

            // Then try name match
            if (info == null)
            {
                info = _discoveredPlugins.Find(p =>
                    p.Name.Equals(nameOrPath, StringComparison.OrdinalIgnoreCase) ||
                    p.Name.Contains(nameOrPath, StringComparison.OrdinalIgnoreCase));
            }

            if (info == null)
            {
                Console.WriteLine($"VST Plugin not found: {nameOrPath}");
                return null;
            }

            try
            {
                var plugin = new VstPlugin(info);
                info.IsLoaded = true;
                _loadedPlugins[info.Name] = plugin;
                Console.WriteLine($"Loaded VST Plugin: {info.Name} ({(info.IsInstrument ? "Instrument" : "Effect")})");
                return plugin;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load VST Plugin '{info.Name}': {ex.Message}");
                return null;
            }
        }
    }

    /// <summary>
    /// Load a VST plugin by index from the discovered list
    /// </summary>
    public VstPlugin? LoadPluginByIndex(int index)
    {
        lock (_lock)
        {
            if (index < 0 || index >= _discoveredPlugins.Count)
            {
                Console.WriteLine($"Invalid VST plugin index: {index}");
                return null;
            }

            return LoadPlugin(_discoveredPlugins[index].Name);
        }
    }

    /// <summary>
    /// Get a loaded plugin by name
    /// </summary>
    public VstPlugin? GetPlugin(string name)
    {
        lock (_lock)
        {
            if (_loadedPlugins.TryGetValue(name, out var plugin))
            {
                return plugin;
            }

            // Try partial match
            foreach (var kvp in _loadedPlugins)
            {
                if (kvp.Key.Contains(name, StringComparison.OrdinalIgnoreCase))
                {
                    return kvp.Value;
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Unload a VST plugin
    /// </summary>
    public void UnloadPlugin(string name)
    {
        lock (_lock)
        {
            if (_loadedPlugins.TryGetValue(name, out var plugin))
            {
                plugin.Dispose();
                _loadedPlugins.Remove(name);

                var info = _discoveredPlugins.Find(p => p.Name == name);
                if (info != null) info.IsLoaded = false;

                Console.WriteLine($"Unloaded VST Plugin: {name}");
            }
        }
    }

    /// <summary>
    /// Print all discovered plugins to console
    /// </summary>
    public void PrintDiscoveredPlugins()
    {
        Console.WriteLine("\n=== Discovered VST Plugins ===");
        if (_discoveredPlugins.Count == 0)
        {
            Console.WriteLine("  No VST plugins found.");
            Console.WriteLine("  Search paths:");
            foreach (var path in Settings.VstPluginSearchPaths)
            {
                Console.WriteLine($"    - {path}");
            }
        }
        else
        {
            for (int i = 0; i < _discoveredPlugins.Count; i++)
            {
                var p = _discoveredPlugins[i];
                var type = p.IsInstrument ? "VSTi" : "VST";
                var loaded = p.IsLoaded ? " [LOADED]" : "";
                Console.WriteLine($"  [{i}] {p.Name} ({type}){loaded}");
                Console.WriteLine($"      Path: {p.Path}");
            }
        }
        Console.WriteLine("==============================\n");
    }

    /// <summary>
    /// Print all loaded plugins to console
    /// </summary>
    public void PrintLoadedPlugins()
    {
        Console.WriteLine("\n=== Loaded VST Plugins ===");
        if (_loadedPlugins.Count == 0)
        {
            Console.WriteLine("  No plugins currently loaded.");
        }
        else
        {
            foreach (var kvp in _loadedPlugins)
            {
                var p = kvp.Value;
                var type = p.IsInstrument ? "VSTi" : "VST";
                Console.WriteLine($"  - {p.Name} ({type})");
            }
        }
        Console.WriteLine("==========================\n");
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        lock (_lock)
        {
            _isDisposed = true;

            foreach (var plugin in _loadedPlugins.Values)
            {
                plugin.Dispose();
            }
            _loadedPlugins.Clear();
            _discoveredPlugins.Clear();
        }

        GC.SuppressFinalize(this);
    }
}
