using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Synthy
{
    public class EditorSequencerToolbar : EditorWindow
    {
        [MenuItem("Synthesizer/Sequencer/Toolbar")]
        public static void Initialize()
        {
            //Show existing window instance. If one doesn't exist, make one.
            GetWindow<EditorSequencerToolbar>("Toolbar");
        }

        private void OnGUI()
        {
            GUILayout.Label("Toolbar");
        }
    }
}