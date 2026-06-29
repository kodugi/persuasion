using System;
using Unity.VisualScripting;

namespace GamePlay
{
    public class ThreatCell : ConceptCell
    {
        public override Type TryBeFlipped(Cell first, Cell second)
        {
            if(first is BlackCell && second is BlackCell)
            {
                return typeof(ThreatCell); // 협박은 뒤집히지 않음, 단 통과는 가능하므로 다시 협박을 반환
            }
            return null;
        }

        public ThreatCell(int placedTurn, Vector2Int coord) : base(placedTurn, coord)
        {
        }

        public override int FlippedPrecedence { get; } = 1;
    }
}