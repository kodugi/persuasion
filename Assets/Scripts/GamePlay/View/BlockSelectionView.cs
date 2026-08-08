using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SingletonUtils;

namespace GamePlay
{
    public class BlockSelectionView : SelfInitializingMonoBehaviourSingleton<BlockSelectionView>
    {
        [SerializeField] private GameObject _blockPanelPrefab;
        [SerializeField] private List<GameObject> _blockUIList;
        private int _selectedBlockIdx;
        
        protected override bool InitializeCore()
        {
            return true;
        }

        public void SetBlockUI(List<IBlock> blocks)
        {
            Debug.Log("blocks size: " + blocks.Count);
            for (int i = 0; i < _blockUIList.Count; i++)
            {
                GameObject blockUI = _blockUIList[i];
                if (i < blocks.Count)
                {
                    int idx = i;
                    IBlock block = blocks[i];
                    
                    blockUI.SetActive(true);
                    Button blockButton = blockUI.GetComponentInChildren<Button>();
                    ModifyBlockUI(blockUI, block);
                    blockButton.onClick.AddListener(() => OnBlockClick(idx));
                }
                else
                {
                    blockUI.SetActive(false);
                }
            }

            SetSelectedBlockUI(0);
        }

        private void ModifyBlockUI(GameObject blockUI, IBlock block)
        {
            // TODO: modify blocks according to its type
        }

        private GameObject GetButtonFromBlockUI(GameObject blockUI)
        {
            return blockUI.GetComponentInChildren<Button>().gameObject;
        }

        public void SetSelectedBlockUI(int selectedBlockIdx)
        {
            if(_blockUIList == null || _blockUIList.Count == 0)
            {
                return;
            }
            GameObject prevButton = GetButtonFromBlockUI(_blockUIList[_selectedBlockIdx]);
            prevButton.GetComponent<Image>().color = Color.white;

            _selectedBlockIdx = selectedBlockIdx;

            GameObject button = GetButtonFromBlockUI(_blockUIList[selectedBlockIdx]);
            button.GetComponent<Image>().color = Color.white;
        }

        private void OnBlockClick(int idx)
        {
            BlockSelectionManager.Instance.SetSelectedBlockIdx(idx);
        }
    }
}