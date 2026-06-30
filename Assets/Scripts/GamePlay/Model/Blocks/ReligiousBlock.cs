using System;

namespace GamePlay
{
    public class ReligiousBlock : BlockBase
    {
        public override Type GetCellType()
        {
            return typeof(ReligiousCell);
        }

        public override int MaxNumPerTurn { get; } = 0;
        public override int MaxNumTotal { get; } = 0;

        public override string Name { get; } = "종교적 공포 조성";
        protected override int GetSuspicionByCount(int countPerTurn)
        {
            switch (countPerTurn)
            {
                case 0:
                    return 22;
                case 1:
                    return 38;
                default:
                    return 38;
            }
        }
    }
}
