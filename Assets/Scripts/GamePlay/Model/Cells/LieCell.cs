using System;
using Unity.VisualScripting;

namespace GamePlay
{
    public class LieCell : ConceptCell
    {
        public override Type TryBeFlipped(Cell first, Cell second)
        {
            if(first is BlackCell && second is BlackCell)
            {
                return typeof(BlackCell); // 거짓말은 뒤집힐 경우 의심으로 변환
            }
            return null;
        }

        public override Type TryFlip(Cell otherCell, Cell cellToFlip)
        {
            if(otherCell is ConceptCell)
            {
                if(otherCell is LieCell)
                {
                    return typeof(LieCell);
                }
                
                if(Vector2Int.TaxiDist(_coord, cellToFlip.GetCoord()) <= Vector2Int.TaxiDist(otherCell.GetCoord(), cellToFlip.GetCoord()))
                {
                    // 자신이 반대편 Cell보다 cellToFlip에 가깝거나 같은 경우
                    // 칸들은 일직선상에 있음이 보장되므로 택시 거리로 비교해도 무방
                    return typeof(LieCell);
                }

                return typeof(ConceptCell);
            }
            return null;
        }

        public override Type TryFlipWeakCell(Cell cellToFlip)
        {
            return typeof(LieCell);
        }

        public LieCell(int placedTurn, Vector2Int coord) : base(placedTurn, coord)
        {
        }

        public override int FlippedPrecedence { get; } = 1;
        public override int FlipperPrecedence { get; } = 1;
    }
}