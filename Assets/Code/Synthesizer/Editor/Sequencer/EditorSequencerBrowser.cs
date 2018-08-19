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

        public static int? Selected { get; set; }

        private double? lastClick;

        [MenuItem("Synthesizer/Sequencer/Browser")]
        public static void Initialize()
        {
            //Show existing window instance. If one doesn't exist, make one.
            GetWindow<EditorSequencerBrowser>("Browser");
        }

        private void OnEnable()
        {
            Selected = null;
        }

        private void OnGUI()
        {
            Repaint();

            //display the create pattern button
            if (GUILayout.Button("New Pattern", EditorStyles.toolbarButton))
            {
                int runs = 1;
                string patternName = "Pattern " + runs;
                while (true)
                {
                    runs++;

                    //check if this pattern name already exists
                    bool exists = false;
                    for (int i = 0; i < Current.uniquePatterns.Count; i++)
                    {
                        if(Current.uniquePatterns[i].name == patternName)
                        {
                            //this pattern name is already taken,
                            //make a new one

                            patternName = "Pattern " + runs;
                            exists = true;
                            break;
                        }
                    }

                    if(!exists)
                    {
                        //pattern with this name doesnt exist
                        //so break
                        break;
                    }
                }
                var pattern = new Pattern(patternName, Current);
                Current.uniquePatterns.Add(pattern);
            }

            //no patterns found in the track, show info
            if (Current.uniquePatterns.Count == 0)
            {
                EditorGUILayout.HelpBox("No patterns in the current track.", MessageType.Info);
            }

            //display a list of all the patterns
            bool deselect = true;
            for (int i = 0; i < Current.uniquePatterns.Count; i++)
            {
                Pattern pattern = Current.uniquePatterns[i];
                Rect rect = GUILayoutUtility.GetRect(Screen.width, PatternHeight);
                if (rect.Contains(Event.current.mousePosition))
                {
                    deselect = false;
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

                if (DrawBox(rect, pattern.name, Selected == i ? 1.2f : 0.75f))
                {
                    //open piano roll on left click
                    //delete on right click
                    if (Event.current.button == 0)
                    {
                        //double click to open piano roll
                        if (lastClick != null && EditorApplication.timeSinceStartup - lastClick < 0.25)
                        {
                            EditorSequencerPianoRoll.Initialize(pattern);
                            lastClick = null;
                        }
                        else
                        {
                            //if double click failed
                            //simply select it on the browser
                            if(Selected != i)
                            {
                                Selected = i;
                                lastClick = null;
                            }
                            else
                            {
                                lastClick = EditorApplication.timeSinceStartup;
                            }
                        }
                    }
                    else if(Event.current.button == 1)
                    {
                        //deleted a selected pattern, reset the selection index
                        if(Selected == i)
                        {
                            Selected = null;
                        }

                        Current.uniquePatterns.Remove(pattern);

                        //also delete any patterns on the playlist that reference this pattern
                        int d = 0;
                        while (d < Current.patterns.Count)
                        {
                            if (Current.patterns[d].pattern == i)
                            {
                                Current.patterns.RemoveAt(d);

                                //deleted a pattern
                                //restart the loop and try again until this reaches the last index
                                d = 0;
                                continue;
                            }

                            d++;
                        }

                        //shift other patterns down
                        for (int p = 0; p < Current.patterns.Count; p++)
                        {
                            if (Current.patterns[p].pattern != 0)
                            {
                                Current.patterns[p].pattern--;
                            }
                        }

                        return;
                    }
                }
            }

            //clicked on nothing
            if(deselect)
            {
                if(Event.current.button == 0)
                {
                    if(Event.current.type == EventType.MouseDown)
                    {
                        Selected = null;
                    }
                }
            }
        }
    }
}