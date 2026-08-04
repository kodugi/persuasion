using System;
using Unity.VisualScripting;
using Vector2Int = VectorUtils.Vector2Int;

namespace GamePlay
{
    public class WeakBlackCell : BlackCell
    {
        public WeakBlackCell(int placedTurn, Vector2Int coord) : base(placedTurn, coord)
        {
        }

        public WeakBlackCell(Vector2Int coord) : base(coord) { }
        public override CellKind CellKind { get; } = CellKind.WeakBlack;
    }
}