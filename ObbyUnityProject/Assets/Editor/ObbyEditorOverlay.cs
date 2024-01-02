using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.SceneManagement;
using System.Linq;

namespace SpatialSys.Obby.Editor
{
    [Overlay(typeof(SceneView), "Obby Editor", true)]
    public class ObbyEditorOverlay : Overlay, ITransientOverlay
    {
        public bool visible => true;
        private bool initEvents = false;

        private int tab = 0;

        //settings
        private static string SHOW_CONNECTIONS_KEY = "SpatialObby_ShowConnections";
        private static bool SHOW_CONNECTIONS_DEFAULT = true;
        private static string SHOW_WAYPOINT_PATHS_KEY = "SpatialObby_ShowWaypointPaths";
        private static bool SHOW_WAYPOINT_PATHS_DEFAULT = true;
        private static string SHOW_BOUNDS_KEY = "SpatialObby_ShowBounds";
        private static bool SHOW_BOUNDS_DEFAULT = false;
        private static string SHOW_FLAGS_KEY = "SpatialObby_ShowFlags";
        private static bool SHOW_FLAGS_DEFAULT = true;
        private static string AUTO_ARRANGE_COURSES = "SpatialObby_AutoArrangeCourses";
        private static bool AUTO_ARRANGE_COURSES_DEFAULT = true;
        private static string AUTO_ARRANGE_COURSE_SPACING = "SpatialObby_AutoArrangeCourseSpacing";
        private static float AUTO_ARRANGE_COURSE_SPACING_DEFAULT = .25f;


        public override VisualElement CreatePanelContent()
        {
            if (!initEvents)
            {
                SceneView.duringSceneGui += DrawHandles;
                initEvents = true;
            }

            return new IMGUIContainer(DrawUI);
        }

        private void DrawUI()
        {

            tab = GUILayout.Toolbar(tab, new string[] { "Edit", "Settings" });
            switch (tab)
            {
                default:
                case 0:
                    break;
                case 1:
                    DrawSettings();
                    break;
            }
        }

        private void DrawSettings()
        {
            bool connection = EditorPrefs.GetBool(SHOW_CONNECTIONS_KEY, SHOW_CONNECTIONS_DEFAULT);
            if (EditorGUILayout.Toggle("Show Node Connections", connection) != connection)
            {
                EditorPrefs.SetBool(SHOW_CONNECTIONS_KEY, !connection);
            }

            bool waypointPaths = EditorPrefs.GetBool(SHOW_WAYPOINT_PATHS_KEY, SHOW_WAYPOINT_PATHS_DEFAULT);
            if (EditorGUILayout.Toggle("Show Waypoint Paths", waypointPaths) != waypointPaths)
            {
                EditorPrefs.SetBool(SHOW_WAYPOINT_PATHS_KEY, !waypointPaths);
            }

            bool bounds = EditorPrefs.GetBool(SHOW_BOUNDS_KEY, SHOW_BOUNDS_DEFAULT);
            if (EditorGUILayout.Toggle("Show Node Bounds", bounds) != bounds)
            {
                EditorPrefs.SetBool(SHOW_BOUNDS_KEY, !bounds);
            }

            bool flags = EditorPrefs.GetBool(SHOW_FLAGS_KEY, SHOW_FLAGS_DEFAULT);
            if (EditorGUILayout.Toggle("Show Node Flags", flags) != flags)
            {
                EditorPrefs.SetBool(SHOW_FLAGS_KEY, !flags);
            }

            bool autoArrange = EditorPrefs.GetBool(AUTO_ARRANGE_COURSES, AUTO_ARRANGE_COURSES_DEFAULT);
            if (EditorGUILayout.Toggle("Auto Arrange Courses", autoArrange) != autoArrange)
            {
                EditorPrefs.SetBool(AUTO_ARRANGE_COURSES, !autoArrange);
            }

            float autoArrangeSpacing = EditorPrefs.GetFloat(AUTO_ARRANGE_COURSE_SPACING, AUTO_ARRANGE_COURSE_SPACING_DEFAULT);
            float newSpacing = EditorGUILayout.FloatField("Auto Arrange Course Spacing", autoArrangeSpacing);
            if (newSpacing != autoArrangeSpacing)
            {
                EditorPrefs.SetFloat(AUTO_ARRANGE_COURSE_SPACING, newSpacing);
            }

            if (GUILayout.Button("Scramble"))
            {
                List<ObbyCourse> obbyCourses = GameObject.FindObjectsOfType<ObbyCourse>().ToList();
                //randomize the order of the nodes in each course, keeping the first and last nodes in place.
                foreach (ObbyCourse obbyCourse in obbyCourses)
                {
                    if (obbyCourse.nodes.Length < 3)
                    {
                        continue;
                    }
                    Undo.RecordObject(obbyCourse, "shuffle");
                    List<ObbyNode> nodes = obbyCourse.nodes.ToList();
                    ObbyNode firstNode = nodes[0];
                    ObbyNode lastNode = nodes[nodes.Count - 1];
                    nodes.RemoveAt(0);
                    nodes.RemoveAt(nodes.Count - 1);
                    nodes = nodes.OrderBy(x => Random.value).ToList();
                    nodes.Insert(0, firstNode);
                    nodes.Add(lastNode);
                    obbyCourse.nodes = nodes.ToArray();
                }
            }
        }

        private void DrawHandles(SceneView sceneView)
        {
            List<ObbyCourse> obbyCourses = GameObject.FindObjectsOfType<ObbyCourse>().ToList();

            foreach (ObbyCourse obbyCourse in obbyCourses)
            {
                if (EditorPrefs.GetBool(SHOW_FLAGS_KEY, SHOW_FLAGS_DEFAULT))
                {
                    ObbyHandles.DrawCourseFlags(obbyCourse, sceneView);
                }

                if (EditorPrefs.GetBool(SHOW_CONNECTIONS_KEY, SHOW_CONNECTIONS_DEFAULT))
                {
                    ObbyHandles.DrawCourseConnections(obbyCourse, sceneView);
                }

                if (EditorPrefs.GetBool(SHOW_BOUNDS_KEY, SHOW_BOUNDS_DEFAULT))
                {
                    ObbyHandles.DrawCourseBounds(obbyCourse, sceneView);
                }

                if (EditorPrefs.GetBool(AUTO_ARRANGE_COURSES, AUTO_ARRANGE_COURSES_DEFAULT))
                {
                    AutoPositionCourse(obbyCourse);
                }

                if (EditorPrefs.GetBool(SHOW_WAYPOINT_PATHS_KEY, SHOW_WAYPOINT_PATHS_DEFAULT))
                ObbyHandles.DrawWaypointPaths(obbyCourse);
            }

            List<ObbyNode> obbyNodes = GameObject.FindObjectsOfType<ObbyNode>().ToList();
            //Find all nodes that are not part of a course
            foreach (ObbyNode obbyNode in obbyNodes)
            {
                if (obbyCourses.Any(c => c.nodes.Contains(obbyNode)))
                {
                    continue;
                }
                ObbyHandles.DrawNodeFlag(obbyNode, ObbyHandles.nodeDetachedTexture, false);
            }
        }

        private void AutoPositionCourse(ObbyCourse course)
        {
            for (int i = 0; i < course.nodes.Length; i++)
            {
                ObbyNode node = course.nodes[i];
                if (node == null || node.target == null || node.nodePlatform == null || i == course.nodes.Length - 1)
                {
                    continue;
                }
                ObbyNode nextNode = course.nodes[i + 1];
                if (nextNode == null || nextNode.nodePlatform == null)
                {
                    continue;
                }

                Vector3 targetPos = node.target.transform.position;
                Quaternion targetRot = node.target.transform.rotation;

                Bounds platformBounds = nextNode.nodePlatform.GetLocalBounds();
                float spacing = EditorPrefs.GetFloat(AUTO_ARRANGE_COURSE_SPACING, AUTO_ARRANGE_COURSE_SPACING_DEFAULT);
                nextNode.transform.position = targetPos + targetRot * new Vector3(0f, 0f, platformBounds.extents.z + spacing);
                nextNode.transform.rotation = targetRot;
            }
        }
    }
}
