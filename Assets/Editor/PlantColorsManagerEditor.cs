using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlantColorsManager))]
public class PlantColorsManagerEditor : Editor
{
    private SerializedProperty stemGradient;
    private SerializedProperty rootGradient;
    private SerializedProperty woodGradient;

    private void OnEnable()
    {
        stemGradient = serializedObject.FindProperty("stemGradient");
        rootGradient = serializedObject.FindProperty("rootGradient");
        woodGradient = serializedObject.FindProperty("woodGradient");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(stemGradient);
        EditorGUILayout.PropertyField(rootGradient);
        EditorGUILayout.PropertyField(woodGradient);
        serializedObject.ApplyModifiedProperties();
    }
}
