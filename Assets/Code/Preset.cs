﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Preset : ScriptableObject
{
    [Serializable]
    public class Oscillator
    {
        public sbyte noteOffset = 0;
        public double detune = 0.0;

        [Range(0f, 1f)]
        public double mix = 1f;

        [Range(0.05f, 10f)]
        public double attack = 100f;

        [Range(0.05f, 10f)]
        public double release = 100f;

        public AnimationCurve wave = new AnimationCurve();
    }

    public sbyte transpose = 0;
    public List<Oscillator> oscillators = new List<Oscillator>();
}