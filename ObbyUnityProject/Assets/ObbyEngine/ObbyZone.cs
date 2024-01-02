using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SpatialSys.UnitySDK;
using System.Security.Cryptography;

public enum ObbyZoneActorEffect
{
    None,
    Kill,
    Force,
    SpeedMultiplier,
}

[RequireComponent(typeof(Collider))]
[DefaultExecutionOrder(50)]
public class ObbyZone : MonoBehaviour
{
    public const float MIN_SPEED_MULTIPLIER = 0.1f;
    private static int PLAYER_LAYER = 30;

    //Affectors
    public ObbyZoneActorEffect actorEffect = ObbyZoneActorEffect.None;
    public ForceSpace forceSpace = ForceSpace.Local;
    public Vector3 force = Vector3.zero;
    public float speedMultiplier;
    public UnityEvent OnPlayerEnter;
    public UnityEvent OnPlayerExit;
    public UnityEvent OnPlayerStay;

    private Collider _collider;

    void Awake()
    {
        OnPlayerEnter.AddListener(OnEnter);
        OnPlayerStay.AddListener(OnStay);
        OnPlayerExit.AddListener(OnExit);
    }

    void OnValidate() {
        if (_collider == null)
        {
            _collider = GetComponent<Collider>();
        }
        _collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other) {
        if (ColliderIsPlayer(other))
            OnPlayerEnter.Invoke();
    }

    private void OnTriggerStay(Collider other) {
        if (ColliderIsPlayer(other))
            OnPlayerStay.Invoke();
    }

    private void OnTriggerExit(Collider other) {
        if (ColliderIsPlayer(other))
            OnPlayerExit.Invoke();
    }

    private void OnEnter()
    {
        switch (actorEffect)
        {
            case ObbyZoneActorEffect.Kill:
                KillPlayer();
                break;
            case ObbyZoneActorEffect.SpeedMultiplier:
                ApplySpeedMultiplier();
                break;
        }
    }

    private void OnStay()
    {
        switch (actorEffect)
        {
            case ObbyZoneActorEffect.Force:
                ApplyForceToPlayer();
                break;
        }
    }

    private void OnExit()
    {
        switch (actorEffect)
        {
            case ObbyZoneActorEffect.SpeedMultiplier:
                RemoveSpeedMultiplier();
                break;
        }
    }

    private void KillPlayer()
    {
        ObbyGameManager.KillPlayer();
    }


    private void ApplyForceToPlayer()
    {
        SpatialBridge.actorService.localActor.avatar.AddForce((forceSpace == ForceSpace.World ? force : transform.rotation * force) * Time.deltaTime);
    }

    private void ApplySpeedMultiplier() {
        SpatialBridge.actorService.localActor.avatar.runSpeed *= speedMultiplier;
        SpatialBridge.actorService.localActor.avatar.walkSpeed *= speedMultiplier;
    }

    private void RemoveSpeedMultiplier() {
        SpatialBridge.actorService.localActor.avatar.runSpeed /= speedMultiplier;
        SpatialBridge.actorService.localActor.avatar.walkSpeed /= speedMultiplier;
    }

    private bool ColliderIsPlayer(Collider collider)
    {
        return collider.gameObject.layer == PLAYER_LAYER;
    }

    private void OnDrawGizmos()
    {
        if (_collider == null)
        {
            _collider = GetComponent<Collider>();
        }
        switch (actorEffect)
        {
            case ObbyZoneActorEffect.None:
                Gizmos.color = ObbySettings.nodePlatformColor;
                break;
            case ObbyZoneActorEffect.Kill:
                Gizmos.color = ObbySettings.killColor;
                break;
            case ObbyZoneActorEffect.Force:
                Gizmos.color = ObbySettings.forceColor;
                break;
            case ObbyZoneActorEffect.SpeedMultiplier:
                Gizmos.color = ObbySettings.speedMultiplierColor;
                break;
        }

        Gizmos.matrix = transform.localToWorldMatrix;
        if (_collider != null && _collider is MeshCollider meshCollider && meshCollider.sharedMesh)
        {
            Gizmos.DrawWireMesh(meshCollider.sharedMesh, Vector3.zero, Quaternion.identity, Vector3.one);
        }
        else if (_collider != null && _collider is BoxCollider boxCollider)
        {
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
        }
        else if (_collider != null && _collider is SphereCollider sphereCollider)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            //Get the largest value of the scale
            Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z));
        }
        else if (_collider != null && _collider is CapsuleCollider capsuleCollider)
        {
            //TODO: Draw capsule
        }
    }
}
