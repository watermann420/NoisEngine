//Engine License (MEL) - Honor-Based Commercial Support
// copyright (c) 2026 MusicEngine Watermann420 and Contributors
// Created by Watermann420
// Description: Interface for components that report latency for Plugin Delay Compensation (PDC).

using System;

namespace MusicEngine.Core.PDC;

/// <summary>
/// Interface for audio components that report processing latency.
/// Implementing this interface allows the PDC system to track and compensate
/// for latency introduced by effects, plugins, or other processing.
/// </summary>
public interface ILatencyReporter
{
    /// <summary>
    /// Gets the current latency in samples introduced by this component.
    /// </summary>
    /// <remarks>
    /// This value represents the number of samples of delay introduced by the processing.
    /// For VST plugins, this typically corresponds to the plugin's reported latency.
    /// For built-in effects, this should reflect any lookahead or buffering requirements.
    /// </remarks>
    int LatencySamples { get; }

    /// <summary>
    /// Event raised when the latency of this component changes.
    /// </summary>
    /// <remarks>
    /// This event should be raised whenever the component's latency changes,
    /// such as when a plugin reports a new latency value or when effect
    /// parameters that affect latency are modified.
    /// </remarks>
    event EventHandler<LatencyChangedEventArgs>? LatencyChanged;
}
