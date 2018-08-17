using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class VoiceKey : MonoBehaviour
{
    public int note = 0;
    public Synthesizer synthesizer;

    private List<VoiceOscillator> voices = new List<VoiceOscillator>();

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

        foreach (var oscillator in synthesizer.preset.oscillators)
        {
            VoiceOscillator voice = new GameObject("Voice").AddComponent<VoiceOscillator>();
            voice.Initialize(this, oscillator);
            voice.transform.SetParent(transform);
            voices.Add(voice);
        }
    }
}