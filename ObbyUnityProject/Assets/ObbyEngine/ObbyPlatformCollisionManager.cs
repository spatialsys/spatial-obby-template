using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpatialSys.UnitySDK;

/// <summary>
/// Handles player collisions with ObbyPlatforms.
/// </summary>
public class ObbyPlatformCollisionManager : MonoBehaviour
{
    //TODO: Handle this in arrays, we generate lots of garbage this way.
    private HashSet<ObbyPlatform> activePlatformCollisions = new HashSet<ObbyPlatform>();
    private HashSet<ObbyPlatform> lastFramePlatformCollisions = new HashSet<ObbyPlatform>();

    private void OnEnable()
    {
        SpatialBridge.actorService.localActor.avatar.onColliderHit += OnPlayerCollision;
    }
    private void OnDisable()
    {
        SpatialBridge.actorService.localActor.avatar.onColliderHit -= OnPlayerCollision;
    }

    private void OnPlayerCollision(ControllerColliderHit hit, Vector3 velocity)
    {
        if (hit.gameObject.TryGetComponent(out ObbyPlatform platform))
        {
            if (!activePlatformCollisions.Contains(platform))
            {
                if (lastFramePlatformCollisions.Contains(platform))
                {
                    platform.OnPlayerStay.Invoke();
                }
                else
                {
                    platform.OnPlayerEnter.Invoke();
                }
                activePlatformCollisions.Add(platform);
            }
        }
    }

    private void LateUpdate()
    {
        foreach (ObbyPlatform platform in lastFramePlatformCollisions)
        {
            if (!activePlatformCollisions.Contains(platform))
            {
                platform.OnPlayerExit.Invoke();
            }
        }

        lastFramePlatformCollisions = new HashSet<ObbyPlatform>(activePlatformCollisions);
        activePlatformCollisions.Clear();
    }
}
