using System;
using UnityEngine;
using SingletonUtils;

namespace GamePlay
{
    public class WinConditionManager: Singleton<WinConditionManager>
    {
        private BoardController _boardController;
        private SuspicionManager _suspicionManager;
        private GameStateManager _gameStateManager;
        private TurnManager _turnManager;
        private bool _isGameEnded;
        
        public void Initialize()
        {
            _boardController = BoardController.Instance;
            _suspicionManager = SuspicionManager.Instance;
            _gameStateManager = GameStateManager.Instance;
            _turnManager = TurnManager.Instance;
            _isGameEnded = false;

            _boardController.RaiseCellPlacementEvent += HandleCellPlacementEvent;
            _suspicionManager.RaiseSetSuspicionEvent += HandleSetSuspicionEvent;
            _turnManager.RaiseSetTurnEvent += HandleSetTurnEvent;
        }

        private void HandleCellPlacementEvent(object sender, EventArgs e)
        {
            EvaluateGameResult();
        }

        private void HandleSetSuspicionEvent(object sender, SetSuspicionEventArgs e)
        {
            EvaluateGameResult();
        }

        private void HandleSetTurnEvent(object sender, SetTurnEventArgs e)
        {
            EvaluateGameResult();
        }

        private void EvaluateGameResult()
        {
            if (_isGameEnded)
            {
                return;
            }

            if (_suspicionManager.GetCurrentSuspicion() > _suspicionManager.GetMaxSuspicion() ||
                _turnManager.GetCurrentTurn() >= GameInfoHolder.GetGameInfo().GetMaxTurns())
            {
                // TODO: 패배 판정
                _isGameEnded = true;
                Debug.Log("설득 실패!");
                _gameStateManager.SetGameState(GameState.Lost);
                //GameOverPopupView.Instance.ShowPopup(true, "설득 실패", "설득에 실패했습니다");
                return;
            }

            if(_boardController.GetConvertedBlackCellCount() >= GameInfoHolder.GetGameInfo().GetTargetNumber())
            {
                // TODO: 승리 판정
                _isGameEnded = true;
                Debug.Log("설득 성공!");
                //GameOverPopupView.Instance.ShowPopup(true, "설득 성공", "설득에 성공했습니다");
                _gameStateManager.SetGameState(GameState.Won);
            }
        }
    }
}
