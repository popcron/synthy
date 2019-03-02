using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Popcron.Synth
{
    public class Generator : MonoBehaviour
    {
        public enum ADSRState
        {
            Attacking,
            Decaying,
            Sustaining,
            Releasing
        }

        [SerializeField]
        private GeneratorPreset preset;

        [SerializeField]
        private double frequency;

        [SerializeField]
        private bool active;

        [SerializeField]
        private double volume;

        [SerializeField]
        private double phase;

        private bool stop;
        private double lastPhase;
        private double dspTime;
        private double sampleRate;
        private double wave;
        private AudioSource audioSource;
        private Synth synth;
        private bool queuedForStopping;
        private double adsrVolume;

        private double startVolume;
        private ADSRState state;
        private float activeTime;
        private float releasedTime;
        private double desiredFrequency;

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
                return desiredFrequency;
            }
            set
            {
                desiredFrequency = value;
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
                if (!active && value)
                {
                    queuedForStopping = false;
                    if (stop)
                    {
                        phase = 0f;
                        stop = false;
                        frequency = desiredFrequency;
                    }

                    state = ADSRState.Attacking;
                    startVolume = adsrVolume;
                }

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
            active = false;
        }

        public void Initialize(Synth synth)
        {
            Reset();
            this.synth = synth;
        }

        private double Lerp(double a, double b, double t)
        {
            return a * (1.0 - t) + b * t;
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
                frequency = desiredFrequency;
                releasedTime = 0f;
                activeTime += Time.deltaTime;

                if (state == ADSRState.Attacking)
                {
                    double t = activeTime / preset.adsr.attack.time;
                    if (t >= 1)
                    {
                        adsrVolume = preset.adsr.attack.volume;
                        state = ADSRState.Decaying;
                        startVolume = adsrVolume;
                        return;
                    }

                    adsrVolume = Lerp(startVolume, preset.adsr.attack.volume, t);
                }
                else if (state == ADSRState.Decaying)
                {
                    double t = (activeTime - preset.adsr.attack.time) / preset.adsr.decay.time;
                    if (t >= 1)
                    {
                        adsrVolume = preset.adsr.decay.volume;
                        state = ADSRState.Sustaining;
                        return;
                    }

                    adsrVolume = Lerp(startVolume, preset.adsr.decay.volume, t);
                }
                else if (state == ADSRState.Sustaining)
                {
                    adsrVolume = preset.adsr.decay.volume;
                }

                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                    phase = 0;
                }
            }
            else
            {
                if (state != ADSRState.Releasing)
                {
                    state = ADSRState.Releasing;
                    startVolume = adsrVolume;
                }
                if (state == ADSRState.Releasing)
                {
                    double t = releasedTime / preset.adsr.release;
                    if (t > 1)
                    {
                        t = 1;
                        if (audioSource.isPlaying)
                        {
                            queuedForStopping = true;
                        }
                    }
                    adsrVolume = Lerp(startVolume, 0, t);
                }

                activeTime = 0f;
                releasedTime += Time.deltaTime;

                if (audioSource.isPlaying && stop)
                {
                    audioSource.Stop();
                }
            }

            sampleRate = AudioSettings.outputSampleRate;
            lastPhase = phase;
            dspTime = AudioSettings.dspTime;
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            if (stop) return;

            double vibrato = Math.Sin(dspTime * preset.vibratoSpeed) * preset.vibratoDepth;
            double f = frequency + vibrato;
            double increment = f * 2.0 * Math.PI / sampleRate;
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
                wave *= volume * adsrVolume;

                //assign data
                data[i] = (float)wave;
                if (channels == 2) data[i + 1] = data[i];

                phase += increment;
                if (phase >= 2.0 * Math.PI)
                {
                    phase = 0;
                    if (!active && queuedForStopping)
                    {
                        stop = true;
                    }
                }
            }
        }
    }
}