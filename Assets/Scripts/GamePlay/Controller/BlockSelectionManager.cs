using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public class BlockSelectionManager : Singleton<BlockSelectionManager>
    {
        private TurnManager _turnManager;
        private BlockSelectionView _blockSelectionView;

        private int _selectedBlockIdx;
        private List<BlockEntry> _blockEntries;
        public IBlock GetSelectedBlock()
        {
            return _blockEntries[_selectedBlockIdx].block;
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
            _blockEntries = new List<BlockEntry>();
            foreach(IBlock block in blockList)
            {
                _blockEntries.Add(new BlockEntry(block));
            }
            _turnManager = TurnManager.Instance;
            _blockSelectionView = BlockSelectionView.Instance;

            _turnManager.RaiseSetTurnEvent += HandleSetTurnEvent;

            _blockSelectionView.SetBlockEntryUI(_blockEntries);
        }

        public bool IsSelectedBlockAvailable()
        {
            BlockEntry selectedBlockEntry = _blockEntries[_selectedBlockIdx];
            if(selectedBlockEntry == null)
            {
                return false;
            }
            if(selectedBlockEntry.block.MaxNumTotal > 0 && selectedBlockEntry.countTotal >= selectedBlockEntry.block.MaxNumTotal)
            {
                Debug.Log("Exceeded maximum total block limit");
                return false;
            }
            if(selectedBlockEntry.block.MaxNumPerTurn > 0 && selectedBlockEntry.countPerTurn >= selectedBlockEntry.block.MaxNumPerTurn)
            {
                Debug.Log("Exceeded maximum block limit per turn");
                return false;
            }
            return true;
        }

        public void PlaceSelectedBlock()
        {
            BlockEntry selectedBlockEntry = _blockEntries[_selectedBlockIdx];
            if (!IsSelectedBlockAvailable())
            {
                Debug.LogError("Selected block is not available!");
                return;
            }
            selectedBlockEntry.countTotal++;
            selectedBlockEntry.countPerTurn++;
        }

        private void ResetCountPerTurn()
        {
            foreach(BlockEntry blockEntry in _blockEntries)
            {
                blockEntry.countPerTurn = 0;
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

    public class BlockEntry
    {
        public IBlock block;
        public int countTotal;
        public int countPerTurn;

        public BlockEntry(IBlock block)
        {
            this.block = block;
            countTotal = 0;
            countPerTurn = 0;
        }
    }
}
