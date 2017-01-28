using Assets.Scripts.Base;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UnityBase
{
    public static class UnityHelper
    {
        private static readonly Vector3 sr_defaultLocation = Vector3.zero;

        public static T Instantiate<T>() where T : MonoBehaviour
        {
            return Instantiate<T>(sr_defaultLocation, GetResourceName(typeof(T)));
        }

        public static T Instantiate<T>(string resourceName) where T : MonoBehaviour
        {
            return Instantiate<T>(sr_defaultLocation, resourceName);
        }

        public static T Instantiate<T>(Vector3 location) where T : MonoBehaviour
        {
            return Instantiate<T>(location, GetResourceName(typeof(T)));
        }

        public static T Instantiate<T>(Vector3 location, string resourceName) where T : MonoBehaviour
        {
            return ((GameObject)GameObject.Instantiate(Resources.Load(resourceName), location, Quaternion.identity)).GetComponent<T>();
        }

        /// <summary>
        /// Receives a list of n items and m targets, activates the first m items and deactivates the rest, 
        /// and operates on the first m items and targets with a given action
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <typeparam name="TTarget"></typeparam>
        /// <param name="items"></param>
        /// <param name="targets"></param>
        /// <param name="action"></param>
        public static void SetFunctionalityForFirstItems<TObject, TTarget>(List<TObject> items, List<TTarget> targets, Action<TObject, TTarget> action) where TObject : MonoBehaviour
        {
            Assert.EqualOrGreater(items.Count, targets.Count);

            int index = 0;
            foreach (var item in items)
            {
                if (index < targets.Count)
                {
                    item.gameObject.SetActive(true);
                    action(item, targets[index]);
                }
                else
                {
                    item.gameObject.SetActive(false);
                }

                index++;
            }
        }

        private static string GetResourceName(Type t)
        {
            return t.Name.Replace("Script", string.Empty);
        }
    }
}