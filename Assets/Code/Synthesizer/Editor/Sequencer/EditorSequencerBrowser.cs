using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Synthy
{
    using static EditorSequencer;
    public class EditorSequencerBrowser : EditorWindow
    {
        public const int PatternHeight = 30;

        [MenuItem("Synthesizer/Sequencer/Browser")]
        public static void Initialize()
        {
            //Show existing window instance. If one doesn't exist, make one.
            GetWindow<EditorSequencerBrowser>("Browser");
        }

        private void OnGUI()
        {
            Repaint();

            //display the create pattern button
            if (GUILayout.Button("New Pattern", EditorStyles.toolbarButton))
            {
                var pattern = new Pattern("Pattern " + (Current.uniquePatterns.Count + 1), Current);
                Current.uniquePatterns.Add(pattern);
            }

            //display a list of all the patterns
            foreach (var pattern in Current.uniquePatterns)
            {
                Rect rect = GUILayoutUtility.GetRect(Screen.width, PatternHeight);
                if (rect.Contains(Event.current.mousePosition))
                {
                    if (Event.current.type == EventType.MouseDown)
                    {
                        DragAndDrop.PrepareStartDrag();// reset data
                        DragAndDrop.SetGenericData("pattern_index", Current.uniquePatterns.IndexOf(pattern));
                        DragAndDrop.objectReferences = new Object[] { new Object() };
                        Event.current.Use();
                    }
                    if (Event.current.type == EventType.MouseDrag)
                    {
                        if (DragAndDrop.GetGenericData("pattern_index") != null)
                        {
                            DragAndDrop.StartDrag("Dragging " + pattern.name);
                            Event.current.Use();
                        }
                    }
                }

                if (DrawBox(rect, pattern.name))
                {
                    //open piano roll from here
                    EditorSequencerPianoRoll.Initialize(pattern);
                }
            }
        }
    }
}