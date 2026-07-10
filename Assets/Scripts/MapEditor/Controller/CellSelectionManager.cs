using System;
using SingletonUtils;
using GamePlay;

namespace MapEditor
{
    public class CellSelectionManager: Singleton<CellSelectionManager>
    {
        private Type _currentCellType;

        public event EventHandler<SetCurrentCellTypeEventArgs> RaiseSetCurrentCellTypeEvent; 
        
        public void Initialize()
        {
            _currentCellType = typeof(EmptyCell);
        }

        public Type GetCurrentCellType()
        {
            return _currentCellType;
        }
        
        public void SetCurrentCellType(Type type)
        {
            _currentCellType = type;
            RaiseSetCurrentCellTypeEvent?.Invoke(this, new SetCurrentCellTypeEventArgs(_currentCellType));
        }
    }

    public class SetCurrentCellTypeEventArgs : EventArgs
    {
        public Type CellType { get; private set; }
        public SetCurrentCellTypeEventArgs(Type cellType)
        {
            CellType = cellType;
        }
    }
}