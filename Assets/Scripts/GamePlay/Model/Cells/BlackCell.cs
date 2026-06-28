using System;

namespace GamePlay
{
    public class BlackCell : Cell, IFlippableCell, IFlipperCell
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

        public Type TryFlip(Cell otherCell, Cell cellToFlip)
        {
            if (otherCell is BlackCell)
            {
                if(cellToFlip is ConceptCell)
                {
                    return typeof(WeakBlackCell); // TODO: WeakBlackCell 구현 후 WeakBlackCell로 대체
                }
            }
            return null;
        }
    }
}