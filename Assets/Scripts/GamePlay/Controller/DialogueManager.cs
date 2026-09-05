using System;
using System.Collections.Generic;
using UnityEngine;
using SingletonUtils;

namespace GamePlay
{
    public class DialogueManager: Singleton<DialogueManager>, IDisposable
    {
        private Dictionary<int, Dictionary<TurnState, DialogueData>> _dialogueDataDict;
        private DialogueData _currentDialogueData;
        private int _currentPage;
        private int _currentEntry;
        private TurnState _resumeTurnState;
        private bool _hasResumeTurnState;
        private bool _resumeAfterDialogue;
        private bool _playAllPagesContinuously;
        private Action _dialogueCompletedCallback;
        private HashSet<ScriptableObject> _playedDialogueSources;
        public event EventHandler<SetDialogueEntryEventArgs> RaiseSetDialogueEntryEvent;
        public event EventHandler<DialoguePageEndEventArgs> RaiseDialoguePageEndEvent;

        private TurnManager _turnManager;
        private TutorialController _tutorialController;

        public void Initialize(TurnManager turnManager, TutorialController tutorialController)
        {
            _dialogueDataDict = GameInfoHolder.GetCurrentGameInfo().GetDialogueDataDict() ?? new Dictionary<int, Dictionary<TurnState, DialogueData>>();
            _currentDialogueData = null;
            _currentPage = 0;
            _currentEntry = 0;
            _resumeTurnState = TurnState.PlayerIdle;
            _hasResumeTurnState = false;
            _resumeAfterDialogue = true;
            _playAllPagesContinuously = false;
            _dialogueCompletedCallback = null;
            _playedDialogueSources = new HashSet<ScriptableObject>();

            _turnManager = turnManager ?? throw new ArgumentNullException(nameof(turnManager));
            _tutorialController = tutorialController ?? throw new ArgumentNullException(nameof(tutorialController));
            _turnManager.RaiseSetTurnStateEvent += HandleSetTurnStateEvent;
        }

        public void Dispose()
        {
            if (_turnManager != null)
            {
                _turnManager.RaiseSetTurnStateEvent -= HandleSetTurnStateEvent;
            }

            ClearPlaybackHistory();
            RaiseSetDialogueEntryEvent = null;
            RaiseDialoguePageEndEvent = null;
            _dialogueDataDict = null;
            _currentDialogueData = null;
            _dialogueCompletedCallback = null;
            _turnManager = null;
            _tutorialController = null;
            ReleaseInstance();
        }

        public void ResetGame()
        {
            _dialogueDataDict = GameInfoHolder.GetCurrentGameInfo().GetDialogueDataDict() ?? new Dictionary<int, Dictionary<TurnState, DialogueData>>();
            _currentDialogueData = null;
            _currentPage = 0;
            _currentEntry = 0;
            _resumeTurnState = TurnState.PlayerIdle;
            _hasResumeTurnState = false;
            _resumeAfterDialogue = true;
            _playAllPagesContinuously = false;
            _dialogueCompletedCallback = null;

            DialogueView.Instance?.Hide();
        }

        public void ClearPlaybackHistory()
        {
            _playedDialogueSources?.Clear();
        }

        public bool TryPlayDialogue(
            DialogueData dialogueData,
            Action onCompleted = null,
            bool resumeAfterDialogue = true)
        {
            if (!HasDialogueEntries(dialogueData) || HasCurrentDialogueData())
            {
                return false;
            }

            _currentDialogueData = dialogueData;
            _currentPage = 0;
            _currentEntry = 0;
            _resumeAfterDialogue = resumeAfterDialogue;
            _playAllPagesContinuously = true;
            _dialogueCompletedCallback = onCompleted;

            if (resumeAfterDialogue)
            {
                _resumeTurnState = _turnManager.GetTurnState();
                _hasResumeTurnState = true;
            }
            else
            {
                _hasResumeTurnState = false;
            }

            SetDialoguePage(0);
            return true;
        }

