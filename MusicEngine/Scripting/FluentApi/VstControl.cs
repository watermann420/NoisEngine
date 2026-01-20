//Engine License (MEL) – Honor-Based Commercial Support
// copyright (c) 2026 MusicEngine Watermann420 and Contributors
// Created by Watermann420
// Description: Fluent API for VST plugin control operations.


using MusicEngine.Core;


namespace MusicEngine.Scripting.FluentApi;


// === VST Fluent API ===

// Main VST control access
public class VstControl
{
    private readonly ScriptGlobals _globals;
    public VstControl(ScriptGlobals globals) => _globals = globals;

    // Load a VST plugin by name or path
    public VstPluginControl? load(string nameOrPath)
    {
        var plugin = _globals.LoadVst(nameOrPath);
        return plugin != null ? new VstPluginControl(_globals, plugin) : null;
    }

    // Load a VST plugin by index
    public VstPluginControl? load(int index)
    {
        var plugin = _globals.LoadVstByIndex(index);
        return plugin != null ? new VstPluginControl(_globals, plugin) : null;
    }

    /// <summary>Alias for load - PascalCase version</summary>
    public VstPluginControl? Load(string nameOrPath) => load(nameOrPath);
    /// <summary>Alias for load - PascalCase version</summary>
    public VstPluginControl? Load(int index) => load(index);
    /// <summary>Alias for load - Loads a VST plugin</summary>
    public VstPluginControl? LoadPlugin(string nameOrPath) => load(nameOrPath);
    /// <summary>Alias for load - Loads a VST plugin</summary>
    public VstPluginControl? LoadPlugin(int index) => load(index);
    /// <summary>Alias for load - Short form</summary>
    public VstPluginControl? l(string nameOrPath) => load(nameOrPath);
    /// <summary>Alias for load - Short form</summary>
    public VstPluginControl? l(int index) => load(index);

    // Get a loaded VST plugin by name
    public VstPluginControl? get(string name)
    {
        var plugin = _globals.GetVst(name);
        return plugin != null ? new VstPluginControl(_globals, plugin) : null;
    }

    /// <summary>Alias for get - PascalCase version</summary>
    public VstPluginControl? Get(string name) => get(name);
    /// <summary>Alias for get - Access VST plugin by name</summary>
    public VstPluginControl? plugin(string name) => get(name);

    // List all discovered VST plugins
    public void list() => _globals.ListVstPlugins();

    /// <summary>Alias for list - PascalCase version</summary>
    public void List() => list();
    /// <summary>Alias for list - Show all VST plugins</summary>
    public void plugins() => list();

    // List all loaded VST plugins
    public void loaded() => _globals.ListLoadedVstPlugins();

    /// <summary>Alias for loaded - PascalCase version</summary>
    public void Loaded() => loaded();
    /// <summary>Alias for loaded - Show active VST plugins</summary>
    public void active() => loaded();
}


// Control for a specific VST plugin
public class VstPluginControl
{
    private readonly ScriptGlobals _globals;
    private readonly VstPlugin _plugin;

    public VstPluginControl(ScriptGlobals globals, VstPlugin plugin)
    {
        _globals = globals;
        _plugin = plugin;
    }

    // Get the underlying plugin
    public VstPlugin Plugin => _plugin;

    // Route MIDI input to this plugin
    public VstPluginControl from(int deviceIndex)
    {
        _globals.RouteToVst(deviceIndex, _plugin);
        return this;
    }

    // Route MIDI input by device name
    public VstPluginControl from(string deviceName)
    {
        int index = _globals.Engine.GetMidiDeviceIndex(deviceName);
        if (index >= 0) _globals.RouteToVst(index, _plugin);
        return this;
    }

    // Set a parameter by name
    public VstPluginControl param(string name, float value)
    {
        _plugin.SetParameter(name, value);
        return this;
    }

    // Set a parameter by index
    public VstPluginControl param(int index, float value)
    {
        _plugin.SetParameterByIndex(index, value);
        return this;
    }

    // Set volume/gain
    public VstPluginControl volume(float value)
    {
        _plugin.SetParameter("volume", value);
        return this;
    }

    // Send a note on
    public VstPluginControl noteOn(int note, int velocity = 100)
    {
        _plugin.NoteOn(note, velocity);
        return this;
    }

    // Send a note off
    public VstPluginControl noteOff(int note)
    {
        _plugin.NoteOff(note);
        return this;
    }

    // Send all notes off
    public VstPluginControl allNotesOff()
    {
        _plugin.AllNotesOff();
        return this;
    }

    // Send control change
    public VstPluginControl cc(int controller, int value, int channel = 0)
    {
        _plugin.SendControlChange(channel, controller, value);
        return this;
    }

    // Send program change
    public VstPluginControl program(int programNumber, int channel = 0)
    {
        _plugin.SendProgramChange(channel, programNumber);
        return this;
    }

    // Send pitch bend
    public VstPluginControl pitchBend(int value, int channel = 0)
    {
        _plugin.SendPitchBend(channel, value);
        return this;
    }

    // === Preset Management ===

    // Load a preset from file
    public VstPluginControl loadPreset(string path)
    {
        _plugin.LoadPreset(path);
        return this;
    }

    // Save current state to preset file
    public VstPluginControl savePreset(string path)
    {
        _plugin.SavePreset(path);
        return this;
    }

    // Set preset by index
    public VstPluginControl preset(int index)
    {
        _plugin.SetPreset(index);
        return this;
    }

    // Get current preset name
    public string currentPreset => _plugin.CurrentPresetName;

    // Get all preset names
    public IReadOnlyList<string> presets => _plugin.GetPresetNames();

    // List all available presets
    public VstPluginControl listPresets()
    {
        var names = _plugin.GetPresetNames();
        Console.WriteLine($"\n=== Presets for {_plugin.Name} ===");
        for (int i = 0; i < names.Count; i++)
        {
            var current = i == _plugin.CurrentPresetIndex ? " [CURRENT]" : "";
            Console.WriteLine($"  [{i}] {names[i]}{current}");
        }
        Console.WriteLine("==============================\n");
        return this;
    }

    // === Parameter Methods ===

    // Get parameter count
    public int paramCount => _plugin.GetParameterCount();

    // Get parameter name by index
    public string paramName(int index) => _plugin.GetParameterName(index);

    // Get parameter value by index
    public float paramValue(int index) => _plugin.GetParameterValue(index);

    // Get parameter display string by index
    public string paramDisplay(int index) => _plugin.GetParameterDisplay(index);

    // List all parameters
    public VstPluginControl listParams()
    {
        Console.WriteLine($"\n=== Parameters for {_plugin.Name} ===");
        int count = _plugin.GetParameterCount();
        if (count == 0)
        {
            Console.WriteLine("  No parameters available.");
        }
        else
        {
            for (int i = 0; i < count; i++)
            {
                string name = _plugin.GetParameterName(i);
                string display = _plugin.GetParameterDisplay(i);
                float value = _plugin.GetParameterValue(i);
                Console.WriteLine($"  [{i}] {name}: {display} ({value:F3})");
            }
        }
        Console.WriteLine("==============================\n");
        return this;
    }

    // === Parameter Automation ===

    // Create automation ramp for a parameter
    public VstPluginControl automate(int paramIndex, float startValue, float endValue, double durationBeats)
    {
        _plugin.AutomateParameter(paramIndex, startValue, endValue, durationBeats);
        return this;
    }

    // Clear automation for a parameter
    public VstPluginControl clearAutomation(int paramIndex)
    {
        _plugin.ClearAutomation(paramIndex);
        return this;
    }

    // Clear all automation
    public VstPluginControl clearAllAutomation()
    {
        _plugin.ClearAllAutomation();
        return this;
    }

    // Set current time for automation playback
    public VstPluginControl setTime(double beats)
    {
        _plugin.SetCurrentTimeBeats(beats);
        return this;
    }

    // Implicit conversion to VstPlugin for direct use
    public static implicit operator VstPlugin(VstPluginControl control) => control._plugin;
}
