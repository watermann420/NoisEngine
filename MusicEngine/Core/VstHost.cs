//Engine License (MEL) - Honor-Based Commercial Support
// copyright (c) 2026 MusicEngine Watermann420 and Contributors
// Created by Watermann420
// Description: VST Plugin Host for loading, managing, and processing VST plugins with MIDI support.


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using NAudio.Wave;


namespace MusicEngine.Core;


/// <summary>
/// Central VST Host that manages plugin discovery, loading, and lifecycle.
/// Provides utilities for preset management and parameter discovery across plugins.
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
                NumParameters = 0,
                NumPrograms = 1
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
                NumParameters = 0,
                NumPrograms = 1
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

    #region Preset Loading Utilities

    /// <summary>
    /// Scan a directory for preset files (.fxp, .fxb) compatible with a plugin
    /// </summary>
    /// <param name="directory">Directory to scan</param>
    /// <param name="pluginName">Optional plugin name to filter by (searches in subdirectories)</param>
    /// <returns>List of preset file paths</returns>
    public List<string> ScanForPresets(string directory, string? pluginName = null)
    {
        var presets = new List<string>();

        if (!Directory.Exists(directory))
        {
            return presets;
        }

        try
        {
            // If plugin name specified, try to find plugin-specific folder first
            if (!string.IsNullOrEmpty(pluginName))
            {
                var pluginDir = Path.Combine(directory, pluginName);
                if (Directory.Exists(pluginDir))
                {
                    presets.AddRange(Directory.GetFiles(pluginDir, "*.fxp", SearchOption.AllDirectories));
                    presets.AddRange(Directory.GetFiles(pluginDir, "*.fxb", SearchOption.AllDirectories));
                }
            }

            // Also scan the main directory
            presets.AddRange(Directory.GetFiles(directory, "*.fxp", SearchOption.AllDirectories));
            presets.AddRange(Directory.GetFiles(directory, "*.fxb", SearchOption.AllDirectories));

            // Remove duplicates
            presets = presets.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scanning for presets: {ex.Message}");
        }

        return presets;
    }

    /// <summary>
    /// Get common preset directories for a platform
    /// </summary>
    /// <returns>List of common preset directories</returns>
    public static List<string> GetCommonPresetDirectories()
    {
        var directories = new List<string>();

        // Windows common preset locations
        string? userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string? appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string? localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        string? documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        if (!string.IsNullOrEmpty(documents))
        {
            directories.Add(Path.Combine(documents, "VST Presets"));
            directories.Add(Path.Combine(documents, "VST3 Presets"));
            directories.Add(Path.Combine(documents, "Presets"));
        }

        if (!string.IsNullOrEmpty(appData))
        {
            directories.Add(Path.Combine(appData, "VST Presets"));
            directories.Add(Path.Combine(appData, "VST3 Presets"));
        }

        // Steinberg standard location
        directories.Add(@"C:\Users\Public\Documents\Steinberg\VST Presets");
        directories.Add(@"C:\ProgramData\Steinberg\VST Presets");

        return directories.Where(Directory.Exists).ToList();
    }

    /// <summary>
    /// Load a preset file into a plugin by name
    /// </summary>
    /// <param name="pluginName">Name of the loaded plugin</param>
    /// <param name="presetPath">Path to the preset file</param>
    /// <returns>True if successful</returns>
    public bool LoadPresetForPlugin(string pluginName, string presetPath)
    {
        var plugin = GetPlugin(pluginName);
        if (plugin == null)
        {
            Console.WriteLine($"Plugin '{pluginName}' not loaded");
            return false;
        }

        return plugin.LoadPreset(presetPath);
    }

    /// <summary>
    /// Save a plugin's current state to a preset file
    /// </summary>
    /// <param name="pluginName">Name of the loaded plugin</param>
    /// <param name="presetPath">Path for the preset file</param>
    /// <returns>True if successful</returns>
    public bool SavePresetForPlugin(string pluginName, string presetPath)
    {
        var plugin = GetPlugin(pluginName);
        if (plugin == null)
        {
            Console.WriteLine($"Plugin '{pluginName}' not loaded");
            return false;
        }

        return plugin.SavePreset(presetPath);
    }

    /// <summary>
    /// Create a preset bank from multiple preset files
    /// </summary>
    /// <param name="presetPaths">List of .fxp preset file paths</param>
    /// <param name="outputPath">Output .fxb bank file path</param>
    /// <returns>True if successful</returns>
    public bool CreatePresetBank(IEnumerable<string> presetPaths, string outputPath)
    {
        try
        {
            var presets = presetPaths.ToList();
            if (presets.Count == 0)
            {
                Console.WriteLine("No presets provided for bank creation");
                return false;
            }

            // Read and combine preset data (simplified implementation)
            // Full implementation would parse and combine FXP files properly
            using var output = File.Create(outputPath);
            using var writer = new BinaryWriter(output);

            // Write FXB header
            uint chunkMagic = 0x4B6E6343; // 'CcnK'
            uint bankMagic = 0x68436246;  // 'FBCh'

            writer.Write(SwapEndian(chunkMagic));
            // Size will be written at the end
            long sizePosition = output.Position;
            writer.Write(0); // Placeholder for size
            writer.Write(SwapEndian(bankMagic));
            writer.Write(SwapEndian(1u)); // Version
            writer.Write(0); // FX ID (placeholder)
            writer.Write(SwapEndian(1u)); // FX Version
            writer.Write(SwapEndian((uint)presets.Count)); // Num programs

            // Write current program (0)
            writer.Write(SwapEndian(0u));

            // Reserved (124 bytes)
            writer.Write(new byte[124]);

            // For each preset, read and write the program data
            foreach (var presetPath in presets)
            {
                if (File.Exists(presetPath))
                {
                    var presetData = File.ReadAllBytes(presetPath);
                    // Skip the header and write the program data
                    if (presetData.Length > 60)
                    {
                        writer.Write(presetData, 28, presetData.Length - 28);
                    }
                }
            }

            // Go back and write the size
            long endPosition = output.Position;
            output.Position = sizePosition;
            writer.Write(SwapEndian((uint)(endPosition - 8)));

            Console.WriteLine($"Created preset bank: {outputPath}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating preset bank: {ex.Message}");
            return false;
        }
    }

    private static uint SwapEndian(uint value)
    {
        return ((value & 0xFF) << 24) |
               ((value & 0xFF00) << 8) |
               ((value & 0xFF0000) >> 8) |
               ((value & 0xFF000000) >> 24);
    }

    #endregion

    #region Parameter Discovery Helpers

    /// <summary>
    /// Discover all parameters for a loaded plugin
    /// </summary>
    /// <param name="pluginName">Name of the loaded plugin</param>
    /// <returns>List of parameter info tuples (index, name, value, display)</returns>
    public List<(int Index, string Name, float Value, string Display)> DiscoverParameters(string pluginName)
    {
        var parameters = new List<(int Index, string Name, float Value, string Display)>();

        var plugin = GetPlugin(pluginName);
        if (plugin == null)
        {
            Console.WriteLine($"Plugin '{pluginName}' not loaded");
            return parameters;
        }

        int count = plugin.GetParameterCount();
        for (int i = 0; i < count; i++)
        {
            parameters.Add((
                i,
                plugin.GetParameterName(i),
                plugin.GetParameterValue(i),
                plugin.GetParameterDisplay(i)
            ));
        }

        return parameters;
    }

    /// <summary>
    /// Print all parameters for a plugin to console
    /// </summary>
    /// <param name="pluginName">Name of the loaded plugin</param>
    public void PrintParameters(string pluginName)
    {
        var plugin = GetPlugin(pluginName);
        if (plugin == null)
        {
            Console.WriteLine($"Plugin '{pluginName}' not loaded");
            return;
        }

        Console.WriteLine($"\n=== Parameters for {pluginName} ===");
        int count = plugin.GetParameterCount();

        if (count == 0)
        {
            Console.WriteLine("  No parameters available.");
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                string name = plugin.GetParameterName(i);
                float value = plugin.GetParameterValue(i);
                string display = plugin.GetParameterDisplay(i);
                Console.WriteLine($"  [{i}] {name}: {display} ({value:F3})");
            }
        }
        Console.WriteLine("==============================\n");
    }

    /// <summary>
    /// Find parameters by name pattern
    /// </summary>
    /// <param name="pluginName">Name of the loaded plugin</param>
    /// <param name="pattern">Search pattern (case-insensitive)</param>
    /// <returns>List of matching parameter indices</returns>
    public List<int> FindParameters(string pluginName, string pattern)
    {
        var matches = new List<int>();

        var plugin = GetPlugin(pluginName);
        if (plugin == null)
        {
            return matches;
        }

        int count = plugin.GetParameterCount();
        for (int i = 0; i < count; i++)
        {
            string name = plugin.GetParameterName(i);
            if (name.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                matches.Add(i);
            }
        }

        return matches;
    }

    /// <summary>
    /// Set multiple parameters at once
    /// </summary>
    /// <param name="pluginName">Name of the loaded plugin</param>
    /// <param name="parameters">Dictionary of parameter index to value</param>
    public void SetParameters(string pluginName, Dictionary<int, float> parameters)
    {
        var plugin = GetPlugin(pluginName);
        if (plugin == null)
        {
            Console.WriteLine($"Plugin '{pluginName}' not loaded");
            return;
        }

        foreach (var kvp in parameters)
        {
            plugin.SetParameterValue(kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Copy parameter values from one plugin to another (useful for A/B comparison)
    /// </summary>
    /// <param name="sourcePluginName">Source plugin name</param>
    /// <param name="destPluginName">Destination plugin name</param>
    /// <returns>Number of parameters copied</returns>
    public int CopyParameters(string sourcePluginName, string destPluginName)
    {
        var source = GetPlugin(sourcePluginName);
        var dest = GetPlugin(destPluginName);

        if (source == null || dest == null)
        {
            Console.WriteLine("Source or destination plugin not loaded");
            return 0;
        }

        int count = Math.Min(source.GetParameterCount(), dest.GetParameterCount());
        for (int i = 0; i < count; i++)
        {
            dest.SetParameterValue(i, source.GetParameterValue(i));
        }

        Console.WriteLine($"Copied {count} parameters from {sourcePluginName} to {destPluginName}");
        return count;
    }

    /// <summary>
    /// Create a snapshot of all parameter values for a plugin
    /// </summary>
    /// <param name="pluginName">Name of the loaded plugin</param>
    /// <returns>Dictionary of parameter index to value</returns>
    public Dictionary<int, float> CreateParameterSnapshot(string pluginName)
    {
        var snapshot = new Dictionary<int, float>();

        var plugin = GetPlugin(pluginName);
        if (plugin == null)
        {
            return snapshot;
        }

        int count = plugin.GetParameterCount();
        for (int i = 0; i < count; i++)
        {
            snapshot[i] = plugin.GetParameterValue(i);
        }

        return snapshot;
    }

    /// <summary>
    /// Restore parameters from a snapshot
    /// </summary>
    /// <param name="pluginName">Name of the loaded plugin</param>
    /// <param name="snapshot">Parameter snapshot to restore</param>
    public void RestoreParameterSnapshot(string pluginName, Dictionary<int, float> snapshot)
    {
        SetParameters(pluginName, snapshot);
    }

    /// <summary>
    /// Randomize parameters within specified ranges
    /// </summary>
    /// <param name="pluginName">Name of the loaded plugin</param>
    /// <param name="minValue">Minimum random value (0-1)</param>
    /// <param name="maxValue">Maximum random value (0-1)</param>
    /// <param name="excludeIndices">Parameter indices to exclude from randomization</param>
    public void RandomizeParameters(string pluginName, float minValue = 0f, float maxValue = 1f, HashSet<int>? excludeIndices = null)
    {
        var plugin = GetPlugin(pluginName);
        if (plugin == null)
        {
            Console.WriteLine($"Plugin '{pluginName}' not loaded");
            return;
        }

        var random = new Random();
        int count = plugin.GetParameterCount();

        for (int i = 0; i < count; i++)
        {
            if (excludeIndices != null && excludeIndices.Contains(i))
            {
                continue;
            }

            float value = minValue + (float)(random.NextDouble() * (maxValue - minValue));
            plugin.SetParameterValue(i, value);
        }

        Console.WriteLine($"Randomized {count} parameters for {pluginName}");
    }

    #endregion

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
