using System;
using GamePlay;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MapEditor
{
    public class CellEntryView: MonoBehaviour
    {
        private Button _button;
        private TextMeshProUGUI _label;
        public CellKind CellKind { get; private set; }

        public void Initialize(int idx, Action<int> handler, CellKind cellKind)
        {
            _button = gameObject.GetComponent<Button>();
            if (_button == null)
            {
                Debug.LogError("cell entry prefab does not have a button component");
                return;
            }
            _button.onClick.AddListener(() => handler(idx));
            CellKind = cellKind;
            
            _label = gameObject.GetComponentInChildren<TextMeshProUGUI>();
            if (_label == null)
            {
                Debug.LogWarning("cell entry prefab does not have a TextMeshProUGUI component");
                return;
            }
            _label.text = CellUtils.CellKindToName(CellKind);
        }

        public void SetSelected(bool selected)
        {
            // TODO: implement actual method of highlighting selection
            if (selected)
            {
                _button.image.color = Color.purple;
            }
            else
            {
                _button.image.color = Color.white;
            }
        }
    }
}