using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GamePlay
{
    public class BlockSelectionView : GamePlay.SelfInitializingMonoBehaviourSingleton<BlockSelectionView>
    {
        [SerializeField] private GameObject _blockPanelPrefab;

        private List<GameObject> _blockUIList;
        private int _selectedBlockIdx;
        
        protected override bool InitializeCore()
        {
            return true;
        }

        public void SetBlockUI(List<IBlock> blocks)
        {
            _blockUIList = new List<GameObject>();
            
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                Destroy(child.gameObject);
            }

            for (int i = 0; i < blocks.Count; i++)
            {
                int idx = i;
                IBlock block = blocks[i];
                GameObject blockUI = Instantiate(_blockPanelPrefab);
                blockUI.transform.SetParent(transform);
                Button blockButton = blockUI.GetComponentInChildren<Button>();
                
                blockUI.GetComponent<RectTransform>().anchoredPosition = GetBlockUIPosition(blocks.Count, idx);
                ModifyBlockUI(blockUI, block);
                blockButton.onClick.AddListener(() => OnBlockClick(idx));
                _blockUIList.Add(blockUI);
            }

            SetSelectedBlockUI(0);
        }

        private Vector2 GetBlockUIPosition(int totalCount, int idx)
        {
            return new Vector3(0, -idx * 200 - 100);
        }

        private void ModifyBlockUI(GameObject blockUI, IBlock block)
        {
            TextMeshProUGUI text = GetButtonFromBlockUI(blockUI).GetComponentInChildren<TextMeshProUGUI>();
            if(text == null)
            {
                Debug.LogError("tmpro not found");
                return;
            }
            text.text = block.Name;
        }

        private GameObject GetButtonFromBlockUI(GameObject blockUI)
        {
            return blockUI.transform.Find("StrategyButton").gameObject;
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
            button.GetComponent<Image>().color = Color.purple;
        }

        private void OnBlockClick(int idx)
        {
            BlockSelectionManager.Instance.SetSelectedBlockIdx(idx);
        }
    }
}