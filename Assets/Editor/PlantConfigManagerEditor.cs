using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlantConfigManager))]
public class PlantConfigManagerEditor : Editor
{
    private SerializedProperty stemGradient;
    private SerializedProperty rootGradient;
    private SerializedProperty woodGradient;
    private SerializedProperty widths;
    private SerializedProperty defaultStemWidthIndex;
    private SerializedProperty defaultRootWidthIndex;
    private SerializedProperty defaultWoodWidthIndex;
    private SerializedProperty minStemHeight;
    private SerializedProperty maxRootHeight;

    private void OnEnable()
    {
        stemGradient = serializedObject.FindProperty("stemGradient");
        rootGradient = serializedObject.FindProperty("rootGradient");
        woodGradient = serializedObject.FindProperty("woodGradient");
        defaultStemWidthIndex = serializedObject.FindProperty("defaultStemWidthIndex");
        defaultRootWidthIndex = serializedObject.FindProperty("defaultRootWidthIndex");
        defaultWoodWidthIndex = serializedObject.FindProperty("defaultWoodWidthIndex");
        widths = serializedObject.FindProperty("edgeWidthsOnLevels");
        minStemHeight = serializedObject.FindProperty("minStemHeight");
        maxRootHeight = serializedObject.FindProperty("maxRootHeight");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(stemGradient);
        EditorGUILayout.PropertyField(rootGradient);
        EditorGUILayout.PropertyField(woodGradient);
        EditorGUILayout.PropertyField(defaultStemWidthIndex);
        EditorGUILayout.PropertyField(defaultRootWidthIndex);
        EditorGUILayout.PropertyField(defaultWoodWidthIndex);
        EditorGUILayout.PropertyField(widths);
        EditorGUILayout.PropertyField(minStemHeight);
        EditorGUILayout.PropertyField(maxRootHeight);
        serializedObject.ApplyModifiedProperties();
    }
}
