using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Synthy
{
    [RequireComponent(typeof(AudioSource))]
    public class Player : MonoBehaviour
    {
        [Serializable]
        public class Note
        {
            public string instrument;
            public Synthy.Note note;
            public int start;
            public int end;

            public Note(string instrument, Synthy.Note note, InstancedPattern instancedPattern)
            {
                this.instrument = instrument;
                this.note = note;

                start = note.start + instancedPattern.time;
                end = note.end + instancedPattern.time;
            }
        }

        public double time = 0.0;

        [Range(0.2f, 3f)]
        public double speed = 1.0;

        public bool play = false;

        public Synthesizer synthesizer;
        public TrackAsset track;
        public List<Preset> instruments = new List<Preset>();

        private List<Synthesizer> synthesizers = new List<Synthesizer>();
        private List<Note> notesPlaying = new List<Note>();
        private Track data;

        private double lastFrame;

        private void Awake()
        {
            lastFrame = AudioSettings.dspTime;

            if (track == null)
            {
                throw new Exception("Nothing to play from Awake.");
            }

            Play(track.track);
        }

        private void OnEnable()
        {
            lastFrame = AudioSettings.dspTime;
        }

        public void Play(Track data)
        {
            lastFrame = AudioSettings.dspTime;
            this.data = data ?? throw new Exception("Track data is null.");

            play = false;

            //create enough synthesizers
            foreach (var instrument in data.instruments)
            {
                Create(instrument);
            }
        }

        private Preset GetInstrument(string name)
        {
            for (int i = 0; i < instruments.Count; i++)
            {
                if (instruments[i].name == name) return instruments[i];
            }

            throw new Exception("No Preset with the name of " + name + " was found.");
        }

        private void Create(string instrument)
        {
            Synthesizer synthesizer = Instantiate(this.synthesizer);
            synthesizer.Initialize(GetInstrument(instrument));

            synthesizers.Add(synthesizer);
        }

        private void OnAudioFilterRead(float[] d, int c)
        {
            double deltaTime = AudioSettings.dspTime - lastFrame;
            lastFrame = AudioSettings.dspTime;

            if (play)
            {
                time += data.bpm * deltaTime * speed / 10.0;
            }
            else
            {
                notesPlaying.Clear();
                goto Play;
            }

            //find the closest note to the time marker
            //that isnt being currently played
            //and add it to the list of currently playing notes
            for (int i = 0; i < data.patterns.Count; i++)
            {
                var instancedPattern = data.patterns[i];
                var pattern = data.uniquePatterns[instancedPattern.pattern];
                for (int p = 0; p < pattern.notes.Count; p++)
                {
                    var rack = pattern.notes[p];
                    for (int r = 0; r < rack.notes.Count; r++)
                    {
                        var note = rack.notes[r];
                        if (!Exists(note))
                        {
                            double noteStart = note.start + instancedPattern.time;
                            double noteEnd = note.end + instancedPattern.time;

                            if (time >= noteStart && time < noteEnd)
                            {
                                var synthesizer = Get(data.instruments[p]);
                                synthesizer.Add(note);

                                //note entered
                                Note localNote = new Note(data.instruments[p], note, instancedPattern);
                                notesPlaying.Add(localNote);

                                break;
                            }
                        }
                    }
                }
            }

            goto Play;

            //play these notes!
            Play:
            {
                if (notesPlaying.Count > 0)
                {
                    for (int n = 0; n < notesPlaying.Count; n++)
                    {
                        if (time < notesPlaying[n].start || time >= notesPlaying[n].end)
                        {
                            var synthesizer = Get(notesPlaying[n].instrument);
                            synthesizer.Remove(notesPlaying[n].note);

                            //note exited
                            notesPlaying.RemoveAt(n);

                            break;
                        }
                    }
                }
            }
        }

        private Synthesizer Get(string name)
        {
            for (int i = 0; i < synthesizers.Count; i++)
            {
                if (name == synthesizers[i].preset.Name) return synthesizers[i];
            }
            return null;
        }

        private bool Exists(Synthy.Note note)
        {
            for (int i = 0; i < notesPlaying.Count; i++)
            {
                if (notesPlaying[i].note == note) return true;
            }

            return false;
        }

        private Note Get(Synthy.Note note)
        {
            for (int i = 0; i < notesPlaying.Count; i++)
            {
                if (notesPlaying[i].note == note) return notesPlaying[i];
            }

            return null;
        }
    }
}