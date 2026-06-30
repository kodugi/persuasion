using UnityEngine;

namespace GamePlay
{
    public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviourSingleton<T>
    {
        public static T Instance { get; private set; }

        protected bool IsSingletonInstance
        {
            get { return ReferenceEquals(Instance, this); }
        }

        protected virtual void Awake()
        {
            if (Instance != null && !ReferenceEquals(Instance, this))
            {
                Debug.LogWarning("Multiple " + typeof(T).Name + " instances were found. The first instance will be used.", this);
                return;
            }

            Instance = (T)this;
        }

        protected virtual void OnDestroy()
        {
            if (ReferenceEquals(Instance, this))
            {
                Instance = null;
            }
        }
    }
}
