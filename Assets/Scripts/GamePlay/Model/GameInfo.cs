namespace GamePlay
{
    public class GameInfo
    {
        private int _width{ get; }
        private int _height{ get; }
        private Cell[,] _board{ get; }

        public int GetWidth()
        {
            return _width;
        }

        public int GetHeight()
        {
            return _height;
        }

        public Cell[,] GetBoard()
        {
            return _board;
        }
        private int _maxTurns{ get; }
        private int _targetNumber{ get; }

        public GameInfo(int width, int height, Cell[,] board, int maxTurns, int targetNumber)
        {
            _width = width;
            _height = height;
            _board = board;
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
    }
}