        public DialogueEntry GetCurrentDialogueEntry()
        {
            if (!HasCurrentDialogueEntry())
            {
                return null;
            }

            return _currentDialogueData.DialogueList[_currentPage][_currentEntry];
        }

        public void SetDialoguePage(int page)
        {
            if (!HasCurrentDialogueData() || page < 0 || page >= _currentDialogueData.DialogueList.Count)
            {
                return;
            }

            _currentPage = page;
            SetDialogueEntry(0);
        }

        public void SetDialogueEntry(int entry)
        {
            if (!HasCurrentDialogueData() ||
                _currentPage < 0 ||
                _currentPage >= _currentDialogueData.DialogueList.Count ||
                _currentDialogueData.DialogueList[_currentPage] == null ||
                entry < 0 ||
                entry >= _currentDialogueData.DialogueList[_currentPage].Count)
            {
                return;
            }

            if (_turnManager.GetTurnState() != TurnState.Dialogue)
            {
                _turnManager.SetTurnState(TurnState.Dialogue);
            }

            _currentEntry = entry;
            RaiseSetDialogueEntryEvent?.Invoke(this, new SetDialogueEntryEventArgs(GetCurrentDialogueEntry()));
        }

        public void ToNextPage()
        {
            if (!HasCurrentDialogueData())
            {
                return;
            }

            if (_currentPage + 1 < _currentDialogueData.DialogueList.Count)
            {
                SetDialoguePage(_currentPage + 1);
            }
            else if (IsEndOfDialogue())
            {
                CompleteDialogue(GetCurrentDialogueEntry());
            }
        }

        public void ToNextEntry()
        {
            if (!HasCurrentDialogueEntry())
            {
                return;
            }

            if (_currentEntry + 1 < _currentDialogueData.DialogueList[_currentPage].Count)
            {
                SetDialogueEntry(_currentEntry + 1);
            }
            else if (IsEndOfDialoguePage())
            {
                DialogueEntry lastDialogueEntry = GetCurrentDialogueEntry();
                if (ShouldContinueToNextPage(lastDialogueEntry))
                {
                    SetDialoguePage(_currentPage + 1);
                    return;
                }

                if (IsEndOfDialogue())
                {
                    CompleteDialogue(lastDialogueEntry);
                }
                else
                {
                    ResumeTurnState();
                    RaiseDialoguePageEndEvent?.Invoke(this, new DialoguePageEndEventArgs(lastDialogueEntry));
                }
            }
        }

        public bool IsEndOfDialoguePage()
        {
            return HasCurrentDialogueEntry() &&
                   _currentEntry + 1 >= _currentDialogueData.DialogueList[_currentPage].Count;
        }

        public bool IsEndOfDialogue()
        {
            return HasCurrentDialogueData() &&
                   _currentPage + 1 >= _currentDialogueData.DialogueList.Count;
        }

        public DialogueData GetCurrentDialogueData()
        {
            return _currentDialogueData;
        }

        public bool HasCurrentDialogueData()
        {
            return HasDialogueEntries(_currentDialogueData);
        }

        public bool ShouldBlockInteractionOutsideDialogue()
        {
            if (!HasCurrentDialogueData())
            {
                return false;
            }

            return _tutorialController == null ||
                   !_tutorialController.IsWaitingForInteractionOutsideDialogue();
        }

