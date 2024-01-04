using System.Collections;
using System.Collections.Generic;
using SpatialSys.UnitySDK;
using UnityEngine;

public class ObbyStartCourseInteractable : MonoBehaviour
{
    public ObbyCourse course;
    public void OnInteract()
    {
        if (ObbyGameManager.instance.saveFile.TryGetValue(course.courseID, out int savedIndex))
        {
            ObbyGameManager.SetCourseAndNode(course, savedIndex, true);
            return;
        }
        ObbyGameManager.SetCourseAndNode(course, 0, true);
    }
}
