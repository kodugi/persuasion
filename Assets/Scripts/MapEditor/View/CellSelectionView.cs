using System;
using System.Collections.Generic;
using UnityEngine;
using SingletonUtils;
using UnityEngine.UI;

namespace MapEditor
{
    public class CellSelectionView : SelfInitializingMonoBehaviourSingleton<CellSelectionView>
    {
        [SerializeField] private List<Type> _cellTypes;
        [SerializeField] private GameObject _cellEntryPrefab;

        private List<CellEntryView> _spawnedCellEntryViews;

        protected override bool InitializeCore()
        {
            if (_cellTypes == null)
            {
                Debug.LogError("CellSelectionView: _cellTypes is null");
                return false;
            }
            if (_cellEntryPrefab == null)
            {
                Debug.LogError("cell entry prefab is null");
                return false;
            }
            
            _spawnedCellEntryViews = new List<CellEntryView>();

            for (int i = 0; i < _cellTypes.Count; i++)
            {
                int cellEntryIdx = i;
                GameObject cellEntryGO = GameObject.Instantiate(_cellEntryPrefab);
                CellEntryView cellEntryView = cellEntryGO.GetComponent<CellEntryView>();
                _spawnedCellEntryViews.Add(cellEntryView);
                cellEntryView.Initialize(cellEntryIdx, HandleCellEntryClick, _cellTypes[i]);
            }

            if (CellSelectionManager.Instance == null)
            {
                return false;
            }

            CellSelectionManager.Instance.RaiseSetCurrentCellTypeEvent += HandleSetCurrentCellTypeEvent;
            
            return true;
        }

        private void HandleCellEntryClick(int idx)
        {
            CellSelectionManager.Instance.SetCurrentCellType(_cellTypes[idx]);
        }

        private void HandleSetCurrentCellTypeEvent(object sender, SetCurrentCellTypeEventArgs e)
        {
            foreach (CellEntryView cellEntryView in _spawnedCellEntryViews)
            {
                if (cellEntryView.CellType == e.CellType)
                {
                    cellEntryView.SetSelected(true);
                }
                else
                {
                    cellEntryView.SetSelected(false);
                }
            }
        }
    }
}