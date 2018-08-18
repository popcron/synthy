using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Synthy
{
    public class EditorSequencerMixer : EditorWindow
    {
        [MenuItem("Synthesizer/Sequencer/Mixer")]
        public static void Initialize()
        {
            //Show existing window instance. If one doesn't exist, make one.
            GetWindow<EditorSequencerMixer>("Mixer");
        }

        private void OnGUI()
        {
            GUILayout.Label("Mixer");
        }
    }
}