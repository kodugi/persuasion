using System;
using GamePlay;

namespace MapEditor
{
    public class EmptyBlock: BlockBase
    {
        public override int MaxNumPerTurn { get; } = 0;
        public override int MaxNumTotal { get; } = 0;
        public override string Name { get; } = "빈 생각";
        public override Type GetCellType()
        {
            return typeof(EmptyCell);
        }

        protected override int GetSuspicionByCount(int countPerTurn)
        {
            return 0;
        }
    }
}