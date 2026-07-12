using GamePlay;

namespace MapEditor.Model
{
    public static class EditorInfoHolder
    {
        private static GameInfo _gameInfo;

        public static void SetGameInfo(GameInfo gameInfo)
        {
            _gameInfo = gameInfo;
        }

        public static GameInfo GetGameInfo()
        {
            return _gameInfo;
        }
    }
}