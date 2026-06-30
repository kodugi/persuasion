using System;

namespace GamePlay
{
    public class BlackCell : Cell, IFlippableCell, IFlipperCell
    {
        public virtual Type TryBeFlipped(Cell first, Cell second)
        {
            if(first is ConceptCell && second is ConceptCell)
            {
                return typeof(ConceptCell);
            }
            return null;
        }

        public BlackCell(int placedTurn, Vector2Int coord) : base(placedTurn, coord) { }

        public BlackCell(Vector2Int coord) : base(coord) { }

        public virtual Type TryFlip(Cell otherCell, Cell cellToFlip)
        {
            if (otherCell is BlackCell)
            {
                if(cellToFlip is ConceptCell)
                {
                    return typeof(WeakBlackCell);
                }
            }
            return null;
        }

        public virtual int FlippedPrecedence { get; } = 0;
        public virtual int FlipperPrecedence { get; } = 0;
    }
}