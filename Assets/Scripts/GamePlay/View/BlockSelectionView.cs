using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace GamePlay
{
    public class BlockSelectionView : GamePlay.SelfInitializingMonoBehaviourSingleton<BlockSelectionView>
    {
        [SerializeField] private GameObject _blockPanelPrefab;

        private List<GameObject> _blockEntryUIList;
        private int _selectedBlockEntryIdx;
        
        protected override bool InitializeCore()
        {
            return true;
        }

        public void SetBlockEntryUI(List<BlockEntry> blockEntries)
        {
            _blockEntryUIList = new List<GameObject>();

            // 기존 BlockEntry UI 삭제
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                Destroy(child.gameObject);
            }

            for (int i = 0; i < blockEntries.Count; i++)
            {
                int idx = i;
                BlockEntry blockEntry = blockEntries[i];
                GameObject blockEntryUI = Instantiate(_blockPanelPrefab);
                blockEntryUI.transform.SetParent(transform);
                Button blockEntryButton = blockEntryUI.GetComponentInChildren<Button>();
                
                blockEntryUI.transform.localPosition = GetBlockEntryUIPosition(blockEntries.Count, idx);
                ModifyBlockEntryUI(blockEntryUI, blockEntry);
                blockEntryButton.onClick.AddListener(() => OnBlockEntryClick(idx));
                _blockEntryUIList.Add(blockEntryUI);
            }

            SetSelectedBlockUI(0);
        }

        private Vector3 GetBlockEntryUIPosition(int totalCount, int idx)
        {
            // TODO: UI 디자인 확정 시 맞춰서 변경
            return new Vector3(0, idx * 20, 0);
        }

        private void ModifyBlockEntryUI(GameObject blockEntryUI, BlockEntry blockEntry)
        {
            // TODO: blockEntry의 설정에 따라 UI 컴포넌트 변경
            TextMeshProUGUI text = GetButtonFromBlockEntryUI(blockEntryUI).GetComponentInChildren<TextMeshProUGUI>();
            if(text == null)
            {
                Debug.LogError("tmpro not found");
                return;
            }
            text.text = blockEntry.block.Name;
        }

        private GameObject GetButtonFromBlockEntryUI(GameObject blockEntryUI)
        {
            return blockEntryUI.transform.Find("StrategyButton").gameObject;
        }

        public void SetSelectedBlockUI(int selectedBlockIdx)
        {
            // TODO: 선택된 버튼 표시 방법에 따라 변경
            GameObject prevButton = GetButtonFromBlockEntryUI(_blockEntryUIList[_selectedBlockEntryIdx]);
            prevButton.GetComponent<Image>().color = Color.white;

            _selectedBlockEntryIdx = selectedBlockIdx;

            GameObject button = GetButtonFromBlockEntryUI(_blockEntryUIList[selectedBlockIdx]);
            button.GetComponent<Image>().color = Color.purple;
        }

        private void OnBlockEntryClick(int idx)
        {
            BlockSelectionManager.Instance.SetSelectedBlockIdx(idx);
        }
    }
}