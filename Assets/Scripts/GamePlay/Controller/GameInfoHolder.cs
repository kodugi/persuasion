namespace GamePlay
{
    public static class GameInfoHolder
    {
        private static GameInfo _gameInfo;

        public static GameInfo GetGameInfo()
        {
            return _gameInfo;
        }

        public static void SetGameInfo(GameInfo gameInfo)
        {
            _gameInfo = gameInfo;
        }
    }
}
