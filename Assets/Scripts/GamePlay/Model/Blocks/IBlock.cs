using System;
using UnityEngine;

namespace GamePlay
{
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

        public virtual CellPlacementResult TryPlacement(Cell[,] board, Vector2Int coord)
        {
            if (board[coord.X, coord.Y] is EmptyCell || board[coord.X, coord.Y] is ConceptCell)
            {
                return new CellPlacementResult(true, CellPlacementResultType.SUCCESS);
            }
            return new CellPlacementResult(false, CellPlacementResultType.OCCUPIED);
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
            Debug.Log("set state to AwaitingContinuedPlacement");
            InputState = MultipleBlockInputState.AwaitingContinuedPlacement;
        }

        public virtual void RegisterContinuedPlacement(Vector2Int coord)
        {
            Debug.Log("set state to Completed");
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
            Debug.Log("set state to Ready");
        }
    }
}
