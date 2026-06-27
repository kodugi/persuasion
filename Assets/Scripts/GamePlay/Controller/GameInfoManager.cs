namespace GamePlay
{
    public class GameInfoManager
    {
        public static GameInfoManager Instance { get; private set; }

        public GameInfoManager()
        {
            if (Instance != null)
            {
                throw new System.Exception("GameInfoManager instance already exists!");
            }
            Instance = this;
        }

        private GameInfo _gameInfo;

        public GameInfo GetGameInfo()
        {
            return _gameInfo;
        }

        public void Initialize(GameInfo gameInfo)
        {
            _gameInfo = gameInfo;
        }
    }
}