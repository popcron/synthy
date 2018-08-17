using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Synthesizer : MonoBehaviour
{
    public const int SampleFrequency = 44000;
    public const int MiddleC = 72;

    private static Synthesizer instance;

    public Preset preset;
    public int polyphony = 16;
    public float tuning = 440;

    [Range(0f, 1f)]
    public float volume = 0.1f;

    private List<int> notes = new List<int>();
    private List<VoiceKey> voices = new List<VoiceKey>();
    private List<int> requests = new List<int>();

    private Preset lastPreset;
    private int lastOscillators;

    private void Awake()
    {
        instance = this;
        CreateVoices();
    }

    private void OnEnable()
    {
        instance = this;
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

    public static async Task Play(int key, float duration)
    {
        if (!instance) instance = FindObjectOfType<Synthesizer>();
        if (!instance) return;

        instance.requests.Add(key);

        await Task.Delay(Mathf.RoundToInt(duration * 1000));

        instance.requests.Remove(key);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            Play(MiddleC, 0.5f);
        }

        if(lastOscillators != preset.oscillators.Count)
        {
            lastOscillators = preset.oscillators.Count;
            CreateVoices();
        }

        if(lastPreset != preset)
        {
            lastPreset = preset;
            CreateVoices();
        }

        foreach (var key in NoteFrequencies.Keys)
        {
            int note = NoteFrequencies.GetNoteFromKeyCode(key);
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

        //disable all voices
        foreach (var voice in voices)
        {
            voice.enabled = false;  notes.Contains(voice.note);
        }

        //for every note, find the first available voice
        List<int> n = new List<int>(notes);
        n.AddRange(requests);

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