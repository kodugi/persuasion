using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public static class GameInfoHolder
    {
        private static List<GameInfo> _gameInfoList;
        private static int _currentIdx = 0;

        public static GameInfo GetCurrentGameInfo()
        {
            return _gameInfoList[_currentIdx];
        }

        public static List<GameInfo> GetGameInfoList()
        {
            return _gameInfoList;
        }

        public static void SetGameInfo(GameInfo gameInfo)
        {
            _gameInfoList = new List<GameInfo>() { gameInfo };
            _currentIdx = 0;
        }

        public static void SetGameInfoList(List<GameInfo> gameInfoList)
        {
            _gameInfoList = gameInfoList;
            _currentIdx = 0;
        }

        public static bool HasMoreGameInfos()
        {
            return _currentIdx < _gameInfoList.Count - 1;
        }

        public static void ToNext()
        {
            SetCurrentIdx(_currentIdx + 1);
        }

        public static void SetCurrentIdx(int idx)
        {
            _currentIdx = idx;
            if (_currentIdx >= _gameInfoList.Count)
            {
                Debug.LogError("current index exceeded max gameInfo count");
            }
        }
    }
}
