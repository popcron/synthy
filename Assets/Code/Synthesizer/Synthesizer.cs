using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Synthy
{
    [RequireComponent(typeof(AudioSource))]
    public class Synthesizer : MonoBehaviour
    {
        public const int SampleFrequency = 44100;
        public const int MiddleC = 72;

        public bool allowKeyboard = false;
        public Preset preset;
        public int polyphony = 16;
        public float tuning = 440;

        [Range(0f, 1f)]
        public float volume = 0.1f;

        private List<int> notes = new List<int>();
        private List<VoiceKey> voices = new List<VoiceKey>();
        private List<Note> requests = new List<Note>();

        private Preset lastPreset;
        private int lastGenerators;
        
        public void Initialize(Preset preset)
        {
            this.preset = preset;
            CreateVoices();

            lastGenerators = preset.Generators.Count;
            lastPreset = preset;
        }

        private void Awake()
        {
            CreateVoices();
        }

        private void OnEnable()
        {
            CreateVoices();
        }

        private void CreateVoices()
        {
            //delete the old voices
            foreach (Transform children in transform)
            {
                Destroy(children.gameObject);
            }

            //create new voices
            voices.Clear();
            for (int i = 0; i < polyphony; i++)
            {
                VoiceKey voice = new GameObject("VoiceKey").AddComponent<VoiceKey>();
                voice.Initialize(this);
                voice.transform.SetParent(transform);
                voices.Add(voice);
            }
        }

        private void OnApplicationFocus(bool focus)
        {
            //focus lost, release all keys
            if (!focus)
            {
                notes.Clear();
            }
        }

        public bool IsPlaying(Note note)
        {
            return requests.Contains(note);
        }

        public void Add(Note note)
        {
            requests.Add(note);
        }

        public void Remove(Note note)
        {
            requests.Remove(note);
        }

        public async void Play(int key, float duration, CancellationToken? token = null)
        {
            Note note = new Note((byte)key, 0, (int)duration);
            requests.Add(note);

            if (token != null)
            {
                await Task.Delay(Mathf.RoundToInt(duration * 1000), token.Value);
            }
            else
            {
                await Task.Delay(Mathf.RoundToInt(duration * 1000));
            }

            requests.Remove(note);
        }

        private void Update()
        {
            if (lastGenerators != preset.Generators.Count)
            {
                lastGenerators = preset.Generators.Count;
                CreateVoices();
            }

            if (lastPreset != preset)
            {
                lastPreset = preset;
                CreateVoices();
            }

            if (allowKeyboard)
            {
                foreach (var key in Helper.Keys)
                {
                    int note = Helper.GetNoteFromKeyCode(key);
                    if (Input.GetKeyDown(key))
                    {
                        if (!notes.Contains(note))
                        {
                            notes.Add(note);
                        }
                    }
                    if (Input.GetKeyUp(key))
                    {
                        while (notes.Contains(note))
                        {
                            notes.Remove(note);
                        }
                    }
                }
            }

            //disable all voices
            foreach (var voice in voices)
            {
                voice.enabled = false; notes.Contains(voice.note);
            }

            //for every note, find the first available voice
            List<int> n = new List<int>(notes);
            for (int i = 0; i < requests.Count; i++)
            {
                n.Add(requests[i].note);
            }

            foreach (var note in n)
            {
                bool found = false;
                foreach (var voice in voices)
                {
                    if (voice.Available || (!voice.Available && voice.note == note))
                    {
                        voice.enabled = true;
                        voice.note = note;

                        found = true;
                        break;
                    }
                }

                if (found) continue;
            }
        }
    }
}