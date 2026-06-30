using System;
using UnityEngine;

namespace GamePlay
{
    public class TurnManager : Singleton<TurnManager>
    {
        private TurnState _currentState;
        public TurnState GetTurnState()
        {
            return _currentState;
        }
        private int _currentTurn;
        public int GetCurrentTurn()
        {
            return _currentTurn;
        }

        public void Initialize()
        {
            _currentState = TurnState.PlayerIdle;
            _currentTurn = 0;

            // TODO: 다른 곳에서 Start 상태를 사용할 경우 삭제
            RaiseSetTurnEvent += HandleTurnStart;
        }

        public event EventHandler<SetTurnEventArgs> RaiseSetTurnEvent;

        public void SetTurnState(TurnState turnState)
        {
            _currentState = turnState;
            if(turnState == TurnState.End)
            {
                _currentState = TurnState.Start;
                _currentTurn++;
            }
            Debug.Log("changed state to " + _currentState);
            RaiseSetTurnEvent.Invoke(this, new SetTurnEventArgs(_currentState));
        }

        // TODO: 다른 곳에서 Start 상태를 사용할 경우 삭제
        private void HandleTurnStart(object sender, SetTurnEventArgs e)
        {
            if(e.turnState == TurnState.Start)
            {
                SetTurnState(TurnState.PlayerIdle);
            }
        }
    }

    public class SetTurnEventArgs : EventArgs
    {
        public TurnState turnState { get; }
        public SetTurnEventArgs(TurnState turnState)
        {
            this.turnState = turnState;
        }
    }
}
