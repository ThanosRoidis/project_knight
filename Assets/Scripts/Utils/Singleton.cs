using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Shared
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        static T _instance;

        public static T Instance
        {
            get {
                if (_instance == null) _instance = (T)FindObjectOfType(typeof(T));
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (_instance == null) _instance = this as T;
            else if (_instance != this) Destroy(this);
        }

        public bool Active
        {
            get => gameObject.activeSelf;
            set => gameObject.SetActive(value);
        }
    }
}