# MusicEngine Scripting API Dokumentation

Diese Dokumentation beschreibt die vollstaendige Scripting API der MusicEngine. Die Scripting-Umgebung basiert auf C# und ermoeglicht die Steuerung von Audio-Engine, Sequenzer, MIDI-Geraeten, VST-Plugins und Sample-Instrumenten.

---

## Inhaltsverzeichnis

1. [Basis-Variablen](#1-basis-variablen)
2. [Synth Erstellung](#2-synth-erstellung)
3. [Sample Instrument API](#3-sample-instrument-api)
4. [Transport Controls](#4-transport-controls)
5. [Pattern Control](#5-pattern-control)
6. [MIDI Fluent API](#6-midi-fluent-api)
7. [Audio Controls](#7-audio-controls)
8. [VST Plugin API](#8-vst-plugin-api)
9. [Sample Fluent API](#9-sample-fluent-api)
10. [Hilfsfunktionen](#10-hilfsfunktionen)
11. [Frequenz-Trigger](#11-frequenz-trigger)
12. [Virtual Audio Channels](#12-virtual-audio-channels)

---

## 1. Basis-Variablen

Die Scripting-Umgebung stellt zwei zentrale Objekte bereit, die in jedem Skript verfuegbar sind:

| Variable | Typ | Beschreibung |
|----------|-----|--------------|
| `Engine` / `engine` | `AudioEngine` | Die Audio-Engine Instanz fuer Soundausgabe und MIDI-Routing |
| `Sequencer` / `sequencer` | `Sequencer` | Der Sequenzer fuer Pattern-Wiedergabe und Timing |

### Beispiel

```csharp
// Beide Schreibweisen sind aequivalent
engine.AddSampleProvider(synth);
Engine.AddSampleProvider(synth);

// Sequenzer starten
sequencer.Start();
Sequencer.Start();
```

---

## 2. Synth Erstellung

### CreateSynth()

Erstellt einen neuen `SimpleSynth` und fuegt ihn automatisch zur Audio-Engine hinzu.

```csharp
SimpleSynth CreateSynth()
```

**Rueckgabe:** Eine neue `SimpleSynth`-Instanz

### CreatePattern(synth)

Erstellt ein neues `Pattern` fuer einen Synthesizer und fuegt es zum Sequenzer hinzu.

```csharp
Pattern CreatePattern(ISynth synth)
```

**Parameter:**
- `synth` - Der Synthesizer, der vom Pattern gesteuert wird

**Rueckgabe:** Eine neue `Pattern`-Instanz

### Beispiel

```csharp
// Synth erstellen und Pattern hinzufuegen
var synth = CreateSynth();
var pattern = CreatePattern(synth);

// Pattern mit Noten fuellen
pattern.AddNote(0, 60, 100, 0.5);    // Beat 0: C4
pattern.AddNote(1, 64, 100, 0.5);    // Beat 1: E4
pattern.AddNote(2, 67, 100, 0.5);    // Beat 2: G4
pattern.AddNote(3, 72, 100, 0.5);    // Beat 3: C5

// Sequenzer starten
SetBpm(120);
Start();
```

---

## 3. Sample Instrument API

Die Sample Instrument API ermoeglicht das Erstellen von Sample-basierten Synthesizern.

### CreateSampler(name?)

Erstellt einen neuen `SampleInstrument` und fuegt ihn zur Audio-Engine hinzu.

```csharp
SampleInstrument CreateSampler(string? name = null)
```

**Parameter:**
- `name` (optional) - Name fuer den Sampler

**Rueckgabe:** Eine neue `SampleInstrument`-Instanz

### CreateSamplerFromFile(filePath, rootNote?)

Erstellt einen Sampler und laedt eine einzelne Audio-Datei. Das Sample wird auf alle Noten mit Pitch-Shifting vom Root-Note gemappt.

```csharp
SampleInstrument CreateSamplerFromFile(string filePath, int rootNote = 60)
```

**Parameter:**
- `filePath` - Pfad zur Audio-Datei (WAV, MP3, etc.)
- `rootNote` (optional) - MIDI-Note, bei der das Sample unveraendert abgespielt wird (Standard: 60 = C4)

**Rueckgabe:** Eine neue `SampleInstrument`-Instanz

### CreateSamplerFromDirectory(directoryPath)

Erstellt einen Sampler und laedt alle Samples aus einem Verzeichnis.

```csharp
SampleInstrument CreateSamplerFromDirectory(string directoryPath)
```

**Parameter:**
- `directoryPath` - Pfad zum Sample-Verzeichnis

**Rueckgabe:** Eine neue `SampleInstrument`-Instanz

### LoadSampleToNote(sampler, filePath, note)

Laedt ein Sample in einen bestehenden Sampler und mappt es auf eine bestimmte Note. Ideal fuer Drum-Pads.

```csharp
Sample? LoadSampleToNote(SampleInstrument sampler, string filePath, int note)
```

**Parameter:**
- `sampler` - Der Ziel-Sampler
- `filePath` - Pfad zur Audio-Datei
- `note` - MIDI-Note, auf die das Sample gemappt wird

**Rueckgabe:** Das geladene `Sample` oder `null` bei Fehler

### Beispiel

```csharp
// Einfaches Piano-Sample laden
var piano = CreateSamplerFromFile("C:/Samples/piano_c4.wav", 60);
var pianoPattern = CreatePattern(piano);

// Drum-Kit aus einzelnen Samples erstellen
var drums = CreateSampler("DrumKit");
LoadSampleToNote(drums, "C:/Samples/kick.wav", 36);
LoadSampleToNote(drums, "C:/Samples/snare.wav", 38);
LoadSampleToNote(drums, "C:/Samples/hihat.wav", 42);

// Sampler aus Verzeichnis laden
var strings = CreateSamplerFromDirectory("C:/Samples/Strings/");

SetBpm(120);
Start();
```

---

## 4. Transport Controls

Funktionen zur Steuerung der Wiedergabe.

### Start()

Startet den Sequenzer.

```csharp
void Start()
```

### Stop()

Stoppt den Sequenzer.

```csharp
void Stop()
```

### SetBpm(bpm)

Setzt das Tempo in Beats per Minute.

```csharp
void SetBpm(double bpm)
```

**Parameter:**
- `bpm` - Tempo in BPM (z.B. 120.0)

### Skip(beats)

Springt um eine bestimmte Anzahl von Beats vor oder zurueck.

```csharp
void Skip(double beats)
```

**Parameter:**
- `beats` - Anzahl der Beats (positiv = vorwaerts, negativ = rueckwaerts)

### SetScratching(scratching)

Aktiviert oder deaktiviert den Scratching-Modus.

```csharp
void SetScratching(bool scratching)
```

### Beispiel

```csharp
SetBpm(140);
Start();

// Nach 8 Beats springen
Skip(8);

// Zurueck zum Anfang
Skip(-8);

Stop();
```

---

## 5. Pattern Control

Funktionen zur Steuerung einzelner Patterns.

### StartPattern(pattern)

Startet ein bestimmtes Pattern.

```csharp
void StartPattern(Pattern p)
```

### StopPattern(pattern)

Stoppt ein bestimmtes Pattern.

```csharp
void StopPattern(Pattern p)
```

### patterns Objekt

Zugriff auf erweiterte Pattern-Kontrolle:

```csharp
patterns.start(pattern)   // Pattern starten
patterns.stop(pattern)    // Pattern stoppen
patterns.toggle(pattern)  // Pattern ein/aus schalten
```

### Beispiel

```csharp
var synth1 = CreateSynth();
var synth2 = CreateSynth();

var melody = CreatePattern(synth1);
var bass = CreatePattern(synth2);

// Nur Melodie starten
StartPattern(melody);
Start();

// Spaeter: Bass hinzufuegen
patterns.start(bass);

// Pattern umschalten
patterns.toggle(melody);
```

---

## 6. MIDI Fluent API

Die MIDI API bietet eine fluessige Syntax fuer MIDI-Konfiguration.

### midi Objekt

Hauptzugriffspunkt fuer alle MIDI-Operationen.

### Geraete-Zugriff

```csharp
// Nach Index
midi.device(0)                    // Erstes MIDI-Geraet
midi.input(0)                     // Alias fuer device()

// Nach Name
midi.device("Akai MPK")           // Geraet nach Name
midi.input("Arturia KeyLab")      // Alias fuer device()
```

### MIDI Routing

```csharp
// MIDI zu Synth routen
midi.device(0).route(synth)       // Alle Noten vom Geraet zum Synth
```

### Control Change (CC) Mapping

```csharp
// CC zu Parameter mappen
midi.device(0).cc(1).to(synth, "filterCutoff")   // CC1 (Modwheel) zu Filter
midi.device(0).cc(7).to(synth, "volume")         // CC7 zu Lautstaerke
midi.device(0).cc(74).to(synth, "filterRes")     // CC74 zu Resonanz
```

### Pitch Bend Mapping

```csharp
// Pitch Bend zu Parameter mappen
midi.device(0).pitchbend().to(synth, "pitch")
```

### Playable Keys (Tasten-Bereiche)

```csharp
// Tasten-Bereich zu Synth mappen
midi.playablekeys.range(21, 108).map(synth)              // Voller Klavierbereich
midi.playablekeys.range(36, 60).from(0).map(synth)       // Untere Haelfte
midi.playablekeys.range(60, 84).from("Akai").map(synth)  // Obere Haelfte

// Mit Richtungsangabe
midi.playablekeys.range(21, 108).low.to.high.map(synth)  // Normal (Standard)
midi.playablekeys.range(21, 108).high.to.low.map(synth)  // Umgekehrt

// Alternative Syntax
midi.playablekeys.range(21, 108).low_to_high().map(synth)
midi.playablekeys.range(21, 108).high_to_low().map(synth)
```

### MIDI Output

```csharp
// MIDI-Ausgabe nach Index oder Name
midi.output(0).noteOn(60, 100)      // Note an (Note, Velocity)
midi.output(0).noteOff(60)          // Note aus
midi.output(0).cc(1, 64)            // Control Change senden

// Mit Kanal
midi.output(0).noteOn(60, 100, 1)   // Kanal 1 (0-basiert)
midi.output("External Synth").cc(7, 100, 0)  // Nach Name
```

### Beispiel

```csharp
var lead = CreateSynth();
var bass = CreateSynth();

// Tastatur aufteilen: untere Haelfte = Bass, obere = Lead
midi.playablekeys.range(21, 59).from(0).map(bass);
midi.playablekeys.range(60, 108).from(0).map(lead);

// Controller mappen
midi.device(0).cc(1).to(lead, "filterCutoff");
midi.device(0).cc(74).to(lead, "filterRes");
midi.device(0).pitchbend().to(lead, "pitch");

// Einfaches Routing fuer zweites Keyboard
midi.device(1).route(bass);

Start();
```

---

## 7. Audio Controls

Funktionen zur Steuerung der Audio-Ausgabe.

### audio Objekt

Hauptzugriffspunkt fuer Audio-Kontrolle.

### Kanal-Lautstaerke

```csharp
// Einzelner Kanal
audio.channel(0).gain(0.8)    // Kanal 0 auf 80%
audio.channel(1).gain(1.2)    // Kanal 1 auf 120%

// Alle Kanaele
audio.all.gain(0.5)           // Master-Lautstaerke auf 50%
```

### Audio-Eingang (Frequenz-Trigger)

```csharp
// Frequenzbereich ueberwachen
audio.input(0).onFrequency(100, 200)          // 100-200 Hz
    .threshold(0.3)                            // Schwellwert
    .trigger(synth, 36, 100);                  // Trigger Note 36

// Mit benutzerdefinierter Aktion
audio.input(0).onFrequency(80, 120)
    .threshold(0.5)
    .trigger(magnitude => {
        Print($"Kick erkannt! Staerke: {magnitude}");
    });
```

### Beispiel

```csharp
var synth = CreateSynth();
var pattern = CreatePattern(synth);

// Lautstaerke-Balance einstellen
audio.channel(0).gain(0.8);
audio.channel(1).gain(1.0);

// Master auf 70%
audio.all.gain(0.7);

Start();
```

---

## 8. VST Plugin API

Funktionen zum Laden und Steuern von VST-Plugins.

### vst Objekt

Hauptzugriffspunkt fuer VST-Operationen.

### Plugin-Verwaltung

```csharp
// Verfuegbare Plugins auflisten
vst.list()                    // Alle entdeckten Plugins ausgeben
vst.loaded()                  // Alle geladenen Plugins ausgeben
```

### Plugin laden

```csharp
// Nach Name oder Pfad
var plugin = vst.load("Serum")                          // Nach Name
var plugin = vst.load("C:/VST/Serum.dll")               // Nach Pfad

// Nach Index
var plugin = vst.load(0)                                // Erstes Plugin
```

### Plugin abrufen

```csharp
// Bereits geladenes Plugin abrufen
var serum = vst.get("Serum")
var serum = vst.plugin("Serum")    // Alias
```

### Plugin-Steuerung (Fluent API)

```csharp
// MIDI-Routing
plugin.from(0)                     // MIDI von Geraet 0
plugin.from("Akai MPK")            // MIDI nach Geraetename

// Parameter setzen
plugin.param("cutoff", 0.5)        // Nach Parametername
plugin.param(12, 0.7)              // Nach Parameterindex
plugin.volume(0.8)                 // Lautstaerke

// Noten senden
plugin.noteOn(60, 100)             // Note an
plugin.noteOff(60)                 // Note aus
plugin.allNotesOff()               // Alle Noten aus

// MIDI-Nachrichten
plugin.cc(1, 64)                   // Control Change
plugin.cc(74, 100, 1)              // CC auf Kanal 1
plugin.program(5)                  // Program Change
plugin.pitchBend(8192)             // Pitch Bend (Mitte = 8192)
```

### Direkte Funktionen

```csharp
// Alternative direkte Aufrufe
var plugin = LoadVst("Serum")           // Plugin laden
var plugin = LoadVstByIndex(0)          // Nach Index laden
var plugin = GetVst("Serum")            // Geladenes abrufen
RouteToVst(0, plugin)                   // MIDI routen
ListVstPlugins()                        // Plugins auflisten
ListLoadedVstPlugins()                  // Geladene auflisten
```

### Beispiel

```csharp
// Alle Plugins anzeigen
vst.list();

// Plugin laden und konfigurieren
var serum = vst.load("Serum");
if (serum != null)
{
    serum.from(0)                    // MIDI von erstem Keyboard
         .param("cutoff", 0.7)       // Filter einstellen
         .param("resonance", 0.3)
         .volume(0.8);               // Lautstaerke

    // Testton spielen
    serum.noteOn(60, 100);

    // CC-Mapping
    midi.device(0).cc(1).to(serum.Plugin, "cutoff");
}

Start();
```

---

## 9. Sample Fluent API

Die Sample Fluent API bietet eine elegante Syntax fuer Sample-Instrumente.

### samples Objekt

Hauptzugriffspunkt fuer Sample-Operationen.

### Sampler erstellen

```csharp
// Leeren Sampler erstellen
var sampler = samples.create()
var sampler = samples.create("MeinSampler")    // Mit Name
```

### Sample laden

```csharp
// Einzelnes Sample als Instrument
var piano = samples.load("C:/Samples/piano.wav")           // Standard Root = C4 (60)
var piano = samples.load("C:/Samples/piano.wav", 48)       // Root = C3 (48)
```

### Aus Verzeichnis laden

```csharp
// Alle Samples aus einem Ordner
var kit = samples.fromDirectory("C:/Samples/DrumKit/")
```

### Builder-Methoden (verkettbar)

```csharp
// Sample zu Note mappen
.map("kick.wav", 36)              // Sample auf Note 36 (C2)
.map("snare.wav", 38)             // Sample auf Note 38 (D2)

// Verzeichnis setzen
.directory("C:/Samples/")         // Basisverzeichnis fuer relative Pfade

// Lautstaerke
.volume(0.8)                      // 80% Lautstaerke

// Name
.name("DrumKit")                  // Name setzen

// Pattern erstellen
.pattern()                        // Gibt Pattern zurueck
```

### Sampler-Objekt abrufen

```csharp
// Das SampleInstrument-Objekt abrufen
var sampler = samples.create().Sampler

// Implizite Konvertierung
SampleInstrument sampler = samples.create();
```

### Beispiel

```csharp
// Einfaches Drum-Kit erstellen
var drums = samples.create("DrumKit")
    .directory("C:/Samples/Drums/")
    .map("kick.wav", 36)
    .map("snare.wav", 38)
    .map("hihat_closed.wav", 42)
    .map("hihat_open.wav", 46)
    .volume(0.9);

// Pattern fuer Drums
var drumPattern = drums.pattern();
drumPattern.AddNote(0, 36, 100, 0.25);    // Kick auf Beat 1
drumPattern.AddNote(1, 38, 100, 0.25);    // Snare auf Beat 2
drumPattern.AddNote(2, 36, 100, 0.25);    // Kick auf Beat 3
drumPattern.AddNote(3, 38, 100, 0.25);    // Snare auf Beat 4

// HiHat-Pattern
for (int i = 0; i < 8; i++)
{
    drumPattern.AddNote(i * 0.5, 42, 80, 0.1);  // HiHat auf 8tel
}

// Piano aus Sample-Datei
var piano = samples.load("C:/Samples/grand_piano_c4.wav", 60)
    .name("GrandPiano")
    .volume(0.7);

// MIDI zu Piano routen
midi.device(0).route(piano.Sampler);

SetBpm(100);
Start();
```

---

## 10. Hilfsfunktionen

Nuetzliche Helfer fuer Skripte.

### Print(message)

Gibt eine Nachricht auf der Konsole aus.

```csharp
void Print(string message)
```

### Random(min, max)

Generiert eine zufaellige Fliesskommazahl.

```csharp
float Random(float min, float max)
```

### RandomInt(min, max)

Generiert eine zufaellige Ganzzahl.

```csharp
int RandomInt(int min, int max)
```

### Beispiel

```csharp
Print("Skript gestartet!");

// Zufaellige Noten generieren
var synth = CreateSynth();
var pattern = CreatePattern(synth);

for (int i = 0; i < 16; i++)
{
    int note = RandomInt(48, 72);           // Zufaellige Note C3-C5
    int velocity = RandomInt(60, 127);      // Zufaellige Velocity
    float duration = Random(0.1f, 0.5f);    // Zufaellige Dauer

    pattern.AddNote(i * 0.25, note, velocity, duration);
}

Print($"Pattern mit 16 zufaelligen Noten erstellt");
SetBpm(120);
Start();
```

---

## 11. Frequenz-Trigger

Erweiterte Funktionen fuer frequenzbasierte Trigger von Audio-Eingaengen.

### AddFrequencyTrigger

Fuegt einen Frequenz-Trigger hinzu, der auf bestimmte Frequenzbereiche reagiert.

```csharp
void AddFrequencyTrigger(int deviceIndex, float low, float high, float threshold, Action<float> action)
```

**Parameter:**
- `deviceIndex` - Index des Audio-Eingangs
- `low` - Untere Frequenzgrenze in Hz
- `high` - Obere Frequenzgrenze in Hz
- `threshold` - Schwellwert fuer die Ausloesung (0.0 - 1.0)
- `action` - Aktion, die mit der Magnitude aufgerufen wird

### Fluent API Syntax

```csharp
audio.input(0)
    .onFrequency(lowHz, highHz)
    .threshold(value)
    .trigger(synth, note, velocity)    // Oder benutzerdefinierte Aktion
```

### Beispiel: Drum-Trigger

```csharp
var drums = samples.create("DrumTrigger")
    .map("kick_sample.wav", 36)
    .map("snare_sample.wav", 38);

// Kick-Drum auf tiefe Frequenzen triggern
audio.input(0).onFrequency(50, 150)
    .threshold(0.4)
    .trigger(drums.Sampler, 36, 100);

// Snare auf mittlere Frequenzen triggern
audio.input(0).onFrequency(200, 400)
    .threshold(0.3)
    .trigger(drums.Sampler, 38, 80);

// Benutzerdefinierte Aktion fuer hohe Frequenzen
audio.input(0).onFrequency(2000, 5000)
    .threshold(0.2)
    .trigger(magnitude => {
        Print($"Hohe Frequenz erkannt: {magnitude:F2}");
        // Hier koennten weitere Aktionen folgen
    });

Start();
```

---

## MIDI Transport Mapping

Funktionen zum Mappen von MIDI-Controllern auf Transport-Funktionen.

### MapBpm(deviceIndex, cc)

Mappt einen MIDI CC auf BPM-Steuerung (60-200 BPM).

```csharp
void MapBpm(int deviceIndex, int cc)
```

### MapStart(deviceIndex, note)

Mappt eine MIDI-Note zum Starten des Sequenzers.

```csharp
void MapStart(int deviceIndex, int note)
```

### MapStop(deviceIndex, note)

Mappt eine MIDI-Note zum Stoppen des Sequenzers.

```csharp
void MapStop(int deviceIndex, int note)
```

### MapSkip(deviceIndex, cc, beats)

Mappt einen MIDI CC zum Ueberspringen von Beats.

```csharp
void MapSkip(int deviceIndex, int cc, double beats)
```

### MapScratch(deviceIndex, cc, scale)

Mappt einen MIDI CC fuer Scratching-Verhalten.

```csharp
void MapScratch(int deviceIndex, int cc, double scale = 16.0)
```

### Beispiel

```csharp
// Transport-Steuerung ueber MIDI
MapStart(0, 60);        // C4 startet Sequenzer
MapStop(0, 61);         // C#4 stoppt Sequenzer
MapBpm(0, 20);          // CC20 steuert BPM
MapSkip(0, 21, 4);      // CC21 springt 4 Beats

// Scratching mit Jog-Wheel
MapScratch(0, 22, 32);  // CC22 fuer Scratch, 32 Beats Bereich

Start();
```

---

## Direkte MIDI-Routing Funktionen

Niedrigere Ebene fuer direktes MIDI-Routing.

### RouteMidi(deviceIndex, synth)

Routet MIDI-Eingabe direkt zu einem Synthesizer.

```csharp
void RouteMidi(int deviceIndex, ISynth synth)
```

### MapControl(deviceIndex, cc, synth, param)

Mappt einen MIDI CC direkt zu einem Synth-Parameter.

```csharp
void MapControl(int deviceIndex, int cc, ISynth synth, string param)
```

### MapPitchBend(deviceIndex, synth, param)

Mappt Pitch Bend zu einem Synth-Parameter.

```csharp
void MapPitchBend(int deviceIndex, ISynth synth, string param)
```

---

## Vollstaendiges Beispiel

```csharp
// ============================================
// MusicEngine Live-Performance Setup
// ============================================

// === Instrumente erstellen ===
var lead = CreateSynth();
var bass = CreateSynth();
var pad = CreateSynth();

// Drum-Kit aus Samples
var drums = samples.create("DrumKit")
    .directory("C:/Samples/Drums/")
    .map("kick.wav", 36)
    .map("snare.wav", 38)
    .map("hihat.wav", 42)
    .map("crash.wav", 49)
    .volume(0.9);

// VST-Plugin laden
var reverb = vst.load("ValhallaRoom");
if (reverb != null)
{
    reverb.param("decay", 0.6)
          .param("mix", 0.3);
}

// === MIDI-Routing ===
// Keyboard 1: Split - Bass links, Lead rechts
midi.playablekeys.range(21, 59).from(0).map(bass);
midi.playablekeys.range(60, 108).from(0).map(lead);

// Keyboard 2: Pad
midi.device(1).route(pad);

// Drum-Pad: Drums
midi.device("Akai MPD").route(drums.Sampler);

// === CC-Mapping ===
midi.device(0).cc(1).to(lead, "filterCutoff");
midi.device(0).cc(74).to(lead, "filterRes");
midi.device(0).pitchbend().to(lead, "pitch");

// === Transport ===
MapStart(0, 120);   // Pad-Taste startet
MapStop(0, 121);    // Pad-Taste stoppt
MapBpm(0, 20);      // Fader fuer BPM

// === Patterns ===
var drumPattern = drums.pattern();
// 4/4 Beat
drumPattern.AddNote(0, 36, 100, 0.1);     // Kick
drumPattern.AddNote(1, 38, 100, 0.1);     // Snare
drumPattern.AddNote(2, 36, 100, 0.1);     // Kick
drumPattern.AddNote(3, 38, 100, 0.1);     // Snare

// HiHat auf 8tel
for (int i = 0; i < 8; i++)
{
    drumPattern.AddNote(i * 0.5, 42, 70, 0.05);
}

// === Audio-Einstellungen ===
audio.channel(0).gain(0.8);   // Lead
audio.channel(1).gain(0.9);   // Bass
audio.channel(2).gain(0.6);   // Pad
audio.channel(3).gain(1.0);   // Drums
audio.all.gain(0.85);         // Master

// === Start ===
SetBpm(128);
Print("Setup komplett! Druecke Start-Taste zum Spielen.");
```

---

## Hinweise

- Alle Pfade sollten absolute Pfade sein oder relativ zum Arbeitsverzeichnis.
- MIDI-Geraete werden bei Index 0 beginnend nummeriert.
- BPM-Bereich: typischerweise 60-200 BPM.
- Velocity-Bereich: 0-127 (MIDI-Standard).
- Noten-Bereich: 0-127 (MIDI-Standard, C4 = 60).
- Gain/Volume-Werte: 0.0 = stumm, 1.0 = normal, >1.0 = Verstaerkung.

---

## 12. Virtual Audio Channels

Die Virtual Audio Channel API ermoeglicht das Routen von Audio zu anderen Anwendungen ueber Named Pipes. Dies kann als virtuelles Mikrofon oder Audio-Input fuer andere Programme verwendet werden.

### virtualChannels Objekt

Hauptzugriffspunkt fuer Virtual Audio Channel Operationen.

### Kanal erstellen

```csharp
// Neuen virtuellen Kanal erstellen
var channel = virtualChannels.create("MeinKanal")

// Mit Verkettung
var channel = virtualChannels.create("Output")
    .volume(0.8)
    .start();
```

### Kanaele auflisten

```csharp
// Alle virtuellen Kanaele anzeigen
virtualChannels.list()
```

### Builder-Methoden

```csharp
// Lautstaerke setzen
.volume(0.8)              // 80% Lautstaerke

// Kanal starten
.start()                  // Startet den Pipe-Server

// Kanal stoppen
.stop()                   // Stoppt den Pipe-Server

// Pipe-Name abrufen
.pipeName                 // Gibt den Namen der Pipe zurueck
```

### Direkter Zugriff

```csharp
// Kanal-Objekt abrufen
var channel = virtualChannels.create("Audio").Channel

// Implizite Konvertierung
VirtualAudioChannel channel = virtualChannels.create("Audio");
```

### Direkte Funktionen

```csharp
// Alternative direkte Aufrufe
var channel = CreateVirtualChannel("MeinKanal")
ListVirtualChannels()
```

### Verwendung mit anderen Programmen

Der virtuelle Kanal erstellt eine Named Pipe mit dem Namen `MusicEngine_[Kanalname]`.

Andere Programme koennen sich mit dieser Pipe verbinden um Audio zu empfangen:

**Python Beispiel:**
```python
import struct

pipe_path = r'\\.\pipe\MusicEngine_MeinKanal'
with open(pipe_path, 'rb') as pipe:
    # Header lesen (SampleRate, Channels, BitsPerSample)
    header = pipe.read(12)
    sample_rate, channels, bits = struct.unpack('III', header)

    # Audio-Daten lesen
    while True:
        data = pipe.read(4096)
        if not data:
            break
        # Float-Samples verarbeiten...
```

**C# Beispiel:**
```csharp
using var pipe = new NamedPipeClientStream(".", "MusicEngine_MeinKanal", PipeDirection.In);
pipe.Connect();

// Header lesen
var headerBuffer = new byte[12];
pipe.Read(headerBuffer, 0, 12);
int sampleRate = BitConverter.ToInt32(headerBuffer, 0);
int channels = BitConverter.ToInt32(headerBuffer, 4);

// Audio lesen
var buffer = new byte[4096];
while (pipe.Read(buffer, 0, buffer.Length) > 0)
{
    // Float-Samples verarbeiten...
}
```

### Beispiel: Audio zu Discord/OBS routen

```csharp
// Virtuellen Kanal fuer Streaming erstellen
var streamOutput = virtualChannels.create("StreamAudio")
    .volume(1.0)
    .start();

Print($"Stream-Kanal erstellt: {streamOutput.pipeName}");
Print("Verbinde dein Streaming-Tool mit dieser Pipe!");

// Normale Audio-Instrumente
var synth = CreateSynth();
var pattern = CreatePattern(synth);

pattern.AddNote(0, 60, 100, 0.5);
pattern.AddNote(1, 64, 100, 0.5);
pattern.AddNote(2, 67, 100, 0.5);
pattern.AddNote(3, 72, 100, 0.5);

SetBpm(120);
Start();
```

### Beispiel: Mehrere Kanaele fuer Multitrack

```csharp
// Separate Kanaele fuer verschiedene Instrumente
var drumsChannel = virtualChannels.create("Drums").start();
var bassChannel = virtualChannels.create("Bass").start();
var leadsChannel = virtualChannels.create("Leads").start();

virtualChannels.list();  // Alle Kanaele anzeigen

// Andere Programme koennen jetzt einzelne Spuren aufnehmen
```

---

*MusicEngine Scripting API - Version 2026*
*Copyright (c) 2026 MusicEngine Watermann420 and Contributors*
