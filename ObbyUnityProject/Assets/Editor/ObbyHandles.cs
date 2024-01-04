using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class ObbyHandles
{ //for some handles an alpha of 1 looks like alpha .7~
    //using colors with alpha 2 generally gets around this.
    public static Color handleBlack = new Color(0, 0, 0, 2);
    private static Texture _nodeTexture;
    public static Texture nodeTexture
    {
        get
        {
            if (_nodeTexture == null)
            {
                _nodeTexture = Resources.Load<Texture>("ObbyNodeIcon");
            }
            return _nodeTexture;
        }
    }
    private static Texture _startTexture;
    public static Texture startTexture
    {
        get
        {
            if (_startTexture == null)
            {
                _startTexture = Resources.Load<Texture>("ObbyStartIcon");
            }
            return _startTexture;
        }
    }
    private static Texture _endTexture;
    public static Texture endTexture
    {
        get
        {
            if (_endTexture == null)
            {
                _endTexture = Resources.Load<Texture>("ObbyFinishIcon");
            }
            return _endTexture;
        }
    }

    public static void DrawHandles(ObbyCourse obbyManager, SceneView sceneView)
    {
    }

    public static void DrawConnections(ObbyCourse obbyManager, SceneView sceneView)
    {
        for (int i = 1; i < obbyManager.nodes.Length; i++)
        {
            Handles.color = ObbySettings.editorColor;
            Handles.DrawLine(NodeTransform(obbyManager.nodes[i - 1]).position, NodeTransform(obbyManager.nodes[i]).position, 2f);

            Handles.color = ObbySettings.editorColor;
            //TODO: I think remove the plus handle.
            /*
            if (PlusHandle((NodeTransform(obbyManager.nodes[i - 1]).position + NodeTransform(obbyManager.nodes[i]).position) / 2f, .15f))
            {
                Debug.Log("Clicked");
            }
            */
        }
    }

    public static void DrawFlags(ObbyCourse obbyManager, SceneView sceneView)
    {
        for (int i = 0; i < obbyManager.nodes.Length; i++)
        {
            ObbyNode node = obbyManager.nodes[i];
            if (node == null)
            {
                continue;
            }
            if (i == 0)
            {
                DrawNodeFlag(node, startTexture);
            }
            else if (i == obbyManager.nodes.Length - 1)
            {
                DrawNodeFlag(node, endTexture);
            }
            else
            {
                DrawNodeFlag(node, nodeTexture);
            }
        }
    }

    //Draw a clickable Plus (+)
    private static bool PlusHandle(Vector3 position, float size)
    {
        size *= HandleUtility.GetHandleSize(position);
        var cameraRot = SceneView.currentDrawingSceneView.camera.transform.rotation;
        var pointsA = new Vector3[]{
                position + cameraRot * new Vector3(1f,.4f,0f) * size,
                position + cameraRot * new Vector3(1f,-.4f, 0f) * size,
                position + cameraRot * new Vector3(-1f,-.4f,0f) * size,
                position + cameraRot * new Vector3(-1f,.4f, 0f) * size,
            };
        var pointsB = new Vector3[]{
                position + cameraRot * new Vector3(.4f,1f,0f) * size,
                position + cameraRot * new Vector3(.4f,-1f, 0f) * size,
                position + cameraRot * new Vector3(-.4f,-1f,0f) * size,
                position + cameraRot * new Vector3(-.4f,1f, 0f) * size,
            };

        var pointsC = new Vector3[]{
                position + cameraRot * new Vector3(.7f,.1f,0f) * size,
                position + cameraRot * new Vector3(.7f,-.1f, 0f) * size,
                position + cameraRot * new Vector3(-.7f,-.1f,0f) * size,
                position + cameraRot * new Vector3(-.7f,.1f, 0f) * size,
            };
        var pointsD = new Vector3[]{
                position + cameraRot * new Vector3(.1f,.7f,0f) * size,
                position + cameraRot * new Vector3(.1f,-.7f, 0f) * size,
                position + cameraRot * new Vector3(-.1f,-.7f,0f) * size,
                position + cameraRot * new Vector3(-.1f,.7f, 0f) * size,
            };

        Handles.color = Color.white;
        Handles.DrawSolidRectangleWithOutline(pointsA, handleBlack, handleBlack);
        Handles.DrawSolidRectangleWithOutline(pointsB, handleBlack, handleBlack);
        Handles.DrawSolidRectangleWithOutline(pointsC, ObbySettings.editorColor, ObbySettings.editorColor);
        Handles.DrawSolidRectangleWithOutline(pointsD, ObbySettings.editorColor, ObbySettings.editorColor);

        Handles.color = Color.clear;
        return Handles.Button(position, cameraRot, size * 1.25f, size * 1.25f, Handles.CircleHandleCap);
    }

    private static void DrawNodeFlag(ObbyNode node, Texture texture2D)
    {
        Handles.color = handleBlack;
        Vector3 labelPos = NodeTransform(node).position + Vector3.up * 2f * HandleUtility.GetHandleSize(NodeTransform(node).position);
        Handles.DrawLine(NodeTransform(node).position, labelPos, 3f);
        Handles.Label(labelPos, new GUIContent(texture2D), new GUIStyle { fixedWidth = 32f, fixedHeight = 32f, alignment = TextAnchor.MiddleCenter });
        Handles.color = Color.clear;
        float handleSize = HandleUtility.GetHandleSize(NodeTransform(node).position) * .2f;
        if (Handles.Button(labelPos, SceneView.currentDrawingSceneView.camera.transform.rotation, handleSize, handleSize, Handles.CircleHandleCap))
        {
            Selection.activeGameObject = node.gameObject;
        }
    }

    private static Transform NodeTransform(ObbyNode node)
    {
        return node.nodePlatform == null ? node.transform : node.nodePlatform.transform;
    }

    public static void DrawBounds(ObbyCourse course, SceneView sceneView)
    {
        GUIStyle boundsWarning = new GUIStyle();
        boundsWarning.normal.textColor = Color.red;

        Bounds[] bounds = new Bounds[course.nodes.Length];
        for (int i = 0; i < course.nodes.Length; i++)
        {
            ObbyNode node = course.nodes[i];
            if (node == null)
            {
                continue;
            }
            //get the combined bounds of all children of node
            bounds[i] = new Bounds(node.transform.position, Vector3.zero);
            foreach (Renderer renderer in node.GetComponentsInChildren<Renderer>())
            {
                bounds[i].Encapsulate(renderer.bounds);
            }
            Handles.color = ObbySettings.nodeBoundsColors[i % ObbySettings.nodeBoundsColors.Length];
            Handles.DrawWireCube(bounds[i].center, bounds[i].size);
        }

        Bounds a;
        Bounds b;
        for (int i = 1; i < bounds.Length; i++)
        {
            a = bounds[i - 1];
            b = bounds[i];

            if (a.Intersects(b))
            {
                continue;
            }

            Vector3 edgeA = Vector3.zero;
            Vector3 edgeB = Vector3.zero;

            //compare each edge of the bounds to each other. Pick the shortest
            float shortestDistance = float.MaxValue;
            foreach (Vector3 aPoint in GetBoundsPoints(a))
            {
                foreach (Vector3 bPoint in GetBoundsPoints(b))
                {
                    float distance = Vector3.Distance(aPoint, bPoint);
                    if (distance < shortestDistance)
                    {
                        shortestDistance = distance;
                        edgeA = aPoint;
                        edgeB = bPoint;
                    }
                }
            }
            if (shortestDistance > ObbySettings.reccomendedBoundsDistance)
            {
                Handles.color = Color.red;
                Handles.DrawLine(edgeA, edgeB, 3f);
                Handles.Label((edgeA + edgeB) / 2f, "Too far apart!", new GUIStyle { alignment = TextAnchor.MiddleLeft });
            }
        }
    }

    private static Vector3[] GetBoundsPoints(Bounds bounds)
    {
        return new Vector3[]
        {
            bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, bounds.extents.z),
            bounds.center + new Vector3(bounds.extents.x, bounds.extents.y, -bounds.extents.z),
            bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, bounds.extents.z),
            bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y, -bounds.extents.z),
            bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, bounds.extents.z),
            bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y, -bounds.extents.z),
            bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, bounds.extents.z),
            bounds.center + new Vector3(-bounds.extents.x, -bounds.extents.y, -bounds.extents.z),
        };
    }
}
