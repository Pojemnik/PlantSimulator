using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TemporaryEdgeController))]
public class TemporaryEdgeControllerEditor : Editor
{
    private SerializedProperty defaultGradient;
    private SerializedProperty errorGradient;
    private SerializedProperty edgeStart;
    private SerializedProperty edgeEnd;
    private SerializedProperty edge;

    private void OnEnable()
    {
        defaultGradient = serializedObject.FindProperty("defaultGradient");
        errorGradient = serializedObject.FindProperty("errorGradient");
        edgeStart = serializedObject.FindProperty("edgeStart");
        edgeEnd = serializedObject.FindProperty("edgeEnd");
        edge = serializedObject.FindProperty("edge");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(defaultGradient);
        EditorGUILayout.PropertyField(errorGradient);
        EditorGUILayout.PropertyField(edgeStart);
        EditorGUILayout.PropertyField(edgeEnd);
        EditorGUILayout.PropertyField(edge);
        serializedObject.ApplyModifiedProperties();
    }
}
