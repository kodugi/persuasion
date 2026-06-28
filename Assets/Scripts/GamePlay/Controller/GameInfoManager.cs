namespace GamePlay
{
    public class GameInfoManager : Singleton<GameInfoManager>
    {
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
