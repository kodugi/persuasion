using GamePlay;
using SingletonUtils;
using UnityEngine;

namespace MapEditor
{
    public class GameInfoController: Singleton<GameInfoController>
    {
        private int _maxTurns;
        private int _targetNumber;
        private BoardController _boardController;

        public void Initialize()
        {
            _boardController = BoardController.Instance;
            _maxTurns = 10;
            _targetNumber = 5;
        }
        
        public GameInfo AssembleGameInfo()
        {
            GameInfo gameInfo = ScriptableObject.CreateInstance<GameInfo>();
            gameInfo.Initialize(_boardController.GetBoard().GetBoard(), _maxTurns, _targetNumber);
            return gameInfo;
        }
        
        public int GetMaxTurns()
        {
            return _maxTurns;
        }

        public int GetTargetNumber()
        {
            return _targetNumber;
        }

        public void SetMaxTurns(int maxTurns)
        {
            _maxTurns = maxTurns;
        }

        public void SetTargetNumber(int targetNumber)
        {
            _targetNumber = targetNumber;
        }
    }
}