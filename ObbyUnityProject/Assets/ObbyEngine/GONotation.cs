using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpatialSys.GoNotation
{
    public static class GoNotation
    {
        /// <summary>
        /// Creates a new gameobject
        /// </summary>
        public static GameObject GO(string name, Action<GameObject> action = null)
        {
            GameObject go = new GameObject(name);
            action?.Invoke(go);
            return go;
        }
        /// <summary>
        /// Creates a child gameobject
        /// </summary>
        public static GameObject GO(this GameObject parent, string name = "Child", Action<GameObject> action = null)
        {
            GameObject go = new GameObject(name);
            go.transform.parent = parent.transform;
            action?.Invoke(go);
            return go;
        }
        /// <summary>
        /// Creates a child gameobject
        /// </summary>
        public static GameObject GO(this GameObject parent, Action<GameObject> action = null)
        {
            GameObject go = new GameObject();
            go.transform.parent = parent.transform;
            action?.Invoke(go);
            return go;
        }

        /// <summary>
        /// Set the passed gameobject as a child of this gameobject
        /// </summary>
        public static GameObject GO(this GameObject parent, GameObject child)
        {
            child.transform.parent = parent.transform;
            return child;
        }

        /// <summary>
        /// Add a component to the gameobject
        /// </summary>
        public static GameObject C<T>(this GameObject go, Action<T> action = null) where T : Component
        {
            T component = go.AddComponent<T>();
            action?.Invoke(component);
            return go;
        }

        /// <summary>
        /// Add a component to the gameobject
        /// </summary>
        public static GameObject C<T>(this GameObject go, out T componentRef, Action<T> action = null) where T : Component
        {
            T component = go.AddComponent<T>();
            action?.Invoke(component);
            componentRef = component;
            return go;
        }

        /// <summary>
        /// Get the parent of the gameobject as a gameobject
        /// </summary>
        public static GameObject UP(this GameObject go)
        {
            return go.transform.parent.gameObject;
        }

        /// <summary>
        /// Get the root of the gameobject as a gameobject
        /// </summary>
        public static GameObject Root(this GameObject go)
        {
            return go.transform.root.gameObject;
        }

        /// <summary>
        /// Shorthand to add a rect transform which is commonly useful while building UI
        /// </summary>
        /// <remarks>
        /// Note that this is really only needed if you are trying to access the rect during the GO lambda. At that point the Rect
        /// has not been automatically added to the gameObject. If you don't need to directly ref the Rect for any reason you don't need to use this since
        /// rects are added automatically when needed.
        /// </remarks>
        public static GameObject Rect(this GameObject go, Action<RectTransform> action = null)
        {
            RectTransform rect = go.AddComponent<RectTransform>();
            action?.Invoke(rect);
            return go;
        }

        private static GameObject Example()
        {
            return
            GO("MeshExample").
            C<MeshFilter>((filter) =>
            {
                filter.mesh = new Mesh();
            }).
            C<MeshRenderer>((renderer) =>
            {
                renderer.material = new Material(Shader.Find("Standard"));
            });
        }
    }
}
