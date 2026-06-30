using System;

namespace GamePlay
{
    public interface IFlipperCell
    {
        public Type TryFlip(Cell otherCell, Cell cellToFlip); // cellToFlip은 항상 뒤집을 수 있는 cell이 온다고 가정

        public int FlipperPrecedence { get; } // 값이 클수록 우선
    }
}