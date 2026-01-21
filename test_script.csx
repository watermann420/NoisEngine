// Create a pattern with the default synth
var pattern = CreatePattern();
pattern.LoopLength = 4.0;

// Add notes: Note(midiNote, beat, duration, velocity)
pattern.Note(60, 0.0, 0.25, 100);
pattern.Note(62, 0.5, 0.25, 90);
pattern.Note(20, 1.0, 0.5, 110);

// Start the pattern
pattern.Play();

