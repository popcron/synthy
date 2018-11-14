using System;
using UnityEngine;

namespace Popcron.Synth
{
    public class Helper
    {
        public static double GetFrequencyFromKey(KeyCode keyCode)
        {
            int? note = GetNoteFromKeyCode(keyCode);
            return GetFrequencyFromNote(note);
        }

        public static double GetFrequencyFromNote(int? note)
        {
            if (note != null)
            {
                return GetFrequencyFromNote(note.Value);
            }

            return 0;
        }

        public static double GetFrequencyFromNote(int note)
        {
            return Mathf.Pow(2, (note - 69) / 12f) * 440f;
        }

        public static int? GetNoteFromKeyCode(KeyCode keyCode)
        {
            //bottom row
            if (keyCode == KeyCode.Z) return 60;
            if (keyCode == KeyCode.S) return 61;
            if (keyCode == KeyCode.X) return 62;
            if (keyCode == KeyCode.D) return 63;
            if (keyCode == KeyCode.C) return 64;
            if (keyCode == KeyCode.V) return 65;
            if (keyCode == KeyCode.G) return 66;
            if (keyCode == KeyCode.B) return 67;
            if (keyCode == KeyCode.H) return 68;
            if (keyCode == KeyCode.N) return 69;
            if (keyCode == KeyCode.J) return 70;
            if (keyCode == KeyCode.M) return 71;
            if (keyCode == KeyCode.Comma) return 72;
            if (keyCode == KeyCode.L) return 73;
            if (keyCode == KeyCode.Period) return 74;
            if (keyCode == KeyCode.Semicolon) return 75;
            if (keyCode == KeyCode.Slash) return 76;

            //top row
            if (keyCode == KeyCode.Q) return 72;
            if (keyCode == KeyCode.Alpha2) return 73;
            if (keyCode == KeyCode.W) return 74;
            if (keyCode == KeyCode.Alpha3) return 75;
            if (keyCode == KeyCode.E) return 76;
            if (keyCode == KeyCode.R) return 77;
            if (keyCode == KeyCode.Alpha5) return 78;
            if (keyCode == KeyCode.T) return 79;
            if (keyCode == KeyCode.Alpha6) return 80;
            if (keyCode == KeyCode.Y) return 81;
            if (keyCode == KeyCode.Alpha7) return 82;
            if (keyCode == KeyCode.U) return 83;
            if (keyCode == KeyCode.I) return 84;
            if (keyCode == KeyCode.Alpha9) return 85;
            if (keyCode == KeyCode.O) return 86;
            if (keyCode == KeyCode.Alpha0) return 87;
            if (keyCode == KeyCode.P) return 88;
            if (keyCode == KeyCode.LeftBracket) return 89;
            if (keyCode == KeyCode.Equals) return 90;
            if (keyCode == KeyCode.RightBracket) return 91;

            return null;
        }

        public static string GetLetterFromNote(int note)
        {
            int normalized = note - (Mathf.FloorToInt(note / 12f) * 12);
            if (normalized == 0) return "C";
            if (normalized == 1) return "C#";
            if (normalized == 2) return "D";
            if (normalized == 3) return "D#";
            if (normalized == 4) return "E";
            if (normalized == 5) return "F";
            if (normalized == 6) return "F#";
            if (normalized == 7) return "G";
            if (normalized == 8) return "G#";
            if (normalized == 9) return "A";
            if (normalized == 10) return "A#";
            if (normalized == 11) return "B";

            return null;
        }
    }
}