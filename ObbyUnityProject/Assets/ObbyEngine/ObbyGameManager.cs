using System.Collections;
using System.Collections.Generic;
using SpatialSys.UnitySDK;
using UnityEngine;

public class ObbyGameManager : MonoBehaviour
{
    public static ObbyGameManager instance;

    //TODO: Handle this in arrays, we generate lots of garbage this way.
    private HashSet<ObbyPlatform> activePlatformCollisions = new HashSet<ObbyPlatform>();
    private HashSet<ObbyPlatform> lastFramePlatformCollisions = new HashSet<ObbyPlatform>();

    public Transform spawnPoint;
    private Coroutine killCo;

    private void Start()
    {
        instance = this;
        SpatialBridge.actorService.localActor.avatar.onColliderHit += OnPlayerCollision;
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

    public void KillPlayer()
    {
        if (killCo != null)
        {
            StopCoroutine(killCo);
        }
        killCo = StartCoroutine(KillCoroutine());
    }

    private IEnumerator KillCoroutine()
    {
        SpatialBridge.SetInputOverrides.Invoke(true, true, true, true, gameObject);//disable player input.
        SpatialBridge.actorService.localActor.avatar.Move(Vector3.zero);//cancel any current movement input.
        ObbySmoothCamera.FreezeCamOnPosition(SpatialBridge.actorService.localActor.avatar.position, () =>
        {
            //regain input once camera returns.
            SpatialBridge.SetInputOverrides.Invoke(false, false, false, false, gameObject);
        });
        SpatialBridge.cameraService.Shake(3f);
        SpatialBridge.cameraService.Wobble(2f);
        SpatialBridge.actorService.localActor.avatar.velocity = Vector3.zero;
        //Kil() is triggered in the middle of the avatars internal movement update. Causing us to move through objects if we teleport immediatly.
        yield return new WaitForEndOfFrame();
        SpatialBridge.actorService.localActor.avatar.SetPositionRotation(spawnPoint.position, spawnPoint.rotation);
    }
}
