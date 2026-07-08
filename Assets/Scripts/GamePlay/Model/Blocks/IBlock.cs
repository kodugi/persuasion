using System;
using UnityEngine;

namespace GamePlay
{
    public interface IPlacementTargetPolicy
    {
        public CellPlacementResult TryPlaceOn(Cell cell);
    }

    public sealed class EmptyCellPlacementTargetPolicy : IPlacementTargetPolicy
    {
        public CellPlacementResult TryPlaceOn(Cell cell)
        {
            if (cell is EmptyCell)
            {
                return new CellPlacementResult(true, CellPlacementResultType.SUCCESS);
            }

            return new CellPlacementResult(false, CellPlacementResultType.OCCUPIED);
        }
    }

    public static class PlacementTargetPolicies
    {
        public static readonly IPlacementTargetPolicy EmptyCellOnly = new EmptyCellPlacementTargetPolicy();
    }

    public interface IBlock
    {
        public CellPlacementResult TryPlacement(Cell[,] board, Vector2Int coord);
        public Type GetCellType();
        public int MaxNumTotal { get; }
        public int MaxNumPerTurn { get; }
        public int CountTotal { get; }
        public int CountPerTurn { get; }

        public String Name { get; }

        public int GetSuspicion();
        public void RegisterPlacement(Vector2Int coord);
        public void ResetTurn();
        public void Reset();
    }

    public enum MultipleBlockInputState
    {
        Ready,
        AwaitingContinuedPlacement,
        Completed
    }

    public interface IMultipleBlock : IBlock
    {
        public MultipleBlockInputState InputState { get; }
        public CellPlacementResult TryContinuedPlacement(Cell[,] board, Vector2Int coord);
        public void RegisterContinuedPlacement(Vector2Int coord);
    }

    public abstract class BlockBase : IBlock
    {
        public abstract int MaxNumTotal { get; }
        public abstract int MaxNumPerTurn { get; }
        public int CountTotal { get; private set; }
        public int CountPerTurn { get; private set; }
        public abstract String Name { get; }
        protected virtual IPlacementTargetPolicy InitialPlacementTargetPolicy => PlacementTargetPolicies.EmptyCellOnly;

        public virtual CellPlacementResult TryPlacement(Cell[,] board, Vector2Int coord)
        {
            return InitialPlacementTargetPolicy.TryPlaceOn(board[coord.X, coord.Y]);
        }
        
        public abstract Type GetCellType();

        public int GetSuspicion()
        {
            return GetSuspicionByCount(CountPerTurn);
        }

        public virtual void RegisterPlacement(Vector2Int coord)
        {
            CountTotal++;
            CountPerTurn++;
        }

        public virtual void ResetTurn()
        {
            ResetCountPerTurn();
        }

        private void ResetCountPerTurn()
        {
            CountPerTurn = 0;
        }

        public virtual void Reset()
        {
            ResetCounts();
        }

        private void ResetCounts()
        {
            CountTotal = 0;
            CountPerTurn = 0;
        }

        protected abstract int GetSuspicionByCount(int countPerTurn);
    }

    public abstract class MultipleBlockBase : BlockBase, IMultipleBlock
    {
        public MultipleBlockInputState InputState { get; private set; } = MultipleBlockInputState.Ready;

        public abstract CellPlacementResult TryContinuedPlacement(Cell[,] board, Vector2Int coord);

        public override void RegisterPlacement(Vector2Int coord)
        {
            base.RegisterPlacement(coord);
            InputState = MultipleBlockInputState.AwaitingContinuedPlacement;
        }

        public virtual void RegisterContinuedPlacement(Vector2Int coord)
        {
            InputState = MultipleBlockInputState.Completed;
        }

        public override void ResetTurn()
        {
            base.ResetTurn();
            ResetBlockPlacementState();
        }

        public override void Reset()
        {
            base.Reset();
            ResetBlockPlacementState();
        }

        public virtual void ResetBlockPlacementState()
        {
            ResetInputState();
        }

        protected virtual void ResetInputState()
        {
            InputState = MultipleBlockInputState.Ready;
        }
    }
}
