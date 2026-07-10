using System;
using UnityEngine;
using UnityEngine.UI;

namespace MapEditor
{
    public class CellEntryView: MonoBehaviour
    {
        private Button _button;
        public Type CellType { get; private set; }

        public void Initialize(int idx, Action<int> handler, Type cellType)
        {
            _button = gameObject.GetComponent<Button>();
            if (_button == null)
            {
                Debug.LogError("cell entry prefab does not have a button component");
                return;
            }
            _button.onClick.AddListener(() => handler(idx));
            CellType = cellType;
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