using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages an obby course
/// </summary>
[ExecuteAlways]
public class ObbyCourse : MonoBehaviour
{
    public string courseName;

    public ObbyNode[] nodes = new ObbyNode[0];

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
        
    }
}
