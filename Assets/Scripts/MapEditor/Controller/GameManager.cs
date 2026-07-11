using UnityEngine;

namespace MapEditor
{
    public class GameManager: MonoBehaviour
    {
        private BoardController _boardController;
        private CellSelectionManager _cellSelectionManager;
        private GameInfoController _gameInfoController;
        
        private void Awake()
        {
            _boardController = new BoardController();
            _cellSelectionManager = new CellSelectionManager();
            _gameInfoController = new GameInfoController();

            _boardController.Initialize(5, 5);
            _cellSelectionManager.Initialize();
            _gameInfoController.Initialize();
        }
    }
}