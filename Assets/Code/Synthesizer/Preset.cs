using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synthy
{
    [CreateAssetMenu]
    public class Preset : ScriptableObject
    {
        [Serializable]
        public class Generator
        {
            public sbyte noteOffset = 0;
            public double detune = 0.0;

            [Range(0.05f, 10f)]
            public double attack = 100f;

            [Range(0.05f, 10f)]
            public double release = 100f;

            [ClampedCurve]
            public AnimationCurve wave = new AnimationCurve();
        }

        [SerializeField]
        private new string name = "";

        [SerializeField]
        private sbyte transpose = 0;

        [SerializeField]
        private List<Generator> generators = new List<Generator>();

        public double Mix
        {
            get
            {
                return 1.0 / generators.Count;
            }
        }

        public string Name => name;
        public List<Generator> Generators => generators;
        public sbyte Transpose => transpose;
    }
}