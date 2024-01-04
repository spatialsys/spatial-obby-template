using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SpatialSys.UnitySDK;
using UnityEngine;

public class ObbyNode : MonoBehaviour
{
    public ObbyPlatform nodePlatform;

    private void OnEnable()
    {
        if (nodePlatform == null)
        {
            Debug.LogError("No node platform set!");
            return;
        }

        nodePlatform.OnPlayerEnter.AddListener(OnNodeReached);
    }

    private void OnNodeReached()
    {
        ObbyGameManager.HandleNodeEnter(this);
    }
}
