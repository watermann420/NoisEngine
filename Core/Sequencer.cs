//Engine License (MEL) – Honor-Based Commercial Support
// copyright (c) 2026 MusicEngine Watermann420 and Contributors
// Created by Watermann420
// Description: A class for sequencing MIDI patterns and controlling playback.


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;


namespace MusicEngine.Core;


public class Sequencer
{
    private readonly List<Pattern> _patterns = new(); // patterns to play
    private bool _running; // is the sequencer running?
    private double _bpm = 120.0; // beats per minute
    private Thread? _thread; // playback thread
    private double _beatAccumulator = 0; // current beat position
    private bool _isScratching = false; // is scratching mode enabled?
    
    // Properties for BPM and current beat
    public double Bpm
    {
        get => _bpm;
        set => _bpm = Math.Max(1.0, value);
    }
    
    // Current beat position in the sequencer
    public double CurrentBeat
    {
        get => _beatAccumulator; // in beats
        set
        {
            lock (_patterns)
            {
                _beatAccumulator = value; 
            }
        }
    }
    
    // Scratching mode property to control playback behavior
    public bool IsScratching
    {
        get => _isScratching;
        set => _isScratching = value;
    }
    
    // Methods to add, clear patterns and control playback
    public void AddPattern(Pattern pattern)
    {
        lock (_patterns)
        {
            _patterns.Add(pattern);
        }
    }
    
    // Clear all patterns and stop their notes
    public void ClearPatterns()
    {
        lock (_patterns)
        {
            foreach (var pattern in _patterns)
            {
                pattern.Synth.AllNotesOff();
            }
            _patterns.Clear();
        }
    }
    
    // Start the sequencer
    public void Start()
    {
        if (_running) return;
        _running = true;
        _thread = new Thread(Run) { IsBackground = true, Priority = ThreadPriority.Highest };
        _thread.Start();
    }
    
    // Stop the sequencer
    public void Stop()
    {
        _running = false;
        _thread?.Join();
    }
    
    // Skip ahead by a certain number of beats
    public void Skip(double beats)
    {
        lock (_patterns)
        {
            _beatAccumulator += beats;
        }
    }
    
    // Main playback loop
    private void Run()
    {
        var stopwatch = Stopwatch.StartNew(); // High-resolution timer
        double lastTime = stopwatch.Elapsed.TotalSeconds; // Last time checkpoint
        double lastProcessedBeat = _beatAccumulator; // Last processed beat position

        while (_running)
        {
            double currentTime = stopwatch.Elapsed.TotalSeconds; // Current time
            double deltaTime = currentTime - lastTime; // Time delta
            lastTime = currentTime; // Update last time

            double nextBeat; // Next beat position
            lock (_patterns)
            {
                if (!_isScratching)
                {
                    double secondsPerBeat = 60.0 / _bpm; // Calculate seconds per beat
                    double beatsInDelta = deltaTime / secondsPerBeat; // Beats to advance
                    nextBeat = _beatAccumulator + beatsInDelta; // Update next beat
                }
                else
                {
                    nextBeat = _beatAccumulator; // In scratching mode, don't advance
                }

                if (nextBeat != lastProcessedBeat) // Process patterns if beat has changed
                {
                    foreach (var pattern in _patterns) // Process each pattern
                    {
                        pattern.Process(lastProcessedBeat, nextBeat, _bpm); // Process pattern for the beat range
                    }
                    lastProcessedBeat = nextBeat; // Update last processed beat
                }

                if (!_isScratching) // Update beat accumulator if not scratching
                {
                    _beatAccumulator = nextBeat; // Update current beat position
                }
            }

            Thread.Sleep(Settings.MidiRefreshRateMs); // Sleep to control update rate
        }
    }
}

// Represents a musical pattern with note events and playback properties
public class Pattern
{
    public ISynth Synth { get; set; } // Synthesizer to play the pattern
    public List<NoteEvent> Events { get; set; } = new(); // Note events in the pattern
    public double LoopLength { get; set; } = 4.0; // in beats
    public bool IsLooping { get; set; } = true; // Looping flag
    public double? StartBeat { get; set; } = null; // When to start the pattern
    public bool Enabled { get; set; } = true;  // Is the pattern enabled?
    
    
    // Constructor to initialize the pattern with a synth
    public Pattern(ISynth synth)
    {
        Synth = synth;
    }
    
    
    // Process the pattern for the given beat range
    public void Process(double startBeat, double endBeat, double bpm = 120.0)
    {
        if (!Enabled)
        {
            Synth.AllNotesOff(); // Stop all notes if disabled
            return;
        }
        if (StartBeat == null) StartBeat = startBeat; // Initialize start beat if not set
        
        double relativeStart = startBeat - StartBeat.Value; // Relative start beat
        double relativeEnd = endBeat - StartBeat.Value; // Relative end beat

        if (!IsLooping && relativeStart >= LoopLength) return;

        double startMod = relativeStart % LoopLength; // Modulo for looping
        double endMod = relativeEnd % LoopLength; // Modulo for looping
            
        
        if (startMod < 0) startMod += LoopLength; // Adjust negative modulo
        if (endMod < 0) endMod += LoopLength; // Adjust negative modulo

        bool wrapped = endMod < startMod; // Check if wrapped around
            
        if (relativeEnd < relativeStart) // Backwards scratching
        {
            wrapped = endMod > startMod; // Adjust wrap check for backwards
        }
        
        foreach (var ev in Events) // Process each note event
        {
            bool trigger = false;
            
            if (!IsLooping)
            {
                // Only trigger if it's within the first iteration of the pattern's life
                if (relativeEnd >= relativeStart)
                {
                    if (ev.Beat >= relativeStart && ev.Beat < relativeEnd && ev.Beat < LoopLength) trigger = true; // Forward playback
                }
                else
                {
                    if (ev.Beat >= relativeEnd && ev.Beat < relativeStart && ev.Beat < LoopLength) trigger = true; // Backward playback
                }
            }
            else
            {
                if (relativeEnd >= relativeStart)
                {
                    if (!wrapped)
                    {
                        if (ev.Beat >= startMod && ev.Beat < endMod) trigger = true; // Normal case
                    }
                    else // Wrap around
                    {
                        if (ev.Beat >= startMod || ev.Beat < endMod) trigger = true; // Wrapped case
                    }
                }
                else // Backwards
                {
                    if (!wrapped)
                    {
                        if (ev.Beat >= endMod && ev.Beat < startMod) trigger = true; // Normal backwards
                    }
                    else // Wrap around backwards
                    {
                        if (ev.Beat >= endMod || ev.Beat < startMod) trigger = true; // Wrapped backwards
                    }
                }
            }

            if (trigger)
            {
                Synth.NoteOn(ev.Note, ev.Velocity); // Trigger note on
                ThreadPool.QueueUserWorkItem(_ => { // Schedule note off
                    Thread.Sleep((int)(ev.Duration * (60000.0 / bpm)));  // Duration in ms
                    Synth.NoteOff(ev.Note); // Note off
                });
            }
        }
    }
}
    
// Represents a single note event in a pattern
public class NoteEvent
{
    public double Beat { get; set; } // in beats
    public int Note { get; set; } // MIDI note number
    public int Velocity { get; set; } // 0-127
    public double Duration { get; set; } = 0.25; // in beats
}
