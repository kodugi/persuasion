using System;

namespace GamePlay
{
    public abstract class Singleton<T> where T : Singleton<T>
    {
        public static T Instance { get; private set; }

        protected Singleton()
        {
            if (Instance != null)
            {
                throw new Exception(typeof(T).Name + " instance already exists!");
            }

            Instance = (T)this;
        }
    }
}
