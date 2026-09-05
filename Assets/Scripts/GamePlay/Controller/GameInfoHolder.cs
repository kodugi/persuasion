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
            TryGetCurrentGameInfo(out GameInfo gameInfo);
            return gameInfo;
        }

        public static bool TryGetCurrentGameInfo(out GameInfo gameInfo)
        {
            gameInfo = null;
            if (_gameInfoList == null ||
                _currentIdx < 0 ||
                _currentIdx >= _gameInfoList.Count)
            {
                return false;
            }

            gameInfo = _gameInfoList[_currentIdx];
            return gameInfo != null;
        }

        public static List<GameInfo> GetGameInfoList()
        {
            return _gameInfoList;
        }

        public static void SetGameInfo(GameInfo gameInfo)
        {
            _gameInfoList = gameInfo == null
                ? new List<GameInfo>()
                : new List<GameInfo> { gameInfo };
            _currentIdx = 0;
            _pendingIdx = null;
        }

        public static void SetGameInfoList(List<GameInfo> gameInfoList)
        {
            _gameInfoList = gameInfoList == null
                ? new List<GameInfo>()
                : gameInfoList.FindAll(gameInfo => gameInfo != null);
            _currentIdx = 0;
            _pendingIdx = null;
        }

        public static bool HasMoreGameInfos()
        {
            return _gameInfoList != null && _currentIdx < _gameInfoList.Count - 1;
        }

        public static void ToNext()
        {
            if (HasMoreGameInfos())
            {
                _pendingIdx = _currentIdx + 1;
            }
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
            if (_gameInfoList == null || idx < 0 || idx >= _gameInfoList.Count)
            {
                Debug.LogError("GameInfo index is outside the available range: " + idx);
                return;
            }

            _currentIdx = idx;
        }
    }
}
