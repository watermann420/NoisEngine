//Engine License (MEL) - Honor-Based Commercial Support
// copyright (c) 2026 MusicEngine Watermann420 and Contributors
// Created by Watermann420
// Description: Interface for audio effects that can be used in the NAudio pipeline.


using NAudio.Wave;


namespace MusicEngine.Core;


/// <summary>
/// Interface for audio effects that can be used in the NAudio pipeline.
/// All effects implement ISampleProvider so they can be chained together.
/// </summary>
public interface IEffect : ISampleProvider
{
    /// <summary>
    /// Gets the name of the effect.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets or sets the dry/wet mix ratio (0.0 = fully dry, 1.0 = fully wet).
    /// </summary>
    float Mix { get; set; }

    /// <summary>
    /// Gets or sets whether the effect is enabled.
    /// When disabled, the effect passes audio through unchanged.
    /// </summary>
    bool Enabled { get; set; }

    /// <summary>
    /// Sets a parameter value by name.
    /// </summary>
    /// <param name="name">The parameter name (case-insensitive)</param>
    /// <param name="value">The parameter value</param>
    void SetParameter(string name, float value);

    /// <summary>
    /// Gets a parameter value by name.
    /// </summary>
    /// <param name="name">The parameter name (case-insensitive)</param>
    /// <returns>The parameter value, or 0 if not found</returns>
    float GetParameter(string name);
}
