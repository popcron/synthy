using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Synthy
{
    using static EditorSequencer;
    public class EditorSequencerRack : EditorWindow
    {
        public const int RackHeight = 30;

        [MenuItem("Synthesizer/Sequencer/Instruments")]
        public static void Initialize()
        {
            //Show existing window instance. If one doesn't exist, make one.
            GetWindow<EditorSequencerRack>("Instruments");
        }

        private void OnGUI()
        {
            Repaint();

            //display the create pattern button
            if (GUILayout.Button("New Instrument", EditorStyles.toolbarButton))
            {
                string instrument = "Saw";
                Current.instruments.Add(instrument);
            }

            //draw list of all the instruments in this track
            for (int i = 0; i < Current.instruments.Count; i++)
            {
                string instrument = Current.instruments[i];
                Rect rect = GUILayoutUtility.GetRect(Screen.width, RackHeight);
                if (DrawBox(rect, instrument))
                {

                }
            }
        }
    }
}