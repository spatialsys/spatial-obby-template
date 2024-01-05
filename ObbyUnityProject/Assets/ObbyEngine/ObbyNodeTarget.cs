using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObbyNodeTarget : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = ObbySettings.editorColor;
        //local matrix
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(4f, 1f, 0f));
    }
}
