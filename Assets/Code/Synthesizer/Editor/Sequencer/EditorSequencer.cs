using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

public class EditorSequencer
{
    public const byte MaxTracks = 6;
    public const ushort DefaultDuration = 200;
    public const string Extension = "sdata";

    public static Color BorderColor = new Color(0f, 0f, 0f, 1f);
    public static Color HoverColor = new Color(0.2f, 0.2f, 0.2f, 1f);
    public static Color NormalColor = new Color(0.15f, 0.15f, 0.15f, 1f);

    [Serializable]
    public class Note
    {
        public byte note;
        public int start;
        public int end;

        public Note(byte note, int start, int end)
        {
            this.note = note;
            this.start = start;
            this.end = end;
        }
    }

    [Serializable]
    public class Rack
    {
        public List<Note> notes = new List<Note>();
    }

    [Serializable]
    public class Pattern
    {
        public string name;
        public List<Rack> notes = new List<Rack>();

        public int Duration => End - Start;

        public List<Note> Notes
        {
            get
            {
                if (this.notes.Count == 0) return new List<Note>();

                List<Note> notes = new List<Note>();
                foreach (var n in this.notes)
                {
                    notes.AddRange(n.notes);
                }

                return notes;
            }
        }

        public int End
        {
            get
            {
                var notes = Notes;
                if (notes.Count == 0) return DefaultDuration;

                int max = int.MinValue;
                int index = -1;
                for (int i = 0; i < notes.Count; i++)
                {
                    if (notes[i].end > max)
                    {
                        max = notes[i].end;
                        index = i;
                    }
                }

                return notes[index].end;
            }
        }

        public int Start
        {
            get
            {
                var notes = Notes;
                if (notes.Count == 0) return 0;

                int min = int.MaxValue;
                int index = -1;
                for (int i = 0; i < notes.Count; i++)
                {
                    if (notes[i].end < min)
                    {
                        min = notes[i].end;
                        index = i;
                    }
                }

                return notes[index].start;
            }
        }

        public byte Highest
        {
            get
            {
                var notes = Notes;
                if (notes.Count == 0) return Synthesizer.MiddleC;

                byte min = byte.MinValue;
                int index = -1;
                for (int i = 0; i < notes.Count; i++)
                {
                    if (notes[i].note > min)
                    {
                        min = notes[i].note;
                        index = i;
                    }
                }

                return notes[index].note;
            }
        }

        public byte Lowest
        {
            get
            {
                var notes = Notes;
                if (notes.Count == 0) return Synthesizer.MiddleC;

                byte max = byte.MaxValue;
                int index = -1;
                for (int i = 0; i < notes.Count; i++)
                {
                    if (notes[i].note < max)
                    {
                        max = notes[i].note;
                        index = i;
                    }
                }

                return notes[index].note;
            }
        }
        
        public Pattern(string name, Track track)
        {
            this.name = name;
            foreach (var instrument in track.instruments)
            {
                notes.Add(new Rack());
            }
        }
    }

    [Serializable]
    public class InstancedPattern
    {
        public string name;
        public int time;
        public byte order;
        public ushort pattern;

        public InstancedPattern(string name, int pattern)
        {
            this.name = name;
            this.pattern = (ushort)pattern;
        }

        public int GetEnd(Track track)
        {
            return track.uniquePatterns[pattern].End + time;
        }

        public int GetStart(Track track)
        {
            return track.uniquePatterns[pattern].Start + time;
        }

        public int GetLength(Track track)
        {
            return GetEnd(track) - time;
        }

        public Vector2 Position
        {
            get
            {
                return new Vector2(time + EditorSequencerPlaylist.LeftOffset, EditorSequencerPlaylist.TopOffset + order * EditorSequencerPlaylist.TrackHeight);
            }
        }
    }

    [Serializable]
    public class Track
    {
        public uint saves = 0;
        public string name = "Track";
        public ushort bpm = 130;
        public List<string> instruments = new List<string>();
        public List<InstancedPattern> patterns = new List<InstancedPattern>();
        public List<Pattern> uniquePatterns = new List<Pattern>();

        public Track()
        {
            instruments.Add("Saw");
            instruments.Add("Sine");
            instruments.Add("Pulse");

            Pattern pattern = new Pattern("Pattern 1", this);

            uniquePatterns.Add(pattern);
        }
    }

    public static Track Current { get; private set; } = new Track();

    [DidReloadScripts]
    private static void AssemblyReload()
    {
        string path = EditorPrefs.GetString("synth_file_path", "");
        if (path != "")
        {
            Load(path);
        }
    }

    [MenuItem("Synthesizer/Sequencer/File/New")]
    public static void New()
    {
        Current = new Track();

        EditorPrefs.SetString("synth_file_path", "");
    }

    [MenuItem("Synthesizer/Sequencer/File/Load")]
    public static void Load()
    {
        string path = EditorUtility.OpenFilePanel("Open audio track...", Application.dataPath, Extension);
        Load(path);
    }
    
    public static void Load(string path)
    {
        if (string.IsNullOrEmpty(path)) return;

        string json = File.ReadAllText(path);
        Track track = JsonUtility.FromJson<Track>(json);
        Current = track;

        //save the loaded path to a string
        //so when scripts reload at runtime
        //the sequencer can reload the old data (if it was saved)
        EditorPrefs.SetString("synth_file_path", path);
    }

    [MenuItem("Synthesizer/Sequencer/File/Save")]
    public static void Save()
    {
        if(Current != null)
        {
            //if its the first time saving, then open the save file dialogue
            //if the track has been saved more than once, then use the default path
            //that was stored when the track loaded
            string path = EditorPrefs.GetString("synth_file_path");
            if (Current.saves == 0)
            {
                path = EditorUtility.SaveFilePanel("Saving new audio track...", Application.dataPath, Current.name, Extension);
            }

            Current.saves++;
            string json = JsonUtility.ToJson(Current, true);
            File.WriteAllText(path, json);
            EditorPrefs.SetString("synth_file_path", path);

            AssetDatabase.Refresh();
        }
        else
        {
            throw new Exception("Nothing to save.");
        }
    }

    public static float Remap(float s, float a1, float a2, float b1, float b2)
    {
        return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
    }

    public static bool DrawBox(Rect rect, string name)
    {
        bool contains = rect.Contains(Event.current.mousePosition);

        //draw border
        EditorGUI.DrawRect(rect, BorderColor);

        if (contains)
        {
            //draw highligthed box
            EditorGUI.DrawRect(new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2), HoverColor);
        }
        else
        {
            //draw normal box
            EditorGUI.DrawRect(new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2), NormalColor);
        }

        //draw label
        GUI.Label(rect, name, EditorStyles.largeLabel);
        return contains && Event.current.type == EventType.MouseUp;
    }
}