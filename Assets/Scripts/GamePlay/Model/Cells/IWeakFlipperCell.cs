using System;

namespace GamePlay
{
    public interface IWeakFlipperCell: IFlipperCell
    {
        public Type TryFlipWeakCell(Cell cellToFlip);
    }
}
