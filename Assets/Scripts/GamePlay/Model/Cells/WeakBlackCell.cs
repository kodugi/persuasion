using System;
using Unity.VisualScripting;

namespace GamePlay
{
    public class WeakBlackCell : BlackCell
    {
        public WeakBlackCell(int placedTurn, Vector2Int coord) : base(placedTurn, coord)
        {
        }

        public WeakBlackCell(Vector2Int coord) : base(coord) { }
    }
}