using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Synthy
{
    using System;
    using static EditorSequencer;
    public class EditorSequencerRack : EditorWindow
    {
        public const int RackHeight = 30;

        private double? lastClick;
        private static EditorSequencerRack instance;
        private List<Preset> presets = new List<Preset>();
        private int? instrumentSelected;
        private double nextUnselect;

        [MenuItem("Synthesizer/Sequencer/Instruments")]
        public static void Initialize()
        {
            //Show existing window instance. If one doesn't exist, make one.
            var window = GetWindow<EditorSequencerRack>("Instruments");
            instance = window;
            FindInstruments();
        }

        private void OnEnable()
        {
            instance = this;
            FindInstruments();
        }

        public static void FindInstruments()
        {
            instance.presets.Clear();
            var presets = AssetDatabase.FindAssets("t:Synthy.Preset");
            foreach (var preset in presets)
            {
                string path = AssetDatabase.GUIDToAssetPath(preset);
                var asset = AssetDatabase.LoadAssetAtPath<Preset>(path);
                instance.presets.Add(asset);
            }
        }

        private void OnGUI()
        {
            Repaint();
            Drag();

            //no instruments found in the track, show info
            if (Current.instruments.Count == 0)
            {
                EditorGUILayout.HelpBox("No instruments in the current track.\nAdd one by dragging in a preset asset.", MessageType.Info);
            }

            //draw list of all the instruments in this track
            bool selected = false;
            for (int i = 0; i < Current.instruments.Count; i++)
            {
                string instrument = Current.instruments[i];
                Rect rect = GUILayoutUtility.GetRect(Screen.width, RackHeight);
                if (rect.Contains(Event.current.mousePosition))
                {
                    instrumentSelected = i;
                    selected = true;
                    nextUnselect = EditorApplication.timeSinceStartup + 0.1;
                }

                if (DrawBox(rect, instrument))
                {
                    if(Event.current.button == 0)
                    {
                        //double click
                        if (lastClick != null && EditorApplication.timeSinceStartup - lastClick < 0.25)
                        {

                        }
                        else
                        {
                            //if double click failed
                            //select the preset that was added
                            EditorGUIUtility.PingObject(presets[i]);
                            Selection.activeObject = presets[i];
                            lastClick = EditorApplication.timeSinceStartup;
                        }
                    }
                    else if(Event.current.button == 1)
                    {
                        //remove the instrument from the track
                        Current.instruments.RemoveAt(i);
                        return;
                    }
                }
            }
            
            if (!selected && EditorApplication.timeSinceStartup > nextUnselect)
            {
                instrumentSelected = null;
            }
        }

        private void Drag()
        {
            if (Event.current.type == EventType.DragUpdated)
            {
                // called every frame that a drag operation is occuring and the mouse is being dragged
                if (DragAndDrop.objectReferences != null)
                {
                    object value = DragAndDrop.objectReferences[0];
                    Preset preset = value as Preset;
                    if (preset != null)
                    {
                        if (!Current.instruments.Contains(preset.Name))
                        {
                            DragAndDrop.visualMode = DragAndDropVisualMode.Link;
                        }
                    }
                }
            }
            else if (Event.current.type == EventType.DragPerform)
            {
                // called on the frame that the dragged object gets dropped (if it's still inside this window)
                if (DragAndDrop.objectReferences != null)
                {
                    object value = DragAndDrop.objectReferences[0];
                    Preset preset = value as Preset;
                    if (preset != null)
                    {
                        if(instrumentSelected != null)
                        {
                            //dropped instrument on top of another one
                            //replace it
                            Current.instruments[instrumentSelected.Value] = preset.Name;
                        }
                        else
                        {
                            if (!Current.instruments.Contains(preset.Name))
                            {
                                //Debug.Log("Dropped " + preset);
                                DragAndDrop.AcceptDrag();

                                //add the preset name as an instrument
                                Current.instruments.Add(preset.Name);

                                //also add a new list of notes to every pattern if its missing a list
                                foreach (var pattern in Current.uniquePatterns)
                                {
                                    if (pattern.notes.Count < Current.instruments.Count)
                                    {
                                        pattern.notes.Add(new Rack());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}