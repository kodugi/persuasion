using SingletonUtils;
using GamePlay;

namespace MapEditor
{
    public class BoardController: Singleton<BoardController>
    {
        private Board _board;
        
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
        }

        public void HandleCellPlacementInput()
        {
            
        }
    }
}