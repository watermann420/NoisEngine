//Engine License (MEL) - Honor-Based Commercial Support
// copyright (c) 2026 MusicEngine Watermann420 and Contributors
// Created by Watermann420
// Description: Represents an audio clip in the arrangement timeline.

using System;

namespace MusicEngine.Core;

/// <summary>
/// Defines the type of fade curve.
/// </summary>
public enum FadeType
{
    /// <summary>Linear fade (constant rate).</summary>
    Linear,

    /// <summary>Exponential fade (fast start, slow end).</summary>
    Exponential,

    /// <summary>Logarithmic fade (slow start, fast end).</summary>
    Logarithmic,

    /// <summary>S-Curve fade (slow start, fast middle, slow end).</summary>
    SCurve,

    /// <summary>Equal power crossfade curve.</summary>
    EqualPower
}

/// <summary>
/// Represents an audio clip in the arrangement timeline.
/// Audio clips reference audio files and can be positioned, trimmed, and faded.
/// </summary>
public class AudioClip : IEquatable<AudioClip>
{
    /// <summary>Unique identifier for this clip.</summary>
    public Guid Id { get; } = Guid.NewGuid();

    /// <summary>Path to the audio file.</summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>Display name of the clip.</summary>
    public string Name { get; set; } = "Audio Clip";

    /// <summary>Start position in the timeline (in beats).</summary>
    public double StartPosition { get; set; }

    /// <summary>Length of the clip in beats (after trimming).</summary>
    public double Length { get; set; }

    /// <summary>Original length of the source audio in beats.</summary>
    public double OriginalLength { get; set; }

    /// <summary>Offset into the source audio (for trimming start, in beats).</summary>
    public double SourceOffset { get; set; }

    /// <summary>Index of the track this clip belongs to.</summary>
    public int TrackIndex { get; set; }

    /// <summary>Fade-in duration in beats.</summary>
    public double FadeInDuration { get; set; }

    /// <summary>Fade-in curve type.</summary>
    public FadeType FadeInType { get; set; } = FadeType.Linear;

    /// <summary>Fade-out duration in beats.</summary>
    public double FadeOutDuration { get; set; }

    /// <summary>Fade-out curve type.</summary>
    public FadeType FadeOutType { get; set; } = FadeType.Linear;

    /// <summary>Gain adjustment in dB (-inf to +12).</summary>
    public float GainDb { get; set; }

    /// <summary>Linear gain multiplier (calculated from GainDb).</summary>
    public float Gain => GainDb <= -96f ? 0f : (float)Math.Pow(10, GainDb / 20.0);

    /// <summary>Whether this clip is muted.</summary>
    public bool IsMuted { get; set; }

    /// <summary>Whether this clip is locked from editing.</summary>
    public bool IsLocked { get; set; }

    /// <summary>Whether this clip is selected in the UI.</summary>
    public bool IsSelected { get; set; }

    /// <summary>Color for visual representation (hex format).</summary>
    public string Color { get; set; } = "#3498DB";

    /// <summary>Time stretch factor (1.0 = original speed).</summary>
    public double TimeStretchFactor { get; set; } = 1.0;

    /// <summary>Pitch shift in semitones.</summary>
    public double PitchShiftSemitones { get; set; }

    /// <summary>Whether the clip audio is reversed.</summary>
    public bool IsReversed { get; set; }

    /// <summary>Whether warp/time-stretch is enabled.</summary>
    public bool IsWarpEnabled { get; set; }

    /// <summary>Creation timestamp.</summary>
    public DateTime CreatedAt { get; } = DateTime.UtcNow;

    /// <summary>Last modification timestamp.</summary>
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the end position of this clip in the timeline.
    /// </summary>
    public double EndPosition => StartPosition + Length;

    /// <summary>
    /// Creates a new audio clip.
    /// </summary>
    public AudioClip()
    {
    }

    /// <summary>
    /// Creates a new audio clip with file path and position.
    /// </summary>
    /// <param name="filePath">Path to the audio file.</param>
    /// <param name="startPosition">Start position in beats.</param>
    /// <param name="length">Length in beats.</param>
    /// <param name="trackIndex">Track index.</param>
    public AudioClip(string filePath, double startPosition, double length, int trackIndex = 0)
    {
        FilePath = filePath;
        Name = System.IO.Path.GetFileNameWithoutExtension(filePath);
        StartPosition = startPosition;
        Length = length;
        OriginalLength = length;
        TrackIndex = trackIndex;
    }

    /// <summary>
    /// Checks if a given position falls within this clip.
    /// </summary>
    /// <param name="position">Position in beats to check.</param>
    /// <returns>True if the position is within the clip.</returns>
    public bool ContainsPosition(double position)
    {
        return position >= StartPosition && position < EndPosition;
    }

    /// <summary>
    /// Gets the position within the source audio for a given timeline position.
    /// </summary>
    /// <param name="timelinePosition">Position in the timeline (in beats).</param>
    /// <returns>Position in the source audio (in beats), or -1 if outside clip.</returns>
    public double GetSourcePosition(double timelinePosition)
    {
        if (!ContainsPosition(timelinePosition))
            return -1;

        var clipOffset = timelinePosition - StartPosition;
        return SourceOffset + (clipOffset / TimeStretchFactor);
    }

    /// <summary>
    /// Calculates the fade gain at a given position within the clip.
    /// </summary>
    /// <param name="positionInClip">Position within the clip (0 to Length).</param>
    /// <returns>Gain multiplier (0 to 1).</returns>
    public float GetFadeGainAt(double positionInClip)
    {
        if (positionInClip < 0 || positionInClip > Length)
            return 0f;

        float fadeGain = 1f;

        // Apply fade-in
        if (FadeInDuration > 0 && positionInClip < FadeInDuration)
        {
            var t = positionInClip / FadeInDuration;
            fadeGain *= CalculateFadeCurve(t, FadeInType);
        }

        // Apply fade-out
        var fadeOutStart = Length - FadeOutDuration;
        if (FadeOutDuration > 0 && positionInClip > fadeOutStart)
        {
            var t = (positionInClip - fadeOutStart) / FadeOutDuration;
            fadeGain *= CalculateFadeCurve(1 - t, FadeOutType);
        }

        return fadeGain;
    }

    /// <summary>
    /// Calculates the fade curve value for a given normalized position.
    /// </summary>
    private static float CalculateFadeCurve(double t, FadeType type)
    {
        t = Math.Clamp(t, 0, 1);

        return type switch
        {
            FadeType.Linear => (float)t,
            FadeType.Exponential => (float)(t * t),
            FadeType.Logarithmic => (float)Math.Sqrt(t),
            FadeType.SCurve => (float)(t * t * (3 - 2 * t)),
            FadeType.EqualPower => (float)Math.Sin(t * Math.PI / 2),
            _ => (float)t
        };
    }

    /// <summary>
    /// Moves the clip to a new position.
    /// </summary>
    /// <param name="newStartPosition">New start position in beats.</param>
    public void MoveTo(double newStartPosition)
    {
        if (IsLocked) return;
        StartPosition = newStartPosition;
        Touch();
    }

    /// <summary>
    /// Trims the clip from the start.
    /// </summary>
    /// <param name="trimAmount">Amount to trim in beats.</param>
    public void TrimStart(double trimAmount)
    {
        if (IsLocked) return;
        if (trimAmount <= 0 || trimAmount >= Length) return;

        StartPosition += trimAmount;
        SourceOffset += trimAmount / TimeStretchFactor;
        Length -= trimAmount;
        Touch();
    }

    /// <summary>
    /// Trims the clip from the end.
    /// </summary>
    /// <param name="trimAmount">Amount to trim in beats.</param>
    public void TrimEnd(double trimAmount)
    {
        if (IsLocked) return;
        if (trimAmount <= 0 || trimAmount >= Length) return;

        Length -= trimAmount;
        Touch();
    }

    /// <summary>
    /// Sets the fade-in parameters.
    /// </summary>
    /// <param name="duration">Fade duration in beats.</param>
    /// <param name="type">Fade curve type.</param>
    public void SetFadeIn(double duration, FadeType type = FadeType.Linear)
    {
        FadeInDuration = Math.Max(0, Math.Min(duration, Length - FadeOutDuration));
        FadeInType = type;
        Touch();
    }

    /// <summary>
    /// Sets the fade-out parameters.
    /// </summary>
    /// <param name="duration">Fade duration in beats.</param>
    /// <param name="type">Fade curve type.</param>
    public void SetFadeOut(double duration, FadeType type = FadeType.Linear)
    {
        FadeOutDuration = Math.Max(0, Math.Min(duration, Length - FadeInDuration));
        FadeOutType = type;
        Touch();
    }

    /// <summary>
    /// Splits this clip at the specified position.
    /// </summary>
    /// <param name="splitPosition">Position to split at (in timeline beats).</param>
    /// <returns>The new clip created after the split point, or null if split is invalid.</returns>
    public AudioClip? Split(double splitPosition)
    {
        if (IsLocked) return null;
        if (!ContainsPosition(splitPosition)) return null;
        if (splitPosition <= StartPosition || splitPosition >= EndPosition) return null;

        var splitOffset = splitPosition - StartPosition;
        var newClipLength = Length - splitOffset;

        // Create new clip for the second part
        var newClip = new AudioClip
        {
            FilePath = FilePath,
            Name = Name + " (split)",
            StartPosition = splitPosition,
            Length = newClipLength,
            OriginalLength = OriginalLength,
            SourceOffset = SourceOffset + (splitOffset / TimeStretchFactor),
            TrackIndex = TrackIndex,
            GainDb = GainDb,
            Color = Color,
            TimeStretchFactor = TimeStretchFactor,
            PitchShiftSemitones = PitchShiftSemitones,
            IsReversed = IsReversed,
            IsWarpEnabled = IsWarpEnabled,
            FadeOutDuration = FadeOutDuration,
            FadeOutType = FadeOutType
        };

        // Adjust this clip
        Length = splitOffset;
        FadeOutDuration = 0;
        Touch();

        return newClip;
    }

    /// <summary>
    /// Creates a duplicate of this clip at a new position.
    /// </summary>
    /// <param name="newStartPosition">Start position for the duplicate.</param>
    /// <returns>A new clip with the same properties.</returns>
    public AudioClip Duplicate(double? newStartPosition = null)
    {
        return new AudioClip
        {
            FilePath = FilePath,
            Name = Name + " (copy)",
            StartPosition = newStartPosition ?? EndPosition,
            Length = Length,
            OriginalLength = OriginalLength,
            SourceOffset = SourceOffset,
            TrackIndex = TrackIndex,
            FadeInDuration = FadeInDuration,
            FadeInType = FadeInType,
            FadeOutDuration = FadeOutDuration,
            FadeOutType = FadeOutType,
            GainDb = GainDb,
            Color = Color,
            TimeStretchFactor = TimeStretchFactor,
            PitchShiftSemitones = PitchShiftSemitones,
            IsReversed = IsReversed,
            IsWarpEnabled = IsWarpEnabled
        };
    }

    /// <summary>
    /// Updates the modification timestamp.
    /// </summary>
    public void Touch()
    {
        ModifiedAt = DateTime.UtcNow;
    }

    public bool Equals(AudioClip? other)
    {
        if (other is null) return false;
        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj) => obj is AudioClip other && Equals(other);

    public override int GetHashCode() => Id.GetHashCode();

    public static bool operator ==(AudioClip? left, AudioClip? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(AudioClip? left, AudioClip? right) => !(left == right);

    public override string ToString() =>
        $"[Audio] {Name} @{StartPosition:F2} ({Length:F2} beats) Track {TrackIndex}";
}
