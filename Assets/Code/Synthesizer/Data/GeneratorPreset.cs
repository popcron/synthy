using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Popcron.Synth
{
    [Serializable]
    public class ADSR
    {
        [Serializable]
        public struct ADSRPoint
        {
            public double time;
            public double volume;

            public ADSRPoint(double x, double y) : this()
            {
                this.time = x;
                this.volume = y;
            }
        }

        public ADSRPoint attack = new ADSRPoint(0.5, 0.5);
        public ADSRPoint decay = new ADSRPoint(0.5, 0.5);
        public double release = 0.5;
    }

    [CreateAssetMenu(menuName = "Synth/Generator preset")]
    public class GeneratorPreset : ScriptableObject
    {
        public Wave wave = Wave.Saw;

        public double vibratoSpeed = 1;
        public double vibratoDepth = 10;
        public double legato = 100;

        public ADSR adsr = new ADSR();
    }
}
