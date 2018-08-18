using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Synthy
{
    public class EditorSequencer
    {
        public const byte MaxTracks = 6;
        public const ushort DefaultDuration = 200;
        public const string Extension = "sdata";

        public static Color BorderColor = new Color(0f, 0f, 0f, 1f);
        public static Color HoverColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        public static Color NormalColor = new Color(0.15f, 0.15f, 0.15f, 1f);

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
            string path = EditorUtility.OpenFilePanel("Open audio track...", Application.dataPath, Extension + ",asset");
            Load(path);
        }

        public static void Load(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            if(path.EndsWith(Extension))
            {
                //load as JSON
                string json = File.ReadAllText(path);
                Track track = JsonUtility.FromJson<Track>(json);
                Current = track;

                //save the loaded path to a string
                //so when scripts reload at runtime
                //the sequencer can reload the old data (if it was saved)
                EditorPrefs.SetString("synth_file_path", path);
            }
            else
            {
                //load as SO
                string name = Path.GetFileNameWithoutExtension(path);
                var trackAsset = AssetDatabase.LoadAssetAtPath<TrackAsset>("Assets\\" + name + ".asset");
                Current = trackAsset.track;
            }
        }

        [MenuItem("Synthesizer/Sequencer/File/Save to SO")]
        public static void SaveSO()
        {
            if (Current != null)
            {
                //if its the first time saving, then open the save file dialogue
                //if the track has been saved more than once, then use the default path
                //that was stored when the track loaded
                string path = "Assets\\" + Current.name + ".asset";

                Current.saves++;
                TrackAsset asset = ScriptableObject.CreateInstance<TrackAsset>();
                asset.track = new Track(Current);
                AssetDatabase.CreateAsset(asset, path);

                AssetDatabase.Refresh();
            }
            else
            {
                throw new Exception("Nothing to save.");
            }
        }

        [MenuItem("Synthesizer/Sequencer/File/Save to JSON")]
        public static void SaveJSON()
        {
            if (Current != null)
            {
                //if its the first time saving, then open the save file dialogue
                //if the track has been saved more than once, then use the default path
                //that was stored when the track loaded
                string path = EditorPrefs.GetString("synth_file_path");
                if (Current.saves == 0)
                {
                    path = EditorUtility.SaveFilePanel("Saving new audio track...", Application.dataPath, Current.name, Extension);
                }

                path = path.Replace(".asset", "." + Extension);

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
            //EditorGUI.DrawRect(rect, BorderColor);

            if (contains)
            {
                //draw highligthed box
                EditorGUI.DrawRect(rect, HoverColor);
            }
            else
            {
                //draw normal box
                EditorGUI.DrawRect(rect, NormalColor);
            }

            //draw label
            GUI.Label(rect, name, EditorStyles.largeLabel);
            return contains && Event.current.type == EventType.MouseUp;
        }
    }
}