using System;
using Unity.VisualScripting;
using Vector2Int = VectorUtils.Vector2Int;

namespace GamePlay
{
    public class ReligiousCell : ConceptCell
    {

        public ReligiousCell(int placedTurn, Vector2Int coord) : base(placedTurn, coord)
        {
        }

        public override CellKind CellKind { get; } = CellKind.Religious;
    }
}