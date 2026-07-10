using System.Collections.Generic;
using System;
using UnityEngine;
using SingletonUtils;

namespace GamePlay
{
    public class TutorialController: Singleton<TutorialController>
    {
        private Dictionary<TutorialState, List<TutorialEntry>> _tutorialEntriesDict;
        private TutorialState _currentState;
        private List<Vector2Int> _currentCellCoords;

        private DialogueManager _dialogueManager;
        private TurnManager _turnManager;
        private BoardController _boardController;

        public event EventHandler<SetTutorialStateEventArgs> RaiseSetTutorialStateEvent;

        public void Initialize(Dictionary<TutorialState, List<TutorialEntry>> tutorialEntries)
        {
            _tutorialEntriesDict = tutorialEntries ?? new Dictionary<TutorialState, List<TutorialEntry>>();
            _currentState = TutorialState.None;
            _currentCellCoords = new List<Vector2Int>();
            _dialogueManager = DialogueManager.Instance;
            _turnManager = TurnManager.Instance;
            _boardController = BoardController.Instance;
            
            _dialogueManager.RaiseDialoguePageEndEvent += HandleDialoguePageEndEvent;
            RaiseSetTutorialStateEvent += HandleSetTutorialStateEvent;
            _boardController.RaiseCellPlacementEvent += HandleCellPlacementEvent;
        }

        public bool CanPlaceCellAt(Vector2Int coord)
        {
            if (_currentState == TutorialState.None)
            {
                return true;
            }

            return _currentCellCoords != null && _currentCellCoords.Contains(coord);
        }

        public bool CanClickEndTurn()
        {
            return _currentState == TutorialState.None
                || _currentState == TutorialState.ExplainEndTurn;
        }

        public bool CanClickEndPlacement()
        {
            return _currentState == TutorialState.None;
        }

        public void NotifyEndTurnClicked()
        {
            if (_currentState == TutorialState.ExplainEndTurn)
            {
                SetTutorialState(TutorialState.None);
            }
        }

        public void NotifySuspicionExplanationClicked()
        {
            if (_currentState == TutorialState.ExplainSuspicion)
            {
                ToNextState();
            }
        }

        private void HandleDialoguePageEndEvent(object sender, DialoguePageEndEventArgs e)
        {
            if (e.GetLastDialogueEntry().StateToTrigger != TutorialState.None)
            {
                SetTutorialState(e.GetLastDialogueEntry().StateToTrigger);
            }
        }

        private void SetTutorialState(TutorialState tutorialState)
        {
            _currentState = tutorialState;
            Debug.Log("set tutorial state to " + tutorialState);
            RaiseSetTutorialStateEvent?.Invoke(this, new SetTutorialStateEventArgs(tutorialState, GetTutorialEntries(tutorialState)));
        }

        private void HandleSetTutorialStateEvent(object sender, SetTutorialStateEventArgs e)
        {
            _currentCellCoords = new List<Vector2Int>();
            RectTransform focusTarget = GetDefaultFocusTarget(e.CurrentState);
            bool shouldBlockOverlayRaycasts = ShouldBlockOverlayRaycasts(e.CurrentState);

            if (TutorialOverlayView.Instance != null)
            {
                TutorialOverlayView.Instance.Hide();
            }

            if (e.CurrentState == TutorialState.None)
            {
                if (BoardView.Instance != null)
                {
                    BoardView.Instance.ClearTutorialHints();
                }

                return;
            }

            foreach(TutorialEntry tutorialEntry in e.TutorialEntries)
            {
                if (tutorialEntry == null)
                {
                    continue;
                }

                Vector2Int cellCoord = tutorialEntry.CellCoord;
                Type cellType = tutorialEntry.CellType;
                GameObject gameObjectToMark = tutorialEntry.GameObjectToMark;

                if (cellCoord != null && cellType != null)
                {
                    _currentCellCoords.Add(cellCoord);
                    if (BoardView.Instance != null)
                    {
                        BoardView.Instance.ShowTutorialHint(cellCoord, cellType);
                    }
                }
                else if (gameObjectToMark != null)
                {
                    focusTarget = gameObjectToMark.GetComponent<RectTransform>();
                }
            }

            if (TutorialOverlayView.Instance != null && focusTarget != null)
            {
                TutorialOverlayView.Instance.Focus(focusTarget, shouldBlockOverlayRaycasts, GetOverlayClickHandler(e.CurrentState));
            }
        }

        private void HandleCellPlacementEvent(object sender, CellPlacementEventArgs e)
        {
            if (_currentState == TutorialState.None)
            {
                return;
            }

            _currentCellCoords.Remove(e.GetCoord());
            if (_currentCellCoords.Count == 0)
            {
                ToNextState();
            }
        }

        private void ToNextState()
        {
            switch (_currentState)
            {
                case TutorialState.PlaceFirstCell:
                    SetTutorialState(TutorialState.PlaceSecondCell);
                    break;
                default:
                    SetTutorialState(TutorialState.None);
                    _dialogueManager.ToNextPage();
                    break;
            }
        }

        private List<TutorialEntry> GetTutorialEntries(TutorialState tutorialState)
        {
            if (_tutorialEntriesDict != null && _tutorialEntriesDict.TryGetValue(tutorialState, out List<TutorialEntry> tutorialEntries))
            {
                return tutorialEntries;
            }

            return new List<TutorialEntry>();
        }

        private RectTransform GetDefaultFocusTarget(TutorialState tutorialState)
        {
            switch (tutorialState)
            {
                case TutorialState.ExplainSuspicion:
                    return SuspicionView.Instance == null ? null : SuspicionView.Instance.GetFocusTarget();
                case TutorialState.ExplainEndTurn:
                    return ButtonUIView.Instance == null ? null : ButtonUIView.Instance.GetEndTurnButtonTarget();
                default:
                    return null;
            }
        }

        private bool ShouldBlockOverlayRaycasts(TutorialState tutorialState)
        {
            return tutorialState == TutorialState.ExplainEndTurn;
        }

        private Action GetOverlayClickHandler(TutorialState tutorialState)
        {
            switch (tutorialState)
            {
                case TutorialState.ExplainSuspicion:
                    return NotifySuspicionExplanationClicked;
                default:
                    return null;
            }
        }
    }

    public enum TutorialState
    {
        PlaceFirstCell,
        PlaceSecondCell,
        ExplainSuspicion,
        ExplainEndTurn,
        WaitEndTurn,
        ExplainWeakThought,
        None
    }

    public class SetTutorialStateEventArgs : EventArgs
    {
        public TutorialState CurrentState { get; private set; }
        public List<TutorialEntry> TutorialEntries { get; private set; }

        public SetTutorialStateEventArgs(TutorialState tutorialState, List<TutorialEntry> tutorialEntries)
        {
            CurrentState = tutorialState;
            TutorialEntries = tutorialEntries;
        }
    }

}
