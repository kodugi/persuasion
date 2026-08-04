using System;
using UnityEngine;

namespace SingletonUtils
{
    public abstract class Singleton<T> where T : Singleton<T>
    {
        public static T Instance { get; private set; }

        protected Singleton()
        {
            if (Instance != null)
            {
                Debug.LogWarning(typeof(T).Name + " instance already exists!");
            }

            Instance = (T)this;
        }
    }
}
