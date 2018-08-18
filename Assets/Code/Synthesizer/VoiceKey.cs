using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Synthy
{
    [RequireComponent(typeof(AudioSource))]
    public class VoiceKey : MonoBehaviour
    {
        public int note = 0;
        public Synthesizer synthesizer;

        private List<VoiceGenerators> voices = new List<VoiceGenerators>();

        public bool Available
        {
            get
            {
                foreach (var voice in voices)
                {
                    if (!voice.Available) return false;
                }

                return true;
            }
        }

        public void Initialize(Synthesizer synthesizer)
        {
            this.synthesizer = synthesizer;

            foreach (var generator in synthesizer.preset.Generators)
            {
                VoiceGenerators voice = new GameObject("Voice").AddComponent<VoiceGenerators>();
                voice.Initialize(this, generator);
                voice.transform.SetParent(transform);
                voices.Add(voice);
            }
        }
    }
}