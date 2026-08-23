using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public static class GameInfoHolder
    {
        private static List<GameInfo> _gameInfoList;
        private static int _currentIdx = 0;
        private static int? _pendingIdx;

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
            _pendingIdx = null;
        }

        public static void SetGameInfoList(List<GameInfo> gameInfoList)
        {
            _gameInfoList = gameInfoList;
            _currentIdx = 0;
            _pendingIdx = null;
        }

        public static bool HasMoreGameInfos()
        {
            return _currentIdx < _gameInfoList.Count - 1;
        }

        public static void ToNext()
        {
            _pendingIdx = _currentIdx + 1;
        }

        public static void CommitPendingGameInfoChange()
        {
            if (!_pendingIdx.HasValue)
            {
                return;
            }

            int pendingIdx = _pendingIdx.Value;
            _pendingIdx = null;
            SetCurrentIdx(pendingIdx);
        }

        public static void SetCurrentIdx(int idx)
        {
            _pendingIdx = null;
            _currentIdx = idx;
            if (_currentIdx >= _gameInfoList.Count)
            {
                Debug.LogError("current index exceeded max gameInfo count");
            }
        }
    }
}
