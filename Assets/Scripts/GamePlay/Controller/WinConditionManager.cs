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
        private TutorialController _tutorialController;
        private bool _isGameEnded;
        private bool _isResetting;
        
        public void Initialize()
        {
            _boardController = BoardController.Instance;
            _suspicionManager = SuspicionManager.Instance;
            _gameStateManager = GameStateManager.Instance;
            _turnManager = TurnManager.Instance;
            _tutorialController = TutorialController.Instance;
            _isGameEnded = false;

            _boardController.RaiseCellPlacementEvent += HandleCellPlacementEvent;
            _suspicionManager.RaiseSetSuspicionEvent += HandleSetSuspicionEvent;
            _turnManager.RaiseSetTurnEvent += HandleSetTurnEvent;
            _tutorialController.RaiseSetTutorialStateEvent += HandleSetTutorialStateEvent;
        }

        public void BeginReset()
        {
            _isResetting = true;
        }

        public void EndReset()
        {
            _isGameEnded = false;
            _isResetting = false;
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
            if (_isGameEnded || _isResetting)
            {
                return;
            }

            if (_turnManager.GetCurrentTurn() >= GameInfoHolder.GetCurrentGameInfo().GetMaxTurns())
            {
                Lose();
                return;
            }

            if (_suspicionManager.GetCurrentSuspicion() > _suspicionManager.GetMaxSuspicion())
            {
                switch (GameInfoHolder.GetCurrentGameInfo().GetMapType())
                {
                    case GameInfo.MapType.Dream1:
                    case GameInfo.MapType.Dream2:
                    case GameInfo.MapType.Dream3:
                    case GameInfo.MapType.Dream4:
                        LoseDream();
                        return;
                }
                Lose();
                return;
            }

            if(_boardController.GetConvertedBlackCellCount() >= GameInfoHolder.GetCurrentGameInfo().GetTargetNumber())
            {
                Win();
            }
        }

        private void HandleSetTutorialStateEvent(object sender, SetTutorialStateEventArgs e)
        {
            if (e.CurrentState == TutorialState.Dream2)
            {
                _isGameEnded = true;
                _gameStateManager.SetGameState(GameState.Lost);
            }
        }

        private void Lose()
        {
            // TODO: 패배 판정
            _isGameEnded = true;
            _gameStateManager.SetGameState(GameState.Lost);
        }

        private void Win()
        {
            // TODO: 승리 판정
            _isGameEnded = true;
            _gameStateManager.SetGameState(GameState.Won);
        }

        private void LoseDream()
        {
            _isGameEnded = true;
            GameManager.Instance.QueueResetGame();
        }
    }
}
