using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Popcron.Synth
{
    public class Generator : MonoBehaviour
    {
        [SerializeField]
        private GeneratorPreset preset;

        [SerializeField]
        private double frequency;

        [SerializeField]
        private bool active;

        [SerializeField]
        private double volume;

        private double phase;
        private double lastPhase;
        private double dspTime;
        private double sampleRate;
        private double wave;
        private Material lineMaterial;
        private AudioSource audioSource;

        private List<double> history = new List<double>();

        public GeneratorPreset Preset
        {
            get
            {
                return preset;
            }
            set
            {
                preset = value;
            }
        }

        public double Frequency
        {
            get
            {
                double vibrato = Math.Sin(dspTime * preset.vibratoSpeed) * preset.vibratoDepth;
                return frequency + vibrato;
            }
            set
            {
                frequency = value;
            }
        }

        public bool Active
        {
            get
            {
                return active;
            }
            set
            {
                active = value;
            }
        }

        public double Volume
        {
            get
            {
                return volume;
            }
            set
            {
                volume = value;
            }
        }

        public void Reset()
        {
            audioSource = GetComponent<AudioSource>();
            if (!audioSource)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.spatialBlend = 0f;
            audioSource.playOnAwake = false;
            audioSource.Stop();
            lineMaterial = null;
            active = false;
        }

        private void OnRenderObject()
        {
            if (!lineMaterial)
            {
                lineMaterial = new Material(Shader.Find("Sprites/Default"));
            }

            GL.PushMatrix();

            lineMaterial.SetPass(0);
            GL.LoadPixelMatrix();
            GL.Begin(GL.LINES);

            float speed = 2f;

            //draw pitch
            GL.Color(Color.red);
            for (int i = 1; i < history.Count - 1; i++)
            {
                int index = (history.Count - 1) - i;
                float x = i * speed;
                if (x > Screen.width) continue;

                float y = (float)history[index];
                GL.Vertex3(x, (y * 0.2f) + Screen.height * 0.5f, 0);
            }

            GL.End();
            GL.PopMatrix();
        }

        private void Update()
        {
            if (preset == null)
            {
                audioSource.Stop();
                return;
            }

            if (Active)
            {
                if (!audioSource.isPlaying)
                {
                    phase = 0f;
                    audioSource.Play();
                }
            }
            else
            {
                //queue for stop, only stop when reached 0 phase
                if (phase > 0 && lastPhase < phase)
                {
                    phase = 0;
                    audioSource.Stop();
                    wave = 0;
                }
                else if (phase < 0 && lastPhase > phase)
                {
                    phase = 0;
                    audioSource.Stop();
                    wave = 0;
                }
            }

            sampleRate = AudioSettings.outputSampleRate;
            lastPhase = phase;
            dspTime = AudioSettings.dspTime;

            //set history
            history.Add(Frequency - 400);
            if (history.Count > 300)
            {
                history.RemoveAt(0);
            }
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            double increment = Frequency * 2 * Math.PI / sampleRate;
            for (int i = 0; i < data.Length; i += channels)
            {
                //generate wave
                wave = 0;
                if (preset.wave == Wave.Saw)
                {
                    wave = phase / (2.0 * Math.PI);
                }
                else if (preset.wave == Wave.Sine)
                {
                    wave = Math.Sin(phase);
                }
                else if (preset.wave == Wave.Square)
                {
                    wave = phase > Math.PI ? 1 : 0;
                }
                else if (preset.wave == Wave.Triangle)
                {
                    wave = phase;
                    if (phase > Math.PI)
                    {
                        wave = (Math.PI * 2.0) - phase;
                    }
                }
                wave *= volume;

                //assign data
                data[i] = (float)wave;
                if (channels == 2) data[i + 1] = data[i];

                phase += increment;
                if (phase > 2.0 * Math.PI)
                {
                    phase = 0;
                }
            }
        }
    }
}