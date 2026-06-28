using System;

namespace GamePlay
{
    public class BasicBlock : IBlock
    {
        public CellPlacementResult TryPlacement(Cell[,] board, Vector2Int coord)
        {
            if (board[coord.X, coord.Y] is EmptyCell || board[coord.X, coord.Y] is ConceptCell)
            {
                return new CellPlacementResult(true, CellPlacementResultType.SUCCESS);
            }
            return new CellPlacementResult(false, CellPlacementResultType.OCCUPIED);
        }
        public Type GetCellType()
        {
            return typeof(ConceptCell);
        }

        public TurnState GetNextTurnState()
        {
            return TurnState.PlayerIdle;
        }

        public int MaxNumTotal { get; } = 0;
        public int MaxNumPerTurn { get; } = 3;

        public String Name { get; } = "무해함";
    }
}
