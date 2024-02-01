using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using SpatialSys.UnitySDK;

public enum ObbyPlatformActorEffect
{
    None,
    Kill,
    Force,
    Trampoline,
}

public enum ForceSpace
{
    Local,
    World,
}

[RequireComponent(typeof(Collider))]
[DefaultExecutionOrder(50)]
public class ObbyPlatform : MonoBehaviour
{
    private static int PLAYER_LAYER = 30;

    // Settings
    public SpatialMovementMaterial movementMaterial;

    //Affectors
    public ObbyPlatformActorEffect actorEffect = ObbyPlatformActorEffect.None;
    public ForceSpace forceSpace = ForceSpace.Local;
    public Vector3 force = Vector3.zero;
    public float trampolineHeight = 0;

    public UnityEvent OnPlayerEnter;
    public UnityEvent OnPlayerExit;
    public UnityEvent OnPlayerStay;

    private Collider _collider;

    private void Awake()
    {
        //Apply movement material to all colliders on this object
        if (movementMaterial != null)
        {
            List<Collider> colliders = new List<Collider>();
            gameObject.GetComponentsInChildren<Collider>(true, colliders);

            colliders.ForEach(collider =>
            {
                if (!TryGetComponent(out SpatialMovementMaterialSurface surface))
                {
                    surface = gameObject.AddComponent<SpatialMovementMaterialSurface>();
                }
                surface.movementMaterial = movementMaterial;
            });
        }

        OnPlayerEnter.AddListener(OnEnter);
        OnPlayerStay.AddListener(OnStay);
        OnPlayerExit.AddListener(OnExit);
    }

    private void OnEnter()
    {
        switch (actorEffect)
        {
            case ObbyPlatformActorEffect.Kill:
                KillPlayer();
                break;
        }
    }

    private void OnStay()
    {
        switch (actorEffect)
        {
            case ObbyPlatformActorEffect.Force:
                ApplyForceToPlayer();
                break;
            case ObbyPlatformActorEffect.Trampoline:
                BouncePlayer();
                break;
        }
    }

    private void OnExit()
    {
    }

    private bool CollisionIsPlayer(Collision collision)
    {
        Debug.LogError(collision.gameObject.name);
        return collision.gameObject.layer == PLAYER_LAYER;
    }

    // * Effects
    private void KillPlayer()
    {
        ObbyGameManager.KillPlayer();
    }

    private void ApplyForceToPlayer()
    {
        SpatialBridge.actorService.localActor.avatar.AddForce((forceSpace == ForceSpace.World ? force : transform.rotation * force) * Time.deltaTime);
    }
    private void BouncePlayer() {
        SpatialBridge.actorService.localActor.avatar.AddForce(Vector3.up * trampolineHeight);
        SpatialBridge.actorService.localActor.avatar.Jump();
    }

    public Bounds GetBounds()
    {
        //get the bounds by looking through all children of this object
        Bounds bounds = new Bounds(transform.position, Vector3.zero);
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }
    public Bounds GetLocalBounds()
    {
        //get the bounds by looking through all children of this object
        Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.localBounds);
        }
        return new Bounds(bounds.center, new Vector3(bounds.size.x * transform.localScale.x, bounds.size.y * transform.localScale.y, bounds.size.z * transform.localScale.z));
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_collider == null)
        {
            _collider = GetComponent<Collider>();
        }
        switch (actorEffect)
        {
            case ObbyPlatformActorEffect.None:
                Gizmos.color = ObbySettings.nodePlatformColor;
                break;
            case ObbyPlatformActorEffect.Kill:
                Gizmos.color = ObbySettings.killColor;
                break;
            case ObbyPlatformActorEffect.Force:
                Gizmos.color = ObbySettings.forceColor;
                break;
            case ObbyPlatformActorEffect.Trampoline:
                Gizmos.color = ObbySettings.trampolineColor;
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
#endif
}
