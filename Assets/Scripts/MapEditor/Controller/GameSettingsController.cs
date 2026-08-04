using GamePlay;
using SingletonUtils;
using UnityEngine;

namespace MapEditor
{
    public class GameSettingsController: Singleton<GameSettingsController>
    {
        private int _maxTurns;
        private int _targetNumber;
        private BoardController _boardController;

        public void Initialize(int maxTurns, int targetNumber)
        {
            _boardController = BoardController.Instance;
            _maxTurns = maxTurns;
            _targetNumber = targetNumber;
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

        public void Refresh(int maxTurns, int targetNumber)
        {
            _maxTurns = maxTurns;
            _targetNumber = targetNumber;
            GameSettingsView.Instance.Refresh();
        }
    }
}