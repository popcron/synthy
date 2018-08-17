using UnityEngine;

public class NoteFrequencies
{
    public static double GetFrequencyFromNote(int note, double tuning)
    {
        return Mathf.Pow(2, (note - 69) / 12f) * tuning;
    }

    public static KeyCode[] Keys = new KeyCode[]
    {
        KeyCode.Z,              //0
        KeyCode.S,              //1
        KeyCode.X,              //2
        KeyCode.D,              //3
        KeyCode.C,              //4
        KeyCode.V,              //5
        KeyCode.G,              //6
        KeyCode.B,              //7
        KeyCode.H,              //8
        KeyCode.N,              //9
        KeyCode.J,              //10
        KeyCode.M,              //11
        KeyCode.Comma,          //12
        KeyCode.L,              //13
        KeyCode.Period,         //14
        KeyCode.Semicolon,      //15
        KeyCode.Slash,          //16
        KeyCode.Q,              //17
        KeyCode.Alpha2,         //18
        KeyCode.W,              //19
        KeyCode.Alpha3,         //20
        KeyCode.E,              //21
        KeyCode.R,              //22
        KeyCode.Alpha5,         //23
        KeyCode.T,              //24
        KeyCode.Alpha6,         //25
        KeyCode.Y,              //26
        KeyCode.Alpha7,         //27
        KeyCode.U,              //28
        KeyCode.I,              //29
        KeyCode.Alpha9,         //30
        KeyCode.O,              //31
        KeyCode.Alpha0,         //32
        KeyCode.P,              //33
        KeyCode.LeftBracket,    //34
        KeyCode.Equals,         //35
        KeyCode.RightBracket    //36
    };

    public static int GetNoteFromKeyCode(KeyCode keyCode)
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

        return -1;
    }
}