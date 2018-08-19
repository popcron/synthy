using System;

namespace Synthy
{
    [Serializable]
    public class Note
    {
        public const int MinDuration = 4;

        public byte note;
        public int start;
        public int end;

        public Note(byte note, int start, int end)
        {
            this.note = note;
            this.start = start;
            this.end = end;
        }

        public int Duration
        {
            get
            {
                return end - start;
            }
        }
    }
}