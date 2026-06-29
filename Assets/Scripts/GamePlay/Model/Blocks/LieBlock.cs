using System;

namespace GamePlay
{
    public class LieBlock : MultipleBlockBase
    {
        private Vector2Int _firstPlacedCoord;

        public override CellPlacementResult TryContinuedPlacement(Cell[,] board, Vector2Int coord)
        {
            if (InputState != MultipleBlockInputState.AwaitingContinuedPlacement || _firstPlacedCoord == null)
            {
                return new CellPlacementResult(false, CellPlacementResultType.STATE_NOT_ALLOWED);
            }

            if (coord == _firstPlacedCoord || !Vector2Int.IsAdjacent(_firstPlacedCoord, coord))
            {
                return new CellPlacementResult(false, CellPlacementResultType.LIE_NOT_ADJACENT);
            }

            if (board[coord.X, coord.Y] is BlackCell)
            {
                return new CellPlacementResult(true, CellPlacementResultType.SUCCESS);
            }

            return new CellPlacementResult(false, CellPlacementResultType.OCCUPIED);
        }

        public override Type GetCellType()
        {
            return typeof(LieCell);
        }

        public override int MaxNumTotal { get; } = 2;
        public override int MaxNumPerTurn { get; } = 0;

        public override String Name { get; } = "거짓말";

        protected override int GetSuspicionByCount(int countPerTurn)
        {
            return 52;
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
