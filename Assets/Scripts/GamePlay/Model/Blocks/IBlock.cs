using System;

namespace GamePlay
{
    public interface IBlock
    {
        public CellPlacementResult TryPlacement(Cell[][] board, Vector2Int coord);
        public Type GetCellType();
    }
}
