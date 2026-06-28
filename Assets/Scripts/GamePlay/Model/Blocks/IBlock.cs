using System;

namespace GamePlay
{
    public interface IBlock
    {
        public CellPlacementResult TryPlacement(Cell[,] board, Vector2Int coord);
        public Type GetCellType();
        public TurnState GetNextTurnState();
        public int MaxNumTotal { get; }
        public int MaxNumPerTurn { get; }

        public String Name { get; }
    }
}