        public bool HasDialogueData()
        {
            if (_dialogueDataDict == null)
            {
                return false;
            }

            foreach (Dictionary<TurnState, DialogueData> dialogueDataByState in _dialogueDataDict.Values)
            {
                if (dialogueDataByState == null)
                {
                    continue;
                }

                foreach (DialogueData dialogueData in dialogueDataByState.Values)
                {
                    if (HasDialogueEntries(dialogueData))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void HandleSetTurnStateEvent(object sender, SetTurnStateEventArgs e)
        {
            if (e.turnState == TurnState.Dialogue || _turnManager.GetTurnState() != e.turnState)
            {
                return;
            }

            int currentTurn = _turnManager.GetCurrentTurn();
            GameInfo gameInfo = GameInfoHolder.GetCurrentGameInfo();
            if (!gameInfo.TryGetDialogueTrigger(
                    currentTurn,
                    e.turnState,
                    out DialogueTriggerData dialogueTrigger) ||
                _playedDialogueSources.Contains(dialogueTrigger))
            {
                return;
            }

            if (TryGetDialogueData(currentTurn, e.turnState, out DialogueData dialogueData))
            {
                _playedDialogueSources.Add(dialogueTrigger);
                _currentDialogueData = dialogueData;
                _resumeTurnState = e.turnState;
                _hasResumeTurnState = true;
                _resumeAfterDialogue = true;
                _playAllPagesContinuously = false;
                _dialogueCompletedCallback = null;
                SetDialoguePage(0);
            }
        }

        private bool TryGetDialogueData(int turn, TurnState turnState, out DialogueData dialogueData)
        {
            dialogueData = null;

            if (!_dialogueDataDict.TryGetValue(turn, out Dictionary<TurnState, DialogueData> dialogueDataByState))
            {
                return false;
            }

            return dialogueDataByState != null &&
                   dialogueDataByState.TryGetValue(turnState, out dialogueData) &&
                   HasDialogueEntries(dialogueData);
        }

        private bool HasDialogueEntries(DialogueData dialogueData)
        {
            if (dialogueData == null || dialogueData.DialogueList == null)
            {
                return false;
            }

            foreach (List<DialogueEntry> dialoguePage in dialogueData.DialogueList)
            {
                if (dialoguePage != null && dialoguePage.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasCurrentDialogueEntry()
        {
            return HasCurrentDialogueData() &&
                   _currentPage >= 0 &&
                   _currentPage < _currentDialogueData.DialogueList.Count &&
                   _currentDialogueData.DialogueList[_currentPage] != null &&
                   _currentEntry >= 0 &&
                   _currentEntry < _currentDialogueData.DialogueList[_currentPage].Count;
        }

        private bool ShouldContinueToNextPage(DialogueEntry dialogueEntry)
        {
            return _currentPage + 1 < _currentDialogueData.DialogueList.Count &&
                   (_playAllPagesContinuously ||
                    (dialogueEntry != null &&
                     dialogueEntry.StateToTrigger != TutorialState.None &&
                     dialogueEntry.StateTriggerTiming == TutorialStateTriggerTiming.WithDialogue));
        }

        private void ResumeTurnState()
        {
            TurnState nextTurnState = _hasResumeTurnState ? _resumeTurnState : TurnState.PlayerIdle;
            _hasResumeTurnState = false;
            _turnManager.SetTurnState(nextTurnState);
        }

        private void CompleteDialogue(DialogueEntry lastDialogueEntry)
        {
            Action completedCallback = _dialogueCompletedCallback;
            _dialogueCompletedCallback = null;

            if (_resumeAfterDialogue)
            {
                ResumeTurnState();
            }
            else
            {
                _hasResumeTurnState = false;
            }

            _resumeAfterDialogue = true;
            _playAllPagesContinuously = false;
            _currentDialogueData = null;
            RaiseDialoguePageEndEvent?.Invoke(this, new DialoguePageEndEventArgs(lastDialogueEntry));
            completedCallback?.Invoke();
        }

    }

    public class SetDialogueEntryEventArgs : EventArgs
    {
        private DialogueEntry _dialogueEntry;

        public SetDialogueEntryEventArgs(DialogueEntry dialogueEntry)
        {
            _dialogueEntry = dialogueEntry;
        }

        public DialogueEntry GetDialogueEntry()
        {
            return _dialogueEntry;
        }
    }

    public class DialoguePageEndEventArgs : EventArgs
    {
        private DialogueEntry _lastDialogueEntry;

        public DialoguePageEndEventArgs(DialogueEntry dialogueEntry)
        {
            _lastDialogueEntry = dialogueEntry;
        }

        public DialogueEntry GetLastDialogueEntry()
        {
            return _lastDialogueEntry;
        }
    }
}
