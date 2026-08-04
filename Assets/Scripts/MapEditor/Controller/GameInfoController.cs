using SingletonUtils;
using GamePlay;
using MapEditor.Model;
using UnityEngine;

namespace MapEditor
{
    public class GameInfoController: Singleton<GameInfoController>
    {
        private BoardController _boardController;
        private GameSettingsController _gameSettingsController;

        public void Initialize()
        {
            _boardController = BoardController.Instance;
            _gameSettingsController = GameSettingsController.Instance;
        }
        
        public GameInfo AssembleGameInfo()
        {
            GameInfo gameInfo = ScriptableObject.CreateInstance<GameInfo>();
            gameInfo.Initialize(_boardController.GetBoard().GetBoard(), _gameSettingsController.GetMaxTurns(), _gameSettingsController.GetTargetNumber());
            return gameInfo;
        }

        public void SetGameInfo(GameInfo gameInfo)
        {
            if (gameInfo == null)
            {
                Debug.LogWarning("GameInfoController ignored null GameInfo.");
                return;
            }

            _boardController.RefreshBoard(gameInfo.GetBoard());
            _gameSettingsController.Refresh(gameInfo.GetMaxTurns(), gameInfo.GetTargetNumber());
            EditorInfoHolder.SetGameInfo(gameInfo);
        }
    }
}
