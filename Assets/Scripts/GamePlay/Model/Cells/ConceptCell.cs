using System;
using Unity.VisualScripting;

namespace GamePlay
{
    public class ConceptCell : Cell, IFlippableCell, IWeakFlipperCell
    {
        public virtual Type TryBeFlipped(Cell first, Cell second)
        {
            if(first is BlackCell && second is BlackCell)
            {
                return typeof(WeakBlackCell);
            }
            return null;
        }

        public virtual Type TryFlip(Cell otherCell, Cell cellToFlip)
        {
            if(otherCell is ConceptCell)
            {
                return typeof(ConceptCell);
            }
            return null;
        }

        public virtual Type TryFlipWeakCell(Cell cellToFlip)
        {
            return typeof(ConceptCell);
        }

        public ConceptCell(int placedTurn, Vector2Int coord) : base(placedTurn, coord)
        {
        }

        public virtual int FlippedPrecedence { get; } = 0;
        public virtual int FlipperPrecedence { get; } = 0;
    }
}