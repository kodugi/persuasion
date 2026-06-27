using System;
using Unity.VisualScripting;

namespace GamePlay
{
    public class ConceptCell : Cell, IFlippableCell, IFlipperCell
    {
        public bool CanBeFlippedBy(Cell first, Cell second)
        {
            if(first is BlackCell && second is BlackCell) {
                return true;
            }
            return false;
        }

        public Type TryFlip(Cell otherCell, Cell cellToFlip)
        {
            if(otherCell is ConceptCell)
            {
                return typeof(ConceptCell);
            }
            return null;
        }

        public Type TryFlipWeakCell(Cell cellToFlip)
        {
            return typeof(ConceptCell);
        }

        public ConceptCell(int placedTurn, Vector2Int coord) : base(placedTurn, coord)
        {
        }
    }
}