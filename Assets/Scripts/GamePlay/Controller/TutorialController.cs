using System.Collections.Generic;
using System;
using UnityEngine;
using SingletonUtils;
using Vector2Int = VectorUtils.Vector2Int;

namespace GamePlay
{
    public class TutorialController: Singleton<TutorialController>, IDisposable
    {
        private Dictionary<TutorialState, List<TutorialEntry>> _tutorialEntriesDict;
        private TutorialState _currentState;
        private bool _currentStateWasTriggeredWithDialogue;
        private List<Vector2Int> _currentCellCoords;
        private float _nextStateDelayRemaining = -1f;

        private DialogueManager _dialogueManager;
        private TurnManager _turnManager;
        private BoardController _boardController;

        public event EventHandler<SetTutorialStateEventArgs> RaiseSetTutorialStateEvent;

        public void Initialize(
            Dictionary<TutorialState, List<TutorialEntry>> tutorialEntries,
            DialogueManager dialogueManager,
            TurnManager turnManager,
            BoardController boardController)
        {
            _tutorialEntriesDict = tutorialEntries ?? new Dictionary<TutorialState, List<TutorialEntry>>();
            _currentState = TutorialState.None;
            _currentStateWasTriggeredWithDialogue = false;
            _currentCellCoords = new List<Vector2Int>();
            _nextStateDelayRemaining = -1f;
            _dialogueManager = dialogueManager ?? throw new ArgumentNullException(nameof(dialogueManager));
            _turnManager = turnManager ?? throw new ArgumentNullException(nameof(turnManager));
            _boardController = boardController ?? throw new ArgumentNullException(nameof(boardController));
            
            _dialogueManager.RaiseSetDialogueEntryEvent += HandleSetDialogueEntryEvent;
            _dialogueManager.RaiseDialoguePageEndEvent += HandleDialoguePageEndEvent;
            RaiseSetTutorialStateEvent += HandleSetTutorialStateEvent;
            _boardController.RaiseCellPlacementEvent += HandleCellPlacementEvent;
        }

        public void Dispose()
        {
            if (_dialogueManager != null)
            {
                _dialogueManager.RaiseSetDialogueEntryEvent -= HandleSetDialogueEntryEvent;
                _dialogueManager.RaiseDialoguePageEndEvent -= HandleDialoguePageEndEvent;
            }

            RaiseSetTutorialStateEvent -= HandleSetTutorialStateEvent;

            if (_boardController != null)
            {
                _boardController.RaiseCellPlacementEvent -= HandleCellPlacementEvent;
            }

            RaiseSetTutorialStateEvent = null;
            _tutorialEntriesDict = null;
            _currentCellCoords = null;
            _dialogueManager = null;
            _turnManager = null;
            _boardController = null;
            ReleaseInstance();
        }

        public void ResetGame()
        {
            _currentState = TutorialState.None;
            _currentStateWasTriggeredWithDialogue = false;
            _currentCellCoords = new List<Vector2Int>();
            _nextStateDelayRemaining = -1f;
            RaiseSetTutorialStateEvent?.Invoke(
                this,
                new SetTutorialStateEventArgs(TutorialState.None, GetTutorialEntries(TutorialState.None)));
        }

