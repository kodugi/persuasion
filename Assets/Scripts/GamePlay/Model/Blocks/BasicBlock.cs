using System;

namespace GamePlay
{
    public class BasicBlock : BlockBase
    {
        public override Type GetCellType()
        {
            return typeof(ConceptCell);
        }

        public override int MaxNumTotal { get; } = 0;
        public override int MaxNumPerTurn { get; } = 3;

        public override String Name { get; } = "무해함";

        protected override int GetSuspicionByCount(int countPerTurn)
        {
            switch (countPerTurn)
            {
                case 0:
                    return 18;
                case 1:
                    return 34;
                case 2:
                    return 40;
                default:
                    return 0;
            }
        }
    }
}
