using System;
using System.Collections.Generic;

namespace Synthy
{
    [Serializable]
    public class Track
    {
        public uint saves = 0;
        public string name = "Track";
        public ushort bpm = 130;
        public List<string> instruments = new List<string>();
        public List<InstancedPattern> patterns = new List<InstancedPattern>();
        public List<Pattern> uniquePatterns = new List<Pattern>();

        public Track()
        {
            instruments.Add("Saw");
            instruments.Add("Sine");
            instruments.Add("Pulse");

            Pattern pattern = new Pattern("Pattern 1", this);

            uniquePatterns.Add(pattern);
        }

        public Track(Track original)
        {
            saves = original.saves;
            name = original.name;
            bpm = original.bpm;

            foreach (var instrument in original.instruments)
            {
                instruments.Add(instrument);
            }

            foreach (var pattern in original.patterns)
            {
                patterns.Add(new InstancedPattern(pattern));
            }

            foreach (var pattern in original.uniquePatterns)
            {
                uniquePatterns.Add(new Pattern(pattern));
            }
        }
    }
}