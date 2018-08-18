using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ClampedCurveAttribute))]
public class EditorClampedCurveAttribute : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.CurveField(position, property, Color.yellow, new Rect(0, 0, 1, 1));
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 30;
    }
}