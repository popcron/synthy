using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Popcron.Synth
{
    public enum Wave
    {
        Saw,
        Sine,
        Square,
        Triangle
    }

    [RequireComponent(typeof(AudioSource))]
    public class Synth : MonoBehaviour
    {
        public int noteOffset = 0;
        public byte voices = 16;
        public double volume = 1.0;
        public GeneratorPreset preset;

        private List<KeyCode> keysPressed = new List<KeyCode>();
        private List<Generator> generators = new List<Generator>();
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            Reset();
        }

        private void OnEnable()
        {
            Reset();
        }

        private void OnApplicationFocus(bool focus)
        {
            Reset(false);
        }

        private void Reset(bool hardReset = true)
        {
            if (hardReset)
            {
                for (int i = 0; i < generators.Count; i++)
                {
                    if (!generators[i]) continue;

                    Destroy(generators[i].gameObject);
                }
                generators.Clear();
            }

            for (int i = 0; i < generators.Count; i++)
            {
                generators[i].Reset();
            }

            keysPressed.Clear();
        }

        private void OnGUI()
        {
            Event e = Event.current;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.None) return;
                if (keysPressed.Contains(e.keyCode)) return;
                keysPressed.Add(e.keyCode);
            }
            else if (e.type == EventType.KeyUp)
            {
                if (!keysPressed.Contains(e.keyCode)) return;
                keysPressed.Remove(e.keyCode);
            }
        }

        private void LateUpdate()
        {
            if (voices != generators.Count)
            {
                generators.Clear();
                for (int i = 0; i < voices; i++)
                {
                    generators.Add(NewGenerator());
                }
            }

            if (audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }

        private Generator NewGenerator()
        {
            GameObject generatorGameObject = new GameObject("Generator");
            Generator generator = generatorGameObject.AddComponent<Generator>();
            generator.Reset();
            return generator;
        }

        private void OnAudioFilterRead(float[] data, int channels)
        {
            double volume = 1.0 / generators.Count * this.volume;
            for (int i = 0; i < generators.Count; i++)
            {
                generators[i].Frequency = keysPressed.Count > i ? Helper.GetFrequencyFromKey(keysPressed[i]) : 0;
                generators[i].Preset = preset;
                generators[i].Volume = volume;
                generators[i].Active = keysPressed.Count > i;
            }
        }
    }
}