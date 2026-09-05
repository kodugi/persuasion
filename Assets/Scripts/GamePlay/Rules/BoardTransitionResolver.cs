using System.Collections.Generic;
using Vector2Int = VectorUtils.Vector2Int;

namespace GamePlay
{
    /// <summary>
    /// Applies complete player and enemy transitions to a board.
    /// </summary>
    internal sealed class BoardTransitionResolver
    {
        private readonly BoardFlipResolver _flipResolver = new BoardFlipResolver();

        public List<CellChange> ApplyPlayerPlacement(
            Board board,
            CellChange placement,
            int currentTurn)
        {
            Queue<CellChange> pendingChanges = new Queue<CellChange>();
            bool[,] visited = new bool[board.GetWidth(), board.GetHeight()];

            EnqueueIfUnvisited(board, pendingChanges, visited, placement);
            SetCell(board, placement, currentTurn);

            foreach (CellChange weakCellChange in _flipResolver.FindAdjacentWeakCellFlips(board, placement.Coord))
            {
                EnqueueIfUnvisited(board, pendingChanges, visited, weakCellChange);
            }

            List<CellChange> appliedChanges = new List<CellChange>();
            while (pendingChanges.Count > 0)
            {
                CellChange currentChange = pendingChanges.Dequeue();
                appliedChanges.Add(currentChange);
                SetCell(board, currentChange, currentTurn);

                foreach (CellChange flip in _flipResolver.FindPlayerFlips(board, currentChange.Coord))
                {
                    EnqueueIfUnvisited(board, pendingChanges, visited, flip);
                }
            }

            return appliedChanges;
        }

        public List<CellChange> ApplyEnemyTurn(Board board, int currentTurn)
        {
            List<CellChange> pendingChanges = new List<CellChange>();
            for (int x = 0; x < board.GetWidth(); x++)
            {
                for (int y = 0; y < board.GetHeight(); y++)
                {
                    Vector2Int coord = new Vector2Int(x, y);
                    if (board.GetCell(coord) is BlackCell)
                    {
                        pendingChanges.AddRange(_flipResolver.FindEnemyFlips(board, coord));
                    }
                }
            }

            List<CellChange> appliedChanges = new List<CellChange>();
            foreach (CellChange change in pendingChanges)
            {
                if (board.GetCell(change.Coord).GetType() == change.CellType)
                {
                    continue;
                }

                SetCell(board, change, currentTurn);
                appliedChanges.Add(change);
            }

            return appliedChanges;
        }

        private static bool EnqueueIfUnvisited(
            Board board,
            Queue<CellChange> queue,
            bool[,] visited,
            CellChange change)
        {
            if (change?.Coord == null || !board.IsWithinBound(change.Coord) ||
                visited[change.Coord.X, change.Coord.Y])
            {
                return false;
            }

            visited[change.Coord.X, change.Coord.Y] = true;
            queue.Enqueue(change);
            return true;
        }

        private static void SetCell(Board board, CellChange change, int currentTurn)
        {
            board.SetCell(
                change.Coord,
                BoardCellFactory.Create(change.CellType, currentTurn, change.Coord));
        }
    }
}
