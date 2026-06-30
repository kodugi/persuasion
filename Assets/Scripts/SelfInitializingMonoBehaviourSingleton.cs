namespace GamePlay
{
    public abstract class SelfInitializingMonoBehaviourSingleton<T> : MonoBehaviourSingleton<T>
        where T : SelfInitializingMonoBehaviourSingleton<T>
    {
        private bool _isInitialized;

        protected bool IsInitialized
        {
            get { return _isInitialized; }
        }

        protected virtual void Start()
        {
            EnsureInitialized();
        }

        public void Initialize()
        {
            if (!IsSingletonInstance)
            {
                return;
            }

            _isInitialized = InitializeCore();
        }

        protected bool EnsureInitialized()
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            return _isInitialized;
        }

        protected void SetInitialized(bool isInitialized)
        {
            _isInitialized = isInitialized;
        }

        protected abstract bool InitializeCore();
    }
}
