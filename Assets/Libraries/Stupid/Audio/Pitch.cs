using System;

namespace Stupid.Audio {
    public static class Pitch {
        public const float MIN_AUDIBLE_FREQUENCY = 20f;     // Purves D, Augustine GJ, Fitzpatrick D, et al., editors. Sunderland(MA) : Sinauer Associates; 2001.
        public const float MAX_AUDIBLE_FREQUENCY = 20000f;  // Neuroscience. 2nd edition.

        public const int MIN_NOTE = 0;
        public const int MAX_NOTE = 87;
        public const float TUNING_FREQUENCY = 440f;
        public const int BASE_OCTAVE = 4;
        public const int NOTES_PER_OCTAVE = 12;

        public static int BaseNote => BASE_OCTAVE * NOTES_PER_OCTAVE;
        public static float BaseNoteHz => NoteHz(BaseNote);
        public static float MinNoteFrequency => NoteHz(MIN_NOTE);
        public static float MaxNoteFrequency => NoteHz(MAX_NOTE);
        public static readonly string[] NoteNames = new string[] {
            "A",
            "A#",
            "B",
            "C",
            "C#",
            "D",
            "D#",
            "E",
            "F",
            "F#",
            "G",
            "G#"
        };

        public static float NoteHz(int note, float detune = 0f) {
            return TUNING_FREQUENCY * Moths.Pow(2f, (note - BaseNote + detune) / NOTES_PER_OCTAVE);
        }

        public static float SemitoneToPlaybackSpeed(float semitone) {
            if (semitone == 0f) { return 1f; }

            return 1f * Moths.Pow(2f, semitone / NOTES_PER_OCTAVE);
        }

        public static int ClosestChromaticNote(float frequencyHz) {
            return Moths.FloorToInt(NOTES_PER_OCTAVE * Moths.Log(frequencyHz / TUNING_FREQUENCY, 2f) + BaseNote);
        }

        public static float ClosestChromaticFrequency(float frequencyHz) => NoteHz(ClosestChromaticNote(frequencyHz));

        public static int NoteToOctave(int note) {
            return Moths.FloorToInt(note / (float)NOTES_PER_OCTAVE);
        }

        public static string NoteName(int note) {
            int octave = NoteToOctave(note);
            return $"{NoteNames[note - octave * NOTES_PER_OCTAVE]}{octave}";
        }
    }
}