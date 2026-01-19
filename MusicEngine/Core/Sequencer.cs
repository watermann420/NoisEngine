//Engine License (MEL) – Honor-Based Commercial Support
// copyright (c) 2026 MusicEngine Watermann420 and Contributors
// Created by Watermann420
// Description: A class for sequencing MIDI patterns and controlling playback with event emission.


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
    private double _defaultLoopLength = 4.0; // default loop length for beat events

    // Event emission for visualization
    private readonly object _eventLock = new();
    private readonly List<MusicalEvent> _activeEvents = new(); // currently playing events

    /// <summary>Fired when a note is triggered (NoteOn).</summary>
    public event EventHandler<MusicalEventArgs>? NoteTriggered;

    /// <summary>Fired when a note ends (NoteOff).</summary>
    public event EventHandler<MusicalEventArgs>? NoteEnded;

    /// <summary>Fired on every beat update (high frequency, ~60fps).</summary>
    public event EventHandler<BeatChangedEventArgs>? BeatChanged;

    /// <summary>Fired when playback starts.</summary>
    public event EventHandler<PlaybackStateEventArgs>? PlaybackStarted;

    /// <summary>Fired when playback stops.</summary>
    public event EventHandler<PlaybackStateEventArgs>? PlaybackStopped;

    /// <summary>Fired when BPM changes.</summary>
    public event EventHandler<ParameterChangedEventArgs>? BpmChanged;

    /// <summary>Fired when a pattern is added.</summary>
    public event EventHandler<Pattern>? PatternAdded;

    /// <summary>Fired when a pattern is removed.</summary>
    public event EventHandler<Pattern>? PatternRemoved;

    /// <summary>Fired when patterns are cleared.</summary>
    public event EventHandler? PatternsCleared;

    // Properties for BPM and current beat
    public double Bpm
    {
        get => _bpm;
        set
        {
            var oldBpm = _bpm;
            _bpm = Math.Max(1.0, value);
            if (Math.Abs(oldBpm - _bpm) > 0.001)
            {
                BpmChanged?.Invoke(this, new ParameterChangedEventArgs("Bpm", oldBpm, _bpm));
            }
        }
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

    /// <summary>Whether the sequencer is currently running.</summary>
    public bool IsRunning => _running;

    /// <summary>Gets currently active (playing) events for visualization.</summary>
    public IReadOnlyList<MusicalEvent> ActiveEvents
    {
        get
        {
            lock (_eventLock)
            {
                // Clean up expired events
                _activeEvents.RemoveAll(e => !e.IsPlaying);
                return _activeEvents.ToArray();
            }
        }
    }

    /// <summary>Gets all patterns.</summary>
    public IReadOnlyList<Pattern> Patterns
    {
        get
        {
            lock (_patterns)
            {
                return _patterns.ToArray();
            }
        }
    }

    /// <summary>Default loop length for beat change events when no patterns exist.</summary>
    public double DefaultLoopLength
    {
        get => _defaultLoopLength;
        set => _defaultLoopLength = Math.Max(0.25, value);
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
            pattern.PatternIndex = _patterns.Count;
            pattern.Sequencer = this; // Link pattern to sequencer for event emission
            _patterns.Add(pattern);
        }
        PatternAdded?.Invoke(this, pattern);
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
        lock (_eventLock)
        {
            _activeEvents.Clear();
        }
        PatternsCleared?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Removes a specific pattern.</summary>
    public void RemovePattern(Pattern pattern)
    {
        lock (_patterns)
        {
            pattern.Synth.AllNotesOff();
            _patterns.Remove(pattern);
            // Re-index remaining patterns
            for (int i = 0; i < _patterns.Count; i++)
            {
                _patterns[i].PatternIndex = i;
            }
        }
        PatternRemoved?.Invoke(this, pattern);
    }

    // Start the sequencer
    public void Start()
    {
        if (_running) return;
        _running = true;
        _thread = new Thread(Run) { IsBackground = true, Priority = ThreadPriority.Highest };
        _thread.Start();
        PlaybackStarted?.Invoke(this, new PlaybackStateEventArgs(true, _beatAccumulator, _bpm));
    }

    // Stop the sequencer
    public void Stop()
    {
        if (!_running) return;
        _running = false;
        _thread?.Join();

        // Clear active events
        lock (_eventLock)
        {
            _activeEvents.Clear();
        }

        PlaybackStopped?.Invoke(this, new PlaybackStateEventArgs(false, _beatAccumulator, _bpm));
    }

    // Skip ahead by a certain number of beats
    public void Skip(double beats)
    {
        lock (_patterns)
        {
            _beatAccumulator += beats;
        }
    }

    /// <summary>Internal method called by Pattern when a note is triggered.</summary>
    internal void OnNoteTriggered(MusicalEvent musicalEvent)
    {
        lock (_eventLock)
        {
            _activeEvents.Add(musicalEvent);
        }
        NoteTriggered?.Invoke(this, new MusicalEventArgs(musicalEvent));
    }

    /// <summary>Internal method called by Pattern when a note ends.</summary>
    internal void OnNoteEnded(MusicalEvent musicalEvent)
    {
        lock (_eventLock)
        {
            _activeEvents.RemoveAll(e => e.Id == musicalEvent.Id);
        }
        NoteEnded?.Invoke(this, new MusicalEventArgs(musicalEvent));
    }

    // Main playback loop
    private void Run()
    {
        var stopwatch = Stopwatch.StartNew(); // High-resolution timer
        double lastTime = stopwatch.Elapsed.TotalSeconds; // Last time checkpoint
        double lastProcessedBeat = _beatAccumulator; // Last processed beat position
        double lastBeatEventTime = 0; // For throttling beat events
        const double beatEventInterval = 1.0 / 60.0; // ~60fps for beat events

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

            // Emit beat changed event at ~60fps
            if (currentTime - lastBeatEventTime >= beatEventInterval)
            {
                lastBeatEventTime = currentTime;
                EmitBeatChanged();
            }

            Thread.Sleep(Settings.MidiRefreshRateMs); // Sleep to control update rate
        }
    }

    private void EmitBeatChanged()
    {
        // Calculate cycle position based on first pattern or default
        double loopLength;
        lock (_patterns)
        {
            loopLength = _patterns.Count > 0 ? _patterns[0].LoopLength : _defaultLoopLength;
        }

        double cyclePosition = _beatAccumulator % loopLength;
        if (cyclePosition < 0) cyclePosition += loopLength;

        BeatChanged?.Invoke(this, new BeatChangedEventArgs(_beatAccumulator, cyclePosition, loopLength, _bpm));
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

    // New properties for visualization
    internal int PatternIndex { get; set; } = 0; // Index in sequencer's pattern list
    internal Sequencer? Sequencer { get; set; } // Reference to parent sequencer

    /// <summary>Name of this pattern for display purposes.</summary>
    public string Name { get; set; } = "";

    /// <summary>Name of the instrument/synth for this pattern.</summary>
    public string InstrumentName { get; set; } = "";

    /// <summary>Code source info for the entire pattern definition.</summary>
    public CodeSourceInfo? SourceInfo { get; set; }

    /// <summary>Unique identifier for this pattern.</summary>
    public Guid Id { get; } = Guid.NewGuid();


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

        int cycleNumber = (int)(relativeEnd / LoopLength);

        for (int noteIndex = 0; noteIndex < Events.Count; noteIndex++) // Process each note event
        {
            var ev = Events[noteIndex];
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
                TriggerNote(ev, noteIndex, endBeat, cycleNumber, bpm);
            }
        }
    }

    private void TriggerNote(NoteEvent ev, int noteIndex, double absoluteBeat, int cycleNumber, double bpm)
    {
        // Create the musical event with full information
        var durationMs = ev.Duration * (60000.0 / bpm);
        var now = DateTime.Now;

        var musicalEvent = new MusicalEvent
        {
            Id = new EventId(PatternIndex, noteIndex),
            SourcePattern = this,
            NoteEvent = ev,
            Note = ev.Note,
            NoteName = MusicalEvent.GetNoteName(ev.Note),
            Velocity = ev.Velocity,
            Duration = ev.Duration,
            CyclePosition = ev.Beat,
            AbsoluteBeat = absoluteBeat,
            CycleNumber = cycleNumber,
            LoopLength = LoopLength,
            InstrumentName = !string.IsNullOrEmpty(InstrumentName) ? InstrumentName : $"Pattern {PatternIndex}",
            SourceInfo = ev.SourceInfo ?? SourceInfo,
            TriggeredAt = now,
            EndsAt = now.AddMilliseconds(durationMs),
            IsNoteOn = true,
            Bpm = bpm
        };

        // Notify sequencer of the event
        Sequencer?.OnNoteTriggered(musicalEvent);

        // Trigger the note on the synth
        Synth.NoteOn(ev.Note, ev.Velocity);

        // Schedule note off
        ThreadPool.QueueUserWorkItem(_ =>
        {
            Thread.Sleep((int)durationMs);
            Synth.NoteOff(ev.Note);

            // Notify sequencer that note ended
            musicalEvent.IsNoteOn = false;
            Sequencer?.OnNoteEnded(musicalEvent);
        });
    }
}

// Represents a single note event in a pattern
public class NoteEvent
{
    public double Beat { get; set; } // in beats
    public int Note { get; set; } // MIDI note number
    public int Velocity { get; set; } // 0-127
    public double Duration { get; set; } = 0.25; // in beats

    /// <summary>Unique identifier for this note event.</summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>Code source info for highlighting this specific note.</summary>
    public CodeSourceInfo? SourceInfo { get; set; }

    /// <summary>Parameter bindings for live slider control.</summary>
    public Dictionary<string, LiveParameter>? Parameters { get; set; }
}

/// <summary>
/// Represents a parameter that can be controlled live via sliders.
/// </summary>
public class LiveParameter
{
    public string Name { get; set; } = "";
    public double Value { get; set; }
    public double MinValue { get; set; }
    public double MaxValue { get; set; }
    public double Step { get; set; } = 1.0;
    public CodeSourceInfo? SourceInfo { get; set; }

    /// <summary>Callback invoked when value changes.</summary>
    public Action<double>? OnValueChanged { get; set; }

    public void SetValue(double newValue)
    {
        Value = Math.Clamp(newValue, MinValue, MaxValue);
        OnValueChanged?.Invoke(Value);
    }
}
