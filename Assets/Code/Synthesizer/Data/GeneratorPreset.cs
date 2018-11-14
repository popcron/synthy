using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Popcron.Synth
{
    [CreateAssetMenu(menuName = "Synth/Generator preset")]
    public class GeneratorPreset : ScriptableObject
    {
        public Wave wave = Wave.Saw;

        public double vibratoSpeed = 1;
        public double vibratoDepth = 10;
    }
}
