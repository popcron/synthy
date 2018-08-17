using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Preset;

[RequireComponent(typeof(AudioSource))]
public class VoiceOscillator : MonoBehaviour
{
    private Oscillator oscillator;
    private VoiceKey key;

    private bool active;
    private bool previousActive;

    private double phase;
    private double gain;
    private double attack;
    private double release;
    private double start;

    public void Initialize(VoiceKey key, Oscillator oscillator)
    {
        this.key = key;
        this.oscillator = oscillator;
    }

    public bool Available
    {
        get
        {
            return gain == 0.0 && release == 1.0;
        }
    }

    private void Update()
    {
        active = key.enabled;
    }

    private double Lerp(double a, double b, double t)
    {
        return b * t + a * (1.0 - t);
    }

    private void Attack()
    {
        if (active && !previousActive)
        {
            previousActive = true;
            start = gain;
            phase = 0;
            release = 0;
        }

        if (active)
        {
            attack += 0.1 * oscillator.attack;
            attack = attack > 1.0 ? 1.0 : attack;
            gain = Lerp(start, 1, attack);
        }
    }

    private void Release()
    {
        if (!active && previousActive)
        {
            previousActive = false;
            start = gain;
            attack = 0;
        }

        if (!active)
        {
            release += 0.1 * oscillator.release;
            release = release > 1.0 ? 1.0 : release;
            gain = Lerp(start, 0, release);
        }
    }

    private void Clamp()
    {
        gain = gain < 0.0 ? 0.0 : gain;
        gain = gain > 1.0 ? 1.0 : gain;
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (oscillator == null) return;

        double frequency = NoteFrequencies.GetFrequencyFromNote(key.note + key.synthesizer.preset.transpose + oscillator.noteOffset, key.synthesizer.tuning + oscillator.detune);
        double increment = frequency * 1.0 / Synthesizer.SampleFrequency;

        Attack();
        Release();
        Clamp();

        for (int i = 0; i < data.Length; i += channels)
        {
            data[i] = (float)(gain * key.synthesizer.volume * oscillator.mix * oscillator.wave.Evaluate((float)phase));

            if (channels == 2)
            {
                data[i + 1] = data[i];
            }

            phase += increment;
            if (phase > 1f)
            {
                phase = 0f;
            }
        }
    }
}