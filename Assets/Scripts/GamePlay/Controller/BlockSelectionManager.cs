using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public class BlockSelectionManager : Singleton<BlockSelectionManager>
    {
        private TurnManager _turnManager;
        private BlockSelectionView _blockSelectionView;

        private int _selectedBlockIdx;
        private List<IBlock> _blocks;
        public IBlock GetSelectedBlock()
        {
            return _blocks[_selectedBlockIdx];
        }
        public void SetSelectedBlockIdx(int selectedBlockIdx) {
            if(_turnManager.GetTurnState() == TurnState.PlayerIdle)
            {
                _selectedBlockIdx = selectedBlockIdx;
                _blockSelectionView.SetSelectedBlockUI(selectedBlockIdx);
            }
        }

        public void Initialize(List<IBlock> blockList)
        {
            _selectedBlockIdx = 0;
            _blocks = new List<IBlock>(blockList);
            foreach(IBlock block in blockList)
            {
                block.Reset();
            }
            _turnManager = TurnManager.Instance;
            _blockSelectionView = BlockSelectionView.Instance;

            _turnManager.RaiseSetTurnEvent += HandleSetTurnEvent;

            _blockSelectionView.SetBlockUI(_blocks);
        }

        public bool IsSelectedBlockAvailable()
        {
            IBlock selectedBlock = _blocks[_selectedBlockIdx];
            if(selectedBlock == null)
            {
                return false;
            }
            if(selectedBlock.MaxNumTotal > 0 && selectedBlock.CountTotal >= selectedBlock.MaxNumTotal)
            {
                Debug.Log("Exceeded maximum total block limit");
                return false;
            }
            if(selectedBlock.MaxNumPerTurn > 0 && selectedBlock.CountPerTurn >= selectedBlock.MaxNumPerTurn)
            {
                Debug.Log("Exceeded maximum block limit per turn");
                return false;
            }
            return true;
        }

        public event EventHandler<PlaceBlockEventArgs> RaisePlaceBlock;

        public void PlaceSelectedBlock(Vector2Int coord)
        {
            IBlock selectedBlock = _blocks[_selectedBlockIdx];
            if (!IsSelectedBlockAvailable())
            {
                Debug.LogError("Selected block is not available!");
                return;
            }
            int incrementAmount = selectedBlock.GetSuspicion();
            Debug.Log("PlaceSelectedBlock called");
            RaisePlaceBlock?.Invoke(this, new PlaceBlockEventArgs(selectedBlock, incrementAmount));
            selectedBlock.RegisterPlacement(coord);
        }

        public void PlaceContinuedBlock(Vector2Int coord)
        {
            IBlock selectedBlock = _blocks[_selectedBlockIdx];
            if (selectedBlock is MultipleBlockBase multipleBlock)
            {
                Debug.Log("PlaceContinuedBlock called");
                multipleBlock.RegisterContinuedPlacement(coord);
                if(multipleBlock.InputState == MultipleBlockInputState.Completed)
                {
                    multipleBlock.ResetBlockPlacementState();
                }
                return;
            }

            Debug.LogError("Selected block does not support continued placement!");
        }

        private void ResetCountPerTurn()
        {
            foreach(IBlock block in _blocks)
            {
                block.ResetTurn();
            }
        }

        private void HandleSetTurnEvent(object sender, SetTurnEventArgs e)
        {
            switch (e.turnState)
            {
                case TurnState.Start:
                    ResetCountPerTurn();
                    break;
                default:
                    break;
            }
        }
    }

    public class PlaceBlockEventArgs : EventArgs
    {
        public IBlock Block { get; }
        public int IncrementAmount { get; }

        public PlaceBlockEventArgs(IBlock block, int incrementAmount)
        {
            Block = block;
            IncrementAmount = incrementAmount;
        }
    }
}