        public void Tick(float deltaTime)
        {
            if (_nextStateDelayRemaining < 0f)
            {
                return;
            }

            _nextStateDelayRemaining -= Mathf.Max(0f, deltaTime);
            if (_nextStateDelayRemaining > 0f)
            {
                return;
            }

            _nextStateDelayRemaining = -1f;
            ToNextState();
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

        public bool IsWaitingForInteractionOutsideDialogue()
        {
            if (_currentState == TutorialState.None || _currentStateWasTriggeredWithDialogue)
            {
                return false;
            }

            if (_currentCellCoords != null && _currentCellCoords.Count > 0)
            {
                return true;
            }

            return _currentState == TutorialState.ExplainEndTurn ||
                   GetOverlayClickHandler(_currentState) != null;
        }

        public void NotifyEndTurnClicked()
        {
            if (_currentState == TutorialState.ExplainEndTurn)
            {
                SetTutorialState(TutorialState.None);
            }
        }

        public void NotifyScreenClicked()
        {
            ToNextState();
        }

        private void HandleSetDialogueEntryEvent(object sender, SetDialogueEntryEventArgs e)
        {
            DialogueEntry dialogueEntry = e.GetDialogueEntry();
            if (_currentStateWasTriggeredWithDialogue)
            {
                SetTutorialState(TutorialState.None);
            }

            if (ShouldTriggerTutorialState(dialogueEntry, TutorialStateTriggerTiming.WithDialogue))
            {
                SetTutorialState(dialogueEntry.StateToTrigger, true);
            }
        }

        private void HandleDialoguePageEndEvent(object sender, DialoguePageEndEventArgs e)
        {
            DialogueEntry lastDialogueEntry = e.GetLastDialogueEntry();
            if (_currentStateWasTriggeredWithDialogue)
            {
                SetTutorialState(TutorialState.None);
            }

            if (ShouldTriggerTutorialState(lastDialogueEntry, TutorialStateTriggerTiming.AfterDialogue))
            {
                SetTutorialState(lastDialogueEntry.StateToTrigger);
            }
        }

        private bool ShouldTriggerTutorialState(
            DialogueEntry dialogueEntry,
            TutorialStateTriggerTiming triggerTiming)
        {
            return dialogueEntry != null &&
                   dialogueEntry.StateToTrigger != TutorialState.None &&
                   dialogueEntry.StateTriggerTiming == triggerTiming;
        }

        private void SetTutorialState(TutorialState tutorialState, bool triggeredWithDialogue = false)
        {
            _nextStateDelayRemaining = -1f;
            _currentState = tutorialState;
            _currentStateWasTriggeredWithDialogue = tutorialState != TutorialState.None && triggeredWithDialogue;
            Debug.Log("set tutorial state to " + tutorialState);
            List<TutorialEntry> tutorialEntries = GetTutorialEntries(tutorialState);
            RaiseSetTutorialStateEvent?.Invoke(this, new SetTutorialStateEventArgs(tutorialState, tutorialEntries));
            ScheduleNextStateAfterDelay(tutorialState, tutorialEntries);
        }

        private void ScheduleNextStateAfterDelay(TutorialState tutorialState, List<TutorialEntry> tutorialEntries)
        {
            if (tutorialState == TutorialState.None || tutorialEntries == null)
            {
                return;
            }

            foreach (TutorialEntry tutorialEntry in tutorialEntries)
            {
                if (tutorialEntry == null || tutorialEntry.NextStateDelay < 0f)
                {
                    continue;
                }

                _nextStateDelayRemaining = tutorialEntry.NextStateDelay;
                return;
            }
        }

        private void HandleSetTutorialStateEvent(object sender, SetTutorialStateEventArgs e)
        {
            _currentCellCoords = new List<Vector2Int>();
            List<RectTransform> focusTargets = new List<RectTransform>();
            List<GameObject> focusWorldTargets = new List<GameObject>();
            RectTransform defaultFocusTarget = GetDefaultFocusTarget(e.CurrentState);
            bool shouldBlockOverlayRaycasts = !_currentStateWasTriggeredWithDialogue &&
                                               ShouldBlockOverlayRaycasts(e.CurrentState);
            BoardView boardView = BoardView.Instance as BoardView;

            if (defaultFocusTarget != null)
            {
                focusTargets.Add(defaultFocusTarget);
            }

            if (TutorialOverlayView.Instance != null)
            {
                TutorialOverlayView.Instance.Hide();
            }

            if (boardView != null)
            {
                boardView.ClearTutorialHints();
            }

            if (e.CurrentState == TutorialState.None)
            {
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
                Vector2Int highlightedCellCoord = tutorialEntry.HighlightedCellCoord;
                GameObject gameObjectToMark = tutorialEntry.GameObjectToMark;

                if (cellCoord != null && cellType != null)
                {
                    _currentCellCoords.Add(cellCoord);
                    if (boardView != null)
                    {
                        boardView.ShowTutorialHint(cellCoord, cellType);
                    }
                }
                else if (highlightedCellCoord != null)
                {
                    if (boardView != null && boardView.TryGetCellObject(highlightedCellCoord, out GameObject cellObject))
                    {
                        focusWorldTargets.Add(cellObject);
                    }
                }
                else if (gameObjectToMark != null)
                {
                    RectTransform rectTransform = gameObjectToMark.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        focusTargets.Add(rectTransform);
                    }
                    else
                    {
                        focusWorldTargets.Add(gameObjectToMark);
                    }
                }
            }

            if (TutorialOverlayView.Instance != null && (focusTargets.Count > 0 || focusWorldTargets.Count > 0))
            {
                TutorialOverlayView.Instance.Focus(
                    focusTargets,
                    focusWorldTargets,
                    shouldBlockOverlayRaycasts,
                    GetOverlayClickHandler(e.CurrentState));
            }
        }

        private void HandleCellPlacementEvent(object sender, CellPlacementEventArgs e)
        {
            if (_currentState == TutorialState.None)
            {
                return;
            }

            if (_currentCellCoords == null || _currentCellCoords.Count == 0)
            {
                return;
            }

            if (!_currentCellCoords.Remove(e.GetCoord()))
            {
                return;
            }

            if (_currentCellCoords.Count == 0)
            {
                ToNextState();
            }
        }

        private void ToNextState()
        {
            switch (_currentState)
            {
                case TutorialState.BeforeExplainSuspicionManagement1:
                    SetTutorialState(TutorialState.BeforeExplainSuspicionManagement2);
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
            if (_currentStateWasTriggeredWithDialogue)
            {
                return null;
            }

            switch (tutorialState)
            {
                case TutorialState.ExplainSuspicion:
                case TutorialState.ExplainTargetNumber:
                case TutorialState.ExplainOriginalBlack:
                    return NotifyScreenClicked;
                default:
                    return null;
            }
        }
    }

    public enum TutorialState
    {
        PlaceFirstCell = 1,
        PlaceSecondCell = 2,
        ExplainSuspicion = 3,
        ExplainEndTurn = 4,
        ExplainWeakThought = 5,
        PlaceWeakThought = 6,
        ExplainTargetNumber = 7,
        ExplainOriginalBlack = 8,
        ExplainEnemyCellFlip = 9,
        ExplainEnemyCellInBetween = 10,
        ExplainDiagonal = 11,
        ExplainWeakThoughtIsBlack1 = 12,
        ExplainWeakThoughtIsBlack2 = 13,
        BeforeExplainSuspicionManagement1 = 14,
        BeforeExplainSuspicionManagement2 = 15,
        ExplainSuspicionManagement = 16,
        BeforeExplainLock = 17,
        ExplainLock = 18,
        Dream1 = 19,
        Dream2 = 20,
        ExplainStage = 21,
        None = 0
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
