namespace GamePlay
{
    public class Board
    {
        private Cell[,] _board;
        private int _width;
        private int _height;
    
        public Board(int width, int height)
        {
            _board = new Cell[width, height];
            _width = width;
            _height = height;
        }

        public Board(Cell[,] board)
        {
            _width = board.GetLength(0);
            _height = board.GetLength(1);
            _board = new Cell[_width, _height];

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _board[x, y] = CloneCell(board[x, y]);
                }
            }
        }

        public Cell[,] GetBoard()
        {
            return _board;
        }

        public int GetWidth()
        {
            return _width;
        }

        public int GetHeight()
        {
            return _height;
        }

        private static Cell CloneCell(Cell cell)
        {
            if (cell == null)
            {
                return null;
            }

            return (Cell)cell.Clone();
        }

        public Cell GetCell(Vector2Int coord)
        {
            return _board[coord.X, coord.Y];
        }

        public void SetCell(Vector2Int coord, Cell cell)
        {
            _board[coord.X, coord.Y] = cell;
        }

        public bool IsWithinBound(Vector2Int coord)
        {
            return coord.X >= 0 && coord.X < _width && coord.Y >= 0 && coord.Y < _height;
        }
    }
}
