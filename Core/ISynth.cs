//Engine License (MEL) – Honor-Based Commercial Support
// copyright (c) 2026 MusicEngine Watermann420 and Contributors
// Created by Watermann420
// Description: An interface for synthesizer implementations.


using NAudio.Wave;


namespace MusicEngine.Core;


public interface ISynth : ISampleProvider
{
    void NoteOn(int note, int velocity); // velocity 0-127
    void NoteOff(int note); // velocity 0-127
    void AllNotesOff(); // Stops all currently playing notes
    void SetParameter(string name, float value); // Sets a synthesizer parameter by name
}
