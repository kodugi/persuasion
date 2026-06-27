using System;

namespace GamePlay
{
    public class BlackCell : Cell, IFlippableCell
    {
        public virtual bool CanBeFlippedBy(Cell first, Cell second)
        {
            if(first is ConceptCell && second is ConceptCell)
            {
                return true;
            }
            return false;
        }

        public BlackCell(int placedTurn, Vector2Int coord) : base(placedTurn, coord) { }

        public BlackCell(Vector2Int coord) : base(coord) { }
    }
}