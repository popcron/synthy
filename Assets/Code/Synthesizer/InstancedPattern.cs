using System;
using UnityEngine;

namespace Synthy
{
    [Serializable]
    public class InstancedPattern
    {
        public string name;
        public int time;
        public byte order;
        public int pattern;

        public InstancedPattern(string name, int pattern)
        {
            this.name = name;
            this.pattern = pattern;
        }

        public InstancedPattern(InstancedPattern original)
        {
            name = original.name;
            time = original.time;
            order = original.order;
            pattern = original.pattern;
        }

        public InstancedPattern(Pattern pattern, Track track)
        {
            name = pattern.name;
            this.pattern = track.uniquePatterns.IndexOf(pattern);
        }

        public int GetEnd(Track track)
        {
            return track.uniquePatterns[pattern].End + time;
        }

        public int GetStart(Track track)
        {
            return track.uniquePatterns[pattern].Start + time;
        }

        public int GetLength(Track track)
        {
            return GetEnd(track) - time;
        }
    }
}