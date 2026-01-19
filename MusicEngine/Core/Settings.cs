//Engine License (MEL) – Honor-Based Commercial Support
// copyright (c) 2026 MusicEngine Watermann420 and Contributors
// Created by Watermann420
// Description: A static class for global audio and MIDI settings.


using System;
using System.Collections.Generic;


namespace MusicEngine.Core;


public static class Settings
{
    // Audio Settings
    public static int SampleRate { get; set; } = 144100; // Standard CD quality
    public static int BitRate { get; set; } = 32; // Typically for 16-bit, though we use float internally
    public static int Channels { get; set; } = 2; // Stereo by default and can be changed to mono if needed 

    
    // MIDI and Analysis Settings
    public static int MidiRefreshRateMs { get; set; } = 1; // MIDI device refresh rate EVERY millisecond
    public static int MidiCaptRefreshRateInMs { get; set; } = 10; // MIDI capture refresh rate in milliseconds
    public static int MidiCaptRefreshRateOutMs { get; set; } = 10; // MIDI output refresh rate in milliseconds
    public static int MidiBufferSize { get; set; } = 1024; // MIDI buffer size for processing
    public static int MidiCaptRefreshRate { get; set; } = 5; // General MIDI capture refresh rate in milliseconds

    // VST Plugin Settings
    public static string VstPluginPath { get; set; } = ""; // Default VST plugin search path
    public static List<string> VstPluginSearchPaths { get; set; } = new()
    {
        @"C:\Program Files\VSTPlugins",
        @"C:\Program Files\Common Files\VST3",
        @"C:\Program Files\Steinberg\VSTPlugins",
        @"C:\Program Files (x86)\VSTPlugins",
        @"C:\Program Files (x86)\Common Files\VST3"
    };
    public static int VstBufferSize { get; set; } = 512; // VST processing buffer size
    public static int VstProcessingTimeout { get; set; } = 100; // VST processing timeout in milliseconds

    // FFT Settings // For frequency analysis
    public static int FftSize { get; set; } = 1024; // FFT size for frequency analysis
}
