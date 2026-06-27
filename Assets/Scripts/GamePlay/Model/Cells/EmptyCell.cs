using System;

namespace GamePlay
{
    public class EmptyCell : Cell
    {
        public EmptyCell(int placedTurn, Vector2Int coord) : base(placedTurn, coord)
        {
        }

        public EmptyCell(Vector2Int coord) : base(coord) { }
    }
}