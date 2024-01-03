using System;
using System.Collections;
using System.Collections.Generic;
using SpatialSys.UnitySDK;
using UnityEngine;

/// <summary>
/// The obby cam duplicates the position of the default Spatial Camera. But allows us to freeze its position, then lerp back to the spatial cam.
/// Important for when the player dies.
/// </summary>
public class ObbySmoothCamera : MonoBehaviour
{
    public static ObbySmoothCamera instance;

    public float freezeCameraDuration = 1f;
    public ParticleSystem deathParticles;

    private Transform target;
    private Coroutine freezeCo;

    private void Awake()
    {
        instance = this;
    }

    private IEnumerator DeathCo(Vector3 position, Action callback)
    {
        if (target == null)
        {
            target = new GameObject().transform;
        }
        //todo: currently avatar position != the anchor point of the camera. Need to get proper camera position at time of death here.
        target.position = position + Vector3.up * 1.5f;//temp add a bit of height to estimate camera position
        SpatialBridge.cameraService.SetTargetOverride(target, SpatialCameraMode.Actor);
        if (deathParticles != null)
        {
            deathParticles.transform.position = target.position;
            deathParticles.Play();
        }
        yield return new WaitForSeconds(freezeCameraDuration);
        SpatialBridge.cameraService.ClearTargetOverride();
        callback?.Invoke();
    }

    public static void FreezeCamOnPosition(Vector3 position, Action callback)
    {
        if (!instance)
        {
            return;
        }
        if (instance.freezeCo != null)
        {
            instance.StopCoroutine(instance.freezeCo);
        }
        instance.freezeCo = instance.StartCoroutine(instance.DeathCo(position, callback));
    }
}
