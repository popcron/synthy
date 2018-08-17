using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ClampedCurveAttribute))]
public class EditorClampedCurveAttribute : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.hasMultipleDifferentValues)
        {
            EditorGUI.LabelField(position, label.text, "---");
            return;
        }

        EditorGUI.CurveField(position, property, Color.yellow, new Rect(0, 0, 1, 1));
    }
}