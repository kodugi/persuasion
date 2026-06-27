using System.Collections.Generic;

namespace GamePlay
{
    public class BlockSelectionManager
    {
        public static BlockSelectionManager Instance { get; private set; }
        public BlockSelectionManager()
        {
            if (Instance != null)
            {
                throw new System.Exception("BlockSelectionManager instance already exists!");
            }
            Instance = this;
        }

        private int _selectedBlockIdx;
        private List<IBlock> _blockList;
        public IBlock GetSelectedBlock()
        {
            return _blockList[_selectedBlockIdx];
        }
        public void SetSelectedBlockIdx(int selectedBlockIdx) {
            _selectedBlockIdx = selectedBlockIdx;
        }

        public void Initialize(List<IBlock> blockList)
        {
            _selectedBlockIdx = 0;
            _blockList = blockList;
        }
    }
}