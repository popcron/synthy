using System;
using System.Collections.Generic;

namespace Synthy
{
    [Serializable]
    public class Rack
    {
        public List<Note> notes = new List<Note>();

        public Rack()
        {
        }

        public Rack(Rack original)
        {
            foreach (var note in original.notes)
            {
                notes.Add(note);
            }
        }
    }
}