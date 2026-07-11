using System;
using System.Collections.Generic;
using GamePlay;
using UnityEngine;
using SingletonUtils;
using UnityEngine.UI;

namespace MapEditor
{
    public class CellSelectionView : SelfInitializingMonoBehaviourSingleton<CellSelectionView>
    {
        [SerializeField] private CellKind[] _cellKinds;
        [SerializeField] private GameObject _cellEntryPrefab;

        private List<CellEntryView> _spawnedCellEntryViews;

        protected override bool InitializeCore()
        {
            if (_cellKinds == null)
            {
                Debug.LogError("CellSelectionView: _cellKinds is null");
                return false;
            }
            if (_cellEntryPrefab == null)
            {
                Debug.LogError("cell entry prefab is null");
                return false;
            }
            
            _spawnedCellEntryViews = new List<CellEntryView>();

            for (int i = 0; i < _cellKinds.Length; i++)
            {
                int cellEntryIdx = i;
                GameObject cellEntryGO = GameObject.Instantiate(_cellEntryPrefab, gameObject.transform);
                CellEntryView cellEntryView = cellEntryGO.GetComponent<CellEntryView>();
                _spawnedCellEntryViews.Add(cellEntryView);
                cellEntryView.Initialize(cellEntryIdx, HandleCellEntryClick, _cellKinds[i]);
            }

            if (CellSelectionManager.Instance == null)
            {
                return false;
            }

            CellSelectionManager.Instance.RaiseSetCurrentCellKindEvent += HandleSetCurrentCellKindEvent;
            
            return true;
        }

        private void HandleCellEntryClick(int idx)
        {
            CellSelectionManager.Instance.SetCurrentCellKind(_cellKinds[idx]);
        }

        private void HandleSetCurrentCellKindEvent(object sender, SetCurrentCellKindEventArgs e)
        {
            foreach (CellEntryView cellEntryView in _spawnedCellEntryViews)
            {
                if (cellEntryView.CellKind == e.CellKind)
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