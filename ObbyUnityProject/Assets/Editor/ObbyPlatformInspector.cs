using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObbyPlatform)), CanEditMultipleObjects]
public class ObbyPlatformInspector : Editor
{
    private SerializedProperty _movementMaterial;
    private SerializedProperty _actorEffect;

    private SerializedProperty _onPlayerEnter;
    private SerializedProperty _onPlayerExit;
    private SerializedProperty _onPlayerStay;
    private bool eventsFoldout = false;

    void OnEnable()
    {
        _movementMaterial = serializedObject.FindProperty(nameof(ObbyPlatform.movementMaterial));
        _actorEffect = serializedObject.FindProperty(nameof(ObbyPlatform.actorEffect));
        _onPlayerEnter = serializedObject.FindProperty(nameof(ObbyPlatform.OnPlayerEnter));
        _onPlayerExit = serializedObject.FindProperty(nameof(ObbyPlatform.OnPlayerExit));
        _onPlayerStay = serializedObject.FindProperty(nameof(ObbyPlatform.OnPlayerStay));
    }

    public void OnSceneGUI()
    {
        var t = target as ObbyPlatform;
        switch (t.actorEffect)
        {
            case ObbyPlatformActorEffect.SetVelocity:
                if (t.force.magnitude > 0)
                {
                    Handles.color = new Color(ObbySettings.launchColor.r, ObbySettings.launchColor.g, ObbySettings.launchColor.b, .25f);
                    Handles.ArrowHandleCap(-1, t.transform.position, Quaternion.LookRotation(t.force), 1f, EventType.Repaint);
                }
                break;
            case ObbyPlatformActorEffect.Force:
                if (t.force.magnitude > 0)
                {
                    Handles.color = new Color(ObbySettings.forceColor.r, ObbySettings.forceColor.g, ObbySettings.forceColor.b, .25f);
                    Handles.ArrowHandleCap(-1, t.transform.position, Quaternion.LookRotation(t.force), 1f, EventType.Repaint);
                }
                break;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var platform = target as ObbyPlatform;

        EditorGUILayout.LabelField("Surface", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_movementMaterial);
        //Header
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Actor Effect", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_actorEffect, new GUIContent("Effect"));
        switch ((ObbyPlatformActorEffect)_actorEffect.enumValueIndex)
        {
            case ObbyPlatformActorEffect.Kill:
                EditorGUILayout.HelpBox("Kill the player, playing the active courses DeathVFX and respawning the player at the last checkpoint.", MessageType.None);
                break;
            case ObbyPlatformActorEffect.SetVelocity:
                EditorGUILayout.HelpBox("Add the force to the player instantly when they enter the platform.", MessageType.None);
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ObbyPlatform.force)));
                break;
            case ObbyPlatformActorEffect.Force:
                EditorGUILayout.HelpBox("Add the force to the player over time while they are touching the platform.", MessageType.None);
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ObbyPlatform.force)));
                break;
        }

        EditorGUILayout.Space();
        eventsFoldout = EditorGUILayout.Foldout(eventsFoldout, "Events", true, EditorStyles.foldoutHeader);
        if (eventsFoldout)
        {
            EditorGUILayout.PropertyField(_onPlayerEnter);
            EditorGUILayout.PropertyField(_onPlayerExit);
            EditorGUILayout.PropertyField(_onPlayerStay);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
