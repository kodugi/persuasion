using System;

namespace GamePlay
{
    public class ThreatBlock : MultipleBlockBase
    {
        private Vector2Int _firstPlacedCoord;

        public override CellPlacementResult TryPlacement(Cell[,] board, Vector2Int coord)
        {
            CellPlacementResult baseResult = base.TryPlacement(board, coord);
            if (!baseResult.GetSuccess())
            {
                return baseResult;
            }

            Vector2Int[] dirs = { new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(-1, 0), new Vector2Int(0, -1) };
            foreach(Vector2Int dir in dirs)
            {
                Vector2Int targetCoord = coord + dir;
                if(targetCoord.X < 0 || targetCoord.X >= board.GetLength(0) || targetCoord.Y < 0 || targetCoord.Y >= board.GetLength(1))
                {
                    continue;
                }
                Cell targetCell = board[targetCoord.X, targetCoord.Y];

                // TODO: 일반 생각 위에 다시 배치도 가능하게 할 것인지?
                if(targetCell is EmptyCell || targetCell is ConceptCell)
                {
                    return new CellPlacementResult(true, CellPlacementResultType.SUCCESS);
                }
            }
            return new CellPlacementResult(false, CellPlacementResultType.THREAT_NO_ADJACENT_CELL_EMPTY);
        }

        public override CellPlacementResult TryContinuedPlacement(Cell[,] board, Vector2Int coord)
        {
            if (InputState != MultipleBlockInputState.AwaitingContinuedPlacement || _firstPlacedCoord == null)
            {
                return new CellPlacementResult(false, CellPlacementResultType.STATE_NOT_ALLOWED);
            }

            if (Vector2Int.TaxiDist(_firstPlacedCoord, coord) != 1)
            {
                return new CellPlacementResult(false, CellPlacementResultType.THREAT_NOT_ADJACENT);
            }

            if (board[coord.X, coord.Y] is EmptyCell || board[coord.X, coord.Y] is ConceptCell)
            {
                return new CellPlacementResult(true, CellPlacementResultType.SUCCESS);
            }

            return new CellPlacementResult(false, CellPlacementResultType.OCCUPIED);
        }

        public override Type GetCellType()
        {
            return typeof(ThreatCell);
        }

        public override int MaxNumTotal { get; } = 0;
        public override int MaxNumPerTurn { get; } = 0;

        public override String Name { get; } = "협박";

        protected override int GetSuspicionByCount(int countPerTurn)
        {
            return 92;
        }

        public override void RegisterPlacement(Vector2Int coord)
        {
            base.RegisterPlacement(coord);
            _firstPlacedCoord = new Vector2Int(coord);
        }

        public override void ResetBlockPlacementState()
        {
            base.ResetBlockPlacementState();
            _firstPlacedCoord = null;
        }
    }
}
