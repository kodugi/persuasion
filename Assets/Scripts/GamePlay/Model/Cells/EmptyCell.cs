using System;
using Vector2Int = VectorUtils.Vector2Int;

namespace GamePlay
{
    public class EmptyCell : Cell
    {
        public EmptyCell(int placedTurn, Vector2Int coord) : base(placedTurn, coord)
        {
        }

        public EmptyCell(Vector2Int coord) : base(coord) { }
        public override CellKind CellKind { get; } = CellKind.Empty;
    }
}
