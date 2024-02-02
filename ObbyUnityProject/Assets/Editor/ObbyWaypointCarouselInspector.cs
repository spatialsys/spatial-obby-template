using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObbyWaypointCarousel))]
public class ObbyWaypointCarouselInspector : Editor
{
    private SerializedProperty _waypoints;
    private SerializedProperty _platforms;
    private SerializedProperty _speed;
    private SerializedProperty _loopType;
    private SerializedProperty _spacingType;
    void OnEnable() {
        _speed = serializedObject.FindProperty(nameof(ObbyWaypointCarousel.speed));
        _loopType = serializedObject.FindProperty(nameof(ObbyWaypointCarousel.loopType));
        _waypoints = serializedObject.FindProperty(nameof(ObbyWaypointCarousel._waypoints));
        _spacingType = serializedObject.FindProperty(nameof(ObbyWaypointCarousel.spacingType));
        _platforms = serializedObject.FindProperty(nameof(ObbyWaypointCarousel._platforms));
    }

    public override void OnInspectorGUI()
    {
        ObbyWaypointCarousel obj = target as ObbyWaypointCarousel;
        serializedObject.Update();
        EditorGUILayout.PropertyField(_waypoints);
        if (_waypoints.arraySize < 2) {
            EditorGUILayout.HelpBox("Warning: requires 2 waypoints to function", MessageType.Warning);
        } 

        EditorGUILayout.PropertyField(_platforms);

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(_loopType, new GUIContent("Loop Type"));
        switch ((WaypointLoopType)_loopType.enumValueIndex)
        {
            case WaypointLoopType.PingPong:
                EditorGUILayout.HelpBox("All platforms reverse direction once the last waypoint is reached.", MessageType.None);
                SerializedProperty _pauseAtEnds = serializedObject.FindProperty(nameof(ObbyWaypointCarousel.pauseAtEnds));
                EditorGUILayout.PropertyField(_pauseAtEnds);
                if (_pauseAtEnds.boolValue) {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ObbyWaypointCarousel.pauseTime)));
                }
                break;
            case WaypointLoopType.Loop:
                EditorGUILayout.HelpBox("Platforms loop back from the last waypoint to the first waypoint.", MessageType.None);
                break;
        }

        if (_platforms.arraySize > 1) {
            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(_spacingType, new GUIContent("Spacing Type"));
            switch ((WaypointSpacingType)_spacingType.enumValueIndex)
            {
                case WaypointSpacingType.Max:
                    EditorGUILayout.HelpBox("All platforms are evenly spaced as far apart as possible.", MessageType.None);
                    break;
                case WaypointSpacingType.Custom:
                    EditorGUILayout.HelpBox("Set a custom amount of space between every platform.", MessageType.None);
                    SerializedProperty _customSpacing = serializedObject.FindProperty(nameof(ObbyWaypointCarousel.customSpacing));
                    _customSpacing.floatValue = EditorGUILayout.Slider(new GUIContent("Custom Spacing"), _customSpacing.floatValue, 0, obj.maxSpacing);
                    break;
            }
        } 

        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(_speed);
        EditorGUILayout.HelpBox("Speed of all platforms, capped at 100.", MessageType.None);
        
        serializedObject.ApplyModifiedProperties();
        (target as ObbyWaypointCarousel).SyncPosition();
    }

}
