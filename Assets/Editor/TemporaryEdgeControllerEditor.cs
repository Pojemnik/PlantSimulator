using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TemporaryEdgeController))]
public class TemporaryEdgeControllerEditor : Editor
{
    private SerializedProperty defaultGradient;
    private SerializedProperty errorGradient;
    private SerializedProperty edge;

    private void OnEnable()
    {
        defaultGradient = serializedObject.FindProperty("defaultGradient");
        errorGradient = serializedObject.FindProperty("errorGradient");
        edge = serializedObject.FindProperty("edge");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(defaultGradient);
        EditorGUILayout.PropertyField(errorGradient);
        EditorGUILayout.PropertyField(edge);
        serializedObject.ApplyModifiedProperties();
    }
}
