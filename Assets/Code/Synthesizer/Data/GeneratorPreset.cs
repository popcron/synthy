using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Popcron.Synth
{
    [CreateAssetMenu(menuName = "Synth/Generator preset")]
    public class GeneratorPreset : ScriptableObject
    {
        public int noteOffset = 0;

        [ClampedCurve]
        public AnimationCurve wave = new AnimationCurve();

        [ClampedCurve]
        public AnimationCurve vibrato = new AnimationCurve();

        public double vibratoSpeed = 1;
        public double vibratoDepth = 10;
    }
}
