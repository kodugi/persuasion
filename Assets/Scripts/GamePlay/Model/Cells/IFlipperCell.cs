using System;

namespace GamePlay
{
    public interface IFlipperCell
    {
        public Type TryFlip(Cell otherCell, Cell cellToFlip);
        public Type TryFlipWeakCell(Cell cellToFlip);
    }
}