using System;

namespace GamePlay
{
    public class DisdainCell: BlackCell
    {
        public override Type TryBeFlipped(Cell first, Cell second)
        {
            if(first is ReligiousCell && second is ReligiousCell)
            {
                return typeof(ConceptCell);
            }
            return null;
        }

        public DisdainCell(int placedTurn, Vector2Int coord) : base(placedTurn, coord) { }

        public DisdainCell(Vector2Int coord) : base(coord) { }

        public override int FlippedPrecedence { get; } = 2;
    }
}