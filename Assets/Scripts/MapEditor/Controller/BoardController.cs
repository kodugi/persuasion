using System;
using SingletonUtils;
using GamePlay;

namespace MapEditor
{
    public class BoardController: Singleton<BoardController>
    {
        private Board _board;
        private CellSelectionManager _cellSelectionManager;
        
        public void Initialize(int width, int height)
        {
            _board = new Board(width, height);
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    _board.SetCell(new Vector2Int(i, j), new EmptyCell(0, new Vector2Int(i, j)));
                }
            }
            
            _cellSelectionManager = CellSelectionManager.Instance;
        }

        public void HandleCellPlacementInput(Vector2Int coord)
        {
            _board.SetCell(coord, (Cell)Activator.CreateInstance(_cellSelectionManager.GetCurrentCellType()));
            
        }
    }
}