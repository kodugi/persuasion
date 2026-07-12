using UnityEngine;
using GamePlay;
using MapEditor.Model;

namespace MapEditor
{
    public class GameManager: MonoBehaviour
    {
        private BoardController _boardController;
        private CellSelectionManager _cellSelectionManager;
        private GameSettingsController _gameSettingsController;
        private GameInfoController _gameInfoController;
        
        private void Start()
        {
            _boardController = new BoardController();
            _cellSelectionManager = new CellSelectionManager();
            _gameSettingsController = new GameSettingsController();
            _gameInfoController = new GameInfoController();
            
            GameInfo gameInfo = EditorInfoHolder.GetGameInfo();
            
            _boardController.Initialize(5, 5);
            _cellSelectionManager.Initialize();
            _gameSettingsController.Initialize(10, 5);
            _gameInfoController.Initialize();

            if (gameInfo == null)
            {
                EditorInfoHolder.SetGameInfo(_gameInfoController.AssembleGameInfo());
            }
            else
            {
                _gameInfoController.SetGameInfo(gameInfo);
            }
        }
    }
}