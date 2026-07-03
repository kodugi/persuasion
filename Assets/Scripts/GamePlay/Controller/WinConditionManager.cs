using System;
using UnityEngine;

namespace GamePlay
{
    public class WinConditionManager: Singleton<WinConditionManager>
    {
        private BoardController _boardController;
        private SuspicionManager _suspicionManager;
        private TurnManager _turnManager;
        private bool _isGameEnded;
        public void Initialize()
        {
            _boardController = BoardController.Instance;
            _suspicionManager = SuspicionManager.Instance;
            _turnManager = TurnManager.Instance;
            _isGameEnded = false;

            _boardController.RaiseCellPlacementEvent += HandleCellPlacementEvent;
            _suspicionManager.RaiseSetSuspicionEvent += HandleSetSuspicionEvent;
        }

        private void HandleCellPlacementEvent(object sender, EventArgs e)
        {
            EvaluateGameResult();
        }

        private void HandleSetSuspicionEvent(object sender, SetSuspicionEventArgs e)
        {
            EvaluateGameResult();
        }

        private void EvaluateGameResult()
        {
            if (_isGameEnded)
            {
                return;
            }

            if (_suspicionManager.GetCurrentSuspicion() > _suspicionManager.GetMaxSuspicion())
            {
                // TODO: 패배 판정
                _isGameEnded = true;
                Debug.Log("설득 실패!");
                _turnManager.SetTurnState(TurnState.GameLose);
                return;
            }

            if(_boardController.GetConvertedBlackCellCount() >= GameInfoManager.GetGameInfo().GetTargetNumber())
            {
                // TODO: 승리 판정
                _isGameEnded = true;
                Debug.Log("설득 성공!");
                _turnManager.SetTurnState(TurnState.GameWin);
            }
        }
    }
}
