namespace GamePlay
{
    public static class GameInfoManager
    {
        private static GameInfo _gameInfo;

        public static GameInfo GetGameInfo()
        {
            return _gameInfo;
        }

        public static void Initialize(GameInfo gameInfo)
        {
            _gameInfo = gameInfo;
        }
    }
}
