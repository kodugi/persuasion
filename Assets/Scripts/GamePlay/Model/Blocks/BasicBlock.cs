using System;

namespace GamePlay
{
    public class BasicBlock : IBlock
    {
        public CellPlacementResult TryPlacement(Cell[][] board, Vector2Int coord)
        {
            if (board[coord.X][coord.Y] is EmptyCell)
            {
                return new CellPlacementResult(true, CellPlacementResultType.SUCCESS);
            }
            return new CellPlacementResult(false, CellPlacementResultType.OCCUPIED);
        }
        public Type GetCellType()
        {
            return typeof(ConceptCell);
        }
    }
}