using System;
using System.Collections.Generic;
using System.Linq;

namespace Synthy
{
    [Serializable]
    public class Pattern
    {
        public string name;
        public List<Rack> notes = new List<Rack>();

        public int Duration => End - Start;

        public List<Note> Notes
        {
            get
            {
                if (this.notes.Count == 0) return new List<Note>();

                List<Note> notes = new List<Note>();
                foreach (var n in this.notes)
                {
                    notes.AddRange(n.notes);
                }

                return notes;
            }
        }

        public int End
        {
            get
            {
                var notes = Notes;
                if (notes.Count == 0) return 80;

                int max = int.MinValue;
                int index = -1;
                for (int i = 0; i < notes.Count; i++)
                {
                    if (notes[i].end > max)
                    {
                        max = notes[i].end;
                        index = i;
                    }
                }

                return notes[index].end;
            }
        }

        public int Start
        {
            get
            {
                var notes = Notes;
                if (notes.Count == 0) return 0;

                int min = int.MaxValue;
                int index = -1;
                for (int i = 0; i < notes.Count; i++)
                {
                    if (notes[i].end < min)
                    {
                        min = notes[i].end;
                        index = i;
                    }
                }

                return notes[index].start;
            }
        }

        public byte Highest
        {
            get
            {
                var notes = Notes;
                if (notes.Count == 0) return Synthesizer.MiddleC;

                byte min = byte.MinValue;
                int index = -1;
                for (int i = 0; i < notes.Count; i++)
                {
                    if (notes[i].note > min)
                    {
                        min = notes[i].note;
                        index = i;
                    }
                }

                return notes[index].note;
            }
        }

        public byte Lowest
        {
            get
            {
                var notes = Notes;
                if (notes.Count == 0) return Synthesizer.MiddleC;

                byte max = byte.MaxValue;
                int index = -1;
                for (int i = 0; i < notes.Count; i++)
                {
                    if (notes[i].note < max)
                    {
                        max = notes[i].note;
                        index = i;
                    }
                }

                return notes[index].note;
            }
        }
        
        public Pattern(string name, Track track)
        {
            this.name = name;
            foreach (var instrument in track.instruments)
            {
                notes.Add(new Rack());
            }
        }

        public Pattern(Pattern original)
        {
            name = original.name;

            foreach (var rack in original.notes)
            {
                notes.Add(new Rack(rack));
            }
        }
    }
}