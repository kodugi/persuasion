using System;
using SingletonUtils;
using GamePlay;
using UnityEngine;

namespace MapEditor
{
    public class CellSelectionManager: Singleton<CellSelectionManager>
    {
        private CellKind _currentCellKind;

        public event EventHandler<SetCurrentCellKindEventArgs> RaiseSetCurrentCellKindEvent; 
        
        public void Initialize()
        {
            _currentCellKind = CellKind.Empty;
        }

        public CellKind GetCurrentCellKind()
        {
            return _currentCellKind;
        }
        
        public void SetCurrentCellKind(CellKind cellKind)
        {
            Debug.Log("set current cell kind to " + cellKind);
            _currentCellKind = cellKind;
            RaiseSetCurrentCellKindEvent?.Invoke(this, new SetCurrentCellKindEventArgs(_currentCellKind));
        }
    }

    public class SetCurrentCellKindEventArgs : EventArgs
    {
        public CellKind CellKind { get; private set; }
        public SetCurrentCellKindEventArgs(CellKind cellKind)
        {
            CellKind = cellKind;
        }
    }
}