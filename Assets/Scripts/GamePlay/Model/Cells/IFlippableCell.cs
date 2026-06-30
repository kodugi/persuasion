using System;

namespace GamePlay
{
    public interface IFlippableCell
    {
        public Type TryBeFlipped(Cell first, Cell second);

        public int FlippedPrecedence { get; } // 값이 클수록 우선
    }
}