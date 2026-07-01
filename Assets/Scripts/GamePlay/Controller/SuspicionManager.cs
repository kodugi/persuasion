using System;

namespace GamePlay
{
    public class SuspicionManager: Singleton<SuspicionManager>
    {
        private int _currentSuspicion;
        private int _currentSuspicionPreview;
        private int _maxSuspicion;
        private int _decrementAmount;

        private BlockSelectionManager _blockSelectionManager;
        private TurnManager _turnManager;

        public event EventHandler<SetSuspicionEventArgs> RaiseSetSuspicionEvent;
        public event EventHandler<SetSuspicionEventArgs> RaiseSetSuspicionPreviewEvent;

        public void Initialize(int maxSuspicion, int decrementAmount)
        {
            _currentSuspicion = 0;
            _maxSuspicion = maxSuspicion;
            _decrementAmount = decrementAmount;
            _blockSelectionManager = BlockSelectionManager.Instance;
            _turnManager = TurnManager.Instance;

            _blockSelectionManager.RaisePlaceBlock += HandlePlaceBlockEvent;
            _blockSelectionManager.RaiseSelectBlockEvent += HandleSelectBlockEvent;
            _turnManager.RaiseSetTurnEvent += HandleSetTurnEvent;
            SetSuspicion(0);
            SetSuspicionPreview(_blockSelectionManager.GetSelectedBlock().GetSuspicion());
        }

        public int GetCurrentSuspicion()
        {
            return _currentSuspicion;
        }

        public int GetCurrentSuspicionPreview()
        {
            return _currentSuspicionPreview;
        }

        public int GetMaxSuspicion()
        {
            return _maxSuspicion;
        }

        public void IncrementSuspicion(int incrementAmount)
        {
            SetSuspicion(_currentSuspicion + incrementAmount);
        }

        public void DecrementSuspicion()
        {
            SetSuspicion(Math.Max(_currentSuspicion - _decrementAmount, 0));
        }

        private void SetSuspicion(int suspicion)
        {
            _currentSuspicion = suspicion;
            RaiseSetSuspicionEvent?.Invoke(this, new SetSuspicionEventArgs(suspicion));
        }

        private void SetSuspicionPreview(int suspicion)
        {
            _currentSuspicionPreview = suspicion;
            RaiseSetSuspicionPreviewEvent?.Invoke(this, new SetSuspicionEventArgs(suspicion));
        }

        private void HandlePlaceBlockEvent(object sender, PlaceBlockEventArgs e)
        {
            IncrementSuspicion(e.IncrementAmount);
        }

        private void HandleSelectBlockEvent(object sender, SelectBlockEventArgs e)
        {
            SetSuspicionPreview(_currentSuspicion + e.IncrementAmount);
        }

        private void HandleSetTurnEvent(object sender, SetTurnEventArgs e)
        {
            if(e.turnState == TurnState.Start)
            {
                DecrementSuspicion();
            }
        }
    }

    public class SetSuspicionEventArgs: EventArgs
    {
        public int Suspicion { get; }

        public SetSuspicionEventArgs(int suspicion)
        {
            Suspicion = suspicion;
        }
    }
}
