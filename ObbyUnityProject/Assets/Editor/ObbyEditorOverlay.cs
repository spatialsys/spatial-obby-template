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
        private static string SHOW_BOUNDS_KEY = "SpatialObby_ShowBounds";
        private static bool SHOW_BOUNDS_DEFAULT = false;
        private static string SHOW_FLAGS_KEY = "SpatialObby_ShowFlags";
        private static bool SHOW_FLAGS_DEFAULT = true;

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
        }

        private void DrawHandles(SceneView sceneView)
        {
            List<ObbyCourse> obbyCourses = GameObject.FindObjectsOfType<ObbyCourse>().ToList();

            foreach (ObbyCourse obbyCourse in obbyCourses)
            {
                if (EditorPrefs.GetBool(SHOW_FLAGS_KEY, SHOW_FLAGS_DEFAULT))
                {
                    ObbyHandles.DrawFlags(obbyCourse, sceneView);
                }

                if (EditorPrefs.GetBool(SHOW_CONNECTIONS_KEY, SHOW_CONNECTIONS_DEFAULT))
                {
                    ObbyHandles.DrawConnections(obbyCourse, sceneView);
                }

                if (EditorPrefs.GetBool(SHOW_BOUNDS_KEY, SHOW_BOUNDS_DEFAULT))
                {
                    ObbyHandles.DrawBounds(obbyCourse, sceneView);
                }
            }
        }
    }
}
