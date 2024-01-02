using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
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
    private static Texture _nodeDetachedTexture;
    public static Texture nodeDetachedTexture
    {
        get
        {
            if (_nodeDetachedTexture == null)
            {
                _nodeDetachedTexture = Resources.Load<Texture>("ObbyNodeDetachedIcon");
            }
            return _nodeDetachedTexture;
        }
    }
    private static Texture _startTextTexture;
    public static Texture startTextTexture
    {
        get
        {
            if (_startTextTexture == null)
            {
                _startTextTexture = Resources.Load<Texture>("ObbyStartText");
            }
            return _startTextTexture;
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
    private static Texture _nodeConfigErrorTexture;
    public static Texture nodeConfigErrorTexture
    {
        get
        {
            if (_nodeConfigErrorTexture == null)
            {
                _nodeConfigErrorTexture = Resources.Load<Texture>("NodeConfigError");
            }
            return _nodeConfigErrorTexture;
        }
    }

    public static void DrawCourseConnections(ObbyCourse obbyManager, SceneView sceneView)
    {
        for (int i = 1; i < obbyManager.nodes.Length; i++)
        {
            Handles.color = ObbySettings.editorColor;
            Handles.DrawLine(NodeTransform(obbyManager.nodes[i - 1]).position, NodeTransform(obbyManager.nodes[i]).position, 2f);

            Handles.color = ObbySettings.editorColor;
        }
    }

    public static void DrawCourseFlags(ObbyCourse obbyManager, SceneView sceneView)
    {
        for (int i = 0; i < obbyManager.nodes.Length; i++)
        {
            ObbyNode node = obbyManager.nodes[i];
            if (node == null)
            {
                continue;
            }
            if (i == obbyManager.nodes.Length - 1)
            {
                DrawNodeFlag(node, endTexture, false);
            }
            else
            {
                DrawNodeFlag(node, nodeTexture, i == 0);
            }
        }
    }

    public static void DrawNodeFlag(ObbyNode node, Texture texture2D, bool isFirst)
    {
        float flagHeight = 1.4f;

        Handles.color = handleBlack;
        Vector3 labelPos = NodeTransform(node).position + Vector3.up * flagHeight * HandleUtility.GetHandleSize(NodeTransform(node).position);
        Handles.DrawLine(NodeTransform(node).position, labelPos, 3f);
        Handles.Label(labelPos, new GUIContent(texture2D), new GUIStyle { fixedWidth = 32f, fixedHeight = 32f, alignment = TextAnchor.MiddleCenter });
        Handles.color = Color.clear;
        float handleSize = HandleUtility.GetHandleSize(NodeTransform(node).position) * .2f;
        if (Handles.Button(labelPos, SceneView.currentDrawingSceneView.camera.transform.rotation, handleSize, handleSize, Handles.CircleHandleCap))
        {
            Selection.activeGameObject = node.gameObject;
        }

        if (isFirst)
        {
            //draw a (start) above the first node
            Vector3 startLabelPos = NodeTransform(node).position + Vector3.up * (flagHeight + .4f) * HandleUtility.GetHandleSize(NodeTransform(node).position);
            Handles.Label(startLabelPos, new GUIContent(startTextTexture), new GUIStyle { fixedWidth = 64f, fixedHeight = 22f, alignment = TextAnchor.MiddleCenter });
        }

        if (node.nodePlatform == null || node.target == null)
        {
            //draw a warning icon if the node is missing a platform or target
            Vector3 errorLabelPos = NodeTransform(node).position + Vector3.up * (flagHeight - .2f) * HandleUtility.GetHandleSize(NodeTransform(node).position);
            Handles.Label(errorLabelPos, new GUIContent(nodeConfigErrorTexture), new GUIStyle { fixedWidth = 16f, fixedHeight = 16f, alignment = TextAnchor.MiddleCenter });
        }
    }

    private static Transform NodeTransform(ObbyNode node)
    {
        return node.nodePlatform == null ? node.transform : node.nodePlatform.transform;
    }

    public static void DrawWaypointPaths(ObbyCourse obbyManager)
    {
        foreach (ObbyNode node in obbyManager.nodes) 
        {
            foreach (ObbyWaypointCarousel carousel in node.GetComponentsInChildren<ObbyWaypointCarousel>()) {
                Handles.color = ObbySettings.editorColor;
                for (int i = 1; i < carousel.waypoints.Count; i++) 
                {
                    Handles.DrawLine(carousel.waypoints[i - 1].position, carousel.waypoints[i].position, 1f);
                    Handles.color = ObbySettings.editorColor;
                }
            }
        }
    }

    public static void DrawCourseBounds(ObbyCourse course, SceneView sceneView)
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
            if (node.target != null)
            {
                bounds[i].Encapsulate(node.target.transform.position);
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
