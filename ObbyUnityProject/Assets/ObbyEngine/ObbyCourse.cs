using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages an obby course
/// </summary>
public class ObbyCourse : MonoBehaviour
{
    public string courseID;

    public ObbyNode[] nodes = new ObbyNode[0];

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
        
    }

    public bool TryGetIndexOfNode(ObbyNode node, out int index)
    {
        for (int i = 0; i < nodes.Length; i++)
        {
            if (nodes[i] == node)
            {
                index = i;
                return true;
            }
        }
        index = -1;
        return false;
    }

    public bool TryGetNodeAtIndex(int index, out ObbyNode node)
    {
        if (index < 0 || index >= nodes.Length)
        {
            node = null;
            return false;
        }
        node = nodes[index];
        return true;
    }
}
