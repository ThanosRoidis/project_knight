using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;


namespace Shared
{
    public static class GenericUtils
    {
       // Get all children of a transform
       public static IEnumerable<Transform> GetAllChildren(this Transform transform)
        {
            return transform.Cast<Transform>();
        }

        public static void CopySpriteInfo(SpriteRenderer from, SpriteRenderer to)
        {
            to.sprite = from.sprite;
            to.sortingLayerID = from.sortingLayerID;
            to.sortingOrder = from.sortingOrder;
        }

        public static IEnumerable<(int index, T value)> Enumerate<T>(IEnumerable<T> coll)
            => coll.Select((i, val) => (val, i));

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            T[] elements = source.ToArray();
            for (int i = elements.Length - 1; i >= 0; i--) {
                // Swap element "i" with a random earlier element it (or itself)
                // ... except we don't really need to swap it fully, as we can
                // return it immediately, and afterwards it's irrelevant.
                int swapIndex = UnityEngine.Random.Range(0, i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }

        public static void DestroyDelayed(GameObject gameObjectToDestroy, float delay)
        {
            Destroyer destroyer = gameObjectToDestroy.AddComponent<Destroyer>();
            destroyer.delay = delay;
        }

        public static IEnumerator ExecuteDelayed(Action action, float delay, bool realtime = false)
        {
            if (delay > 0) {
                if (realtime)
                    yield return new WaitForSecondsRealtime(delay);
                else
                    yield return new WaitForSeconds(delay);
            } else {
                yield return null;
            }
            action();
        }

        /// <summary>
        /// Return a copy of a Component (same values)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="comp"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static T GetCopyOf<T>(this Component comp, T other) where T : Component
        {
            Type type = comp.GetType();
            if (type != other.GetType()) return null; // type mis-match
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos) {
                if (pinfo.CanWrite) {
                    try {
                        pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                    } catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }
            FieldInfo[] finfos = type.GetFields(flags);
            foreach (var finfo in finfos) {
                finfo.SetValue(comp, finfo.GetValue(other));
            }
            return comp as T;
        }


        /// <summary>
        /// Extension method for GameObject that copies another Component and adds it to the GameObject
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="go"></param>
        /// <param name="toAdd"></param>
        /// <returns></returns>
        public static T AddComponent<T>(this GameObject go, T toAdd) where T : Component
        {
            return go.AddComponent<T>().GetCopyOf(toAdd) as T;
        }


        /// <summary>
        /// Return a copy of a Component (same values)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="behaviour"></param>
        /// <param name="other"></param>
        /// <returns></returns>
        public static void CopyValues<T>(this MonoBehaviour behaviour, T other) where T : MonoBehaviour
        {
            Type type = behaviour.GetType();
            if (type != other.GetType()) return; // type mis-match
            BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
            PropertyInfo[] pinfos = type.GetProperties(flags);
            foreach (var pinfo in pinfos) {
                if (pinfo.CanWrite) {
                    try {
                        pinfo.SetValue(behaviour, pinfo.GetValue(other, null), null);
                    } catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
                }
            }
            FieldInfo[] finfos = type.GetFields(flags);
            foreach (var finfo in finfos) {
                finfo.SetValue(behaviour, finfo.GetValue(other));
            }
        }

        public static bool TimeElapsed(float startTime, float duration, float timescale = 1f)
        {
            return Time.time > startTime + duration / timescale;
        }


        public static void RemoveByIndexes<T>(List<T> col, List<int> idxs)
        {
            foreach (int indice in idxs.OrderByDescending(v => v)) {
                col.RemoveAt(indice);
            }
        }

    }
}