using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpatialSys.UnitySDK;
using UnityEngine;

public class ObbyGameManager : MonoBehaviour
{
    public static ObbyGameManager instance;

    //* Inspector
    public ObbyCourse defaultCourse;
    [Tooltip("If a player enters a different course, should we treat this new course as the new active course? If false, the player will be teleported back to the last node they were on in the previous course.")]
    public bool allowCourseHopping = true;
    [Tooltip("When the user joins, should we teleport them to a node? If they have played before, they will be teleported to the last node they were on.")]
    public bool teleportPlayerToNodeOnStart = true;
    public ParticleSystem newNodeParticles;

    //* Properties
    public ObbyCourse currentCourse { get; private set; } = null;
    public int currentCourseNodeIndex { get; private set; } = -1;

    public Dictionary<string, int> saveFile { get; private set; } = new Dictionary<string, int>();

    //Called when we progress to a new node, or change courses.
    public event Action OnCurrentNodeChanged;

    private Coroutine killCo;

    private void Start()
    {
        instance = this;
        StartCoroutine(LoadFromSave());
    }

    private IEnumerator LoadFromSave()
    {
        yield return new WaitUntil(() => SpatialBridge.GetIsSceneInitialized.Invoke());

        //TODO: Load from datastore

        if (defaultCourse == null)
        {
            Debug.LogError("No default course set!");
            yield break;
        }

        SetCourseAndNode(defaultCourse, 0, teleportPlayerToNodeOnStart);
    }

    public static void HandleNodeEnter(ObbyNode node)
    {
        if (instance == null)
            return;

        if (instance.currentCourse.TryGetIndexOfNode(node, out int index))
        {
            if (index > instance.currentCourseNodeIndex)
            {
                if (instance.newNodeParticles != null)
                {
                    instance.newNodeParticles.transform.position = SpatialBridge.actorService.localActor.avatar.position;
                    instance.newNodeParticles.Play();
                }
                // we just reached a node further along in the course.
                SetCourseAndNode(instance.currentCourse, index, false);
            }
            else
            {
                // we just touched a node we already passed.
                return;
            }
        }
        else
        {
            if (!instance.TryGetCourseFromNode(node, out ObbyCourse course))
            {
                Debug.LogError("Node is not part of any course!");
                KillPlayer();
                return;
            }

            //We just entered a node that is part of a different course.
            if (instance.allowCourseHopping)
            {
                course.TryGetIndexOfNode(node, out int newCourseNodeIndex);
                int savedIndex = -1;
                instance.saveFile.TryGetValue(course.courseID, out savedIndex);
                if (newCourseNodeIndex > savedIndex && instance.newNodeParticles != null)
                {
                    instance.newNodeParticles.transform.position = SpatialBridge.actorService.localActor.avatar.position;
                    instance.newNodeParticles.Play();
                }
                SetCourseAndNode(course, Mathf.Max(newCourseNodeIndex, savedIndex), false);
            }
            else
            {
                SpatialBridge.coreGUIService.DisplayToastMessage("No course hopping!");
                KillPlayer();
            }
        }
    }

    public static void SetCourseAndNode(ObbyCourse course, int nodeIndex, bool teleport)
    {
        if (instance == null)
        {
            Debug.LogError("No ObbyGameManager instance!");
            return;
        }

        //Update save
        if (!instance.saveFile.ContainsKey(course.courseID))
        {
            instance.saveFile.Add(course.courseID, nodeIndex);
        }
        else
        {
            instance.saveFile[course.courseID] = Mathf.Max(instance.saveFile[course.courseID], nodeIndex);
        }

        bool isNewLocation = instance.currentCourse != course || instance.currentCourseNodeIndex != nodeIndex;
        instance.currentCourse = course;
        instance.currentCourseNodeIndex = nodeIndex;


        if (isNewLocation)
        {
            instance.OnCurrentNodeChanged?.Invoke();
        }

        if (teleport)
        {
            TeleportToNode(instance.currentCourse.nodes[instance.currentCourseNodeIndex]);
        }
    }

    public static void TeleportToNode(ObbyNode node)
    {
        if (instance == null || instance.currentCourse == null || node == null)
        {
            return;
        }

        if (node.nodePlatform != null)
        {
            //Raycast down to find the top of the platform.
            RaycastHit hit;
            Bounds bounds = node.nodePlatform.GetBounds();
            Vector3 top = bounds.center + Vector3.up * bounds.extents.y;
            if (Physics.Raycast(top + Vector3.up * 2.5f, Vector3.down, out hit, 200f))
            {
                SpatialBridge.actorService.localActor.avatar.SetPositionRotation(hit.point, Quaternion.LookRotation(node.nodePlatform.transform.forward, Vector3.up));
                return;
            }
            else
            {
                Debug.LogError("Failed to raycast down to platform!");
            }
        }
        else
        {
            Debug.LogError("Node has no platform assigned!");
            SpatialBridge.actorService.localActor.avatar.SetPositionRotation(node.transform.position, Quaternion.LookRotation(node.transform.forward, Vector3.up));
        }
    }

    public static void KillPlayer()
    {
        if (instance.killCo != null)
        {
            instance.StopCoroutine(instance.killCo);
        }
        instance.killCo = instance.StartCoroutine(instance.KillCoroutine());
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
        TeleportToNode(currentCourse.nodes[currentCourseNodeIndex]);
    }

    private bool TryGetCourseFromNode(ObbyNode node, out ObbyCourse course)
    {
        //search through all courses
        foreach (ObbyCourse c in FindObjectsOfType<ObbyCourse>())
        {
            if (c.nodes.Contains(node))
            {
                course = c;
                return true;
            }
        }
        course = null;
        return false;
    }
}
