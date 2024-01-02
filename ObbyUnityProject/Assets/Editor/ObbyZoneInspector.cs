using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObbyZone)), CanEditMultipleObjects]
public class ObbyZoneInspector : Editor
{
    private SerializedProperty _actorEffect;

    private SerializedProperty _onPlayerEnter;
    private SerializedProperty _onPlayerExit;
    private SerializedProperty _onPlayerStay;
    private bool eventsFoldout = false;

    void OnEnable()
    {
        _actorEffect = serializedObject.FindProperty(nameof(ObbyZone.actorEffect));
        _onPlayerEnter = serializedObject.FindProperty(nameof(ObbyZone.OnPlayerEnter));
        _onPlayerExit = serializedObject.FindProperty(nameof(ObbyZone.OnPlayerExit));
        _onPlayerStay = serializedObject.FindProperty(nameof(ObbyZone.OnPlayerStay));
    }

    public void OnSceneGUI()
    {
        var t = target as ObbyZone;
        switch (t.actorEffect)
        {
            case ObbyZoneActorEffect.Force:
                if (t.force.magnitude > 0)
                {
                    Handles.color = new Color(ObbySettings.forceColor.r, ObbySettings.forceColor.g, ObbySettings.forceColor.b, .25f);
                    Handles.ArrowHandleCap(-1, t.transform.position, Quaternion.LookRotation(t.forceSpace == ForceSpace.World ? t.force : t.transform.rotation * t.force), 1f, EventType.Repaint);
                }
                break;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var platform = target as ObbyZone;

        //Header
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Actor Effect", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_actorEffect, new GUIContent("Effect"));
        switch ((ObbyZoneActorEffect)_actorEffect.enumValueIndex)
        {
            case ObbyZoneActorEffect.Kill:
                EditorGUILayout.HelpBox("Kill the player, playing the active courses DeathVFX and respawning the player at the last checkpoint.", MessageType.None);
                break;
            case ObbyZoneActorEffect.Force:
                EditorGUILayout.HelpBox("Add the force to the player over time while they are touching the platform.", MessageType.None);
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ObbyZone.forceSpace)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ObbyZone.force)));
                break;
            case ObbyZoneActorEffect.SpeedMultiplier:
                EditorGUILayout.HelpBox("Apply a multipler to player's speed.\nCan be negative, magnitude limited from [0.1, 10].", MessageType.None);
                SerializedProperty _speedMultiplier = serializedObject.FindProperty(nameof(ObbyZone.speedMultiplier));
                EditorGUILayout.PropertyField(_speedMultiplier);
                if (Mathf.Abs(_speedMultiplier.floatValue) < ObbyZone.MIN_SPEED_MULTIPLIER) {
                    if (_speedMultiplier.floatValue >= 0) {
                        _speedMultiplier.floatValue = ObbyZone.MIN_SPEED_MULTIPLIER;
                    }
                    else {
                        _speedMultiplier.floatValue = -ObbyZone.MIN_SPEED_MULTIPLIER;
                    }
                }
                _speedMultiplier.floatValue = Mathf.Clamp(_speedMultiplier.floatValue, -10f, 10f);
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
