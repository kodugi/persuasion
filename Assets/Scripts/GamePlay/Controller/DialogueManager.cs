using System;
using System.Collections.Generic;
using UnityEngine;
using SingletonUtils;

namespace GamePlay
{
    public class DialogueManager: Singleton<DialogueManager>
    {
        private Dictionary<int, Dictionary<TurnState, DialogueData>> _dialogueDataDict;
        private DialogueData _currentDialogueData;
        private GameStateManager _gameStateManager;
        private int _currentPage;
        private int _currentEntry;
        private TurnState _resumeTurnState;
        private bool _hasResumeTurnState;
        private HashSet<DialogueTriggerKey> _playedDialogueTriggers;
        public event EventHandler<SetDialogueEntryEventArgs> RaiseSetDialogueEntryEvent;
        public event EventHandler<DialoguePageEndEventArgs> RaiseDialoguePageEndEvent;

        private TurnManager _turnManager;
        
        public void Initialize(Dictionary<int, Dictionary<TurnState, DialogueData>> dialogueDataDict)
        {
            _dialogueDataDict = dialogueDataDict ?? new Dictionary<int, Dictionary<TurnState, DialogueData>>();
            _gameStateManager = GameStateManager.Instance;
            _currentDialogueData = null;
            _currentPage = 0;
            _currentEntry = 0;
            _resumeTurnState = TurnState.PlayerIdle;
            _hasResumeTurnState = false;
            _playedDialogueTriggers = new HashSet<DialogueTriggerKey>();

            _turnManager = TurnManager.Instance;
            _turnManager.RaiseSetTurnStateEvent += HandleSetTurnStateEvent;
        }

        public DialogueEntry GetCurrentDialogueEntry()
        {
            return GetCurrentDialogueData().DialogueList[_currentPage][_currentEntry];
        }

        public void SetDialoguePage(int page)
        {
            if (GetCurrentDialogueData() == null)
            {
                return;
            }

            _currentPage = page;
            SetDialogueEntry(0);
        }

        public void SetDialogueEntry(int entry)
        {
            if (_turnManager.GetTurnState() != TurnState.Dialogue)
            {
                _turnManager.SetTurnState(TurnState.Dialogue);
            }

            _currentEntry = entry;
            RaiseSetDialogueEntryEvent?.Invoke(this, new SetDialogueEntryEventArgs(GetCurrentDialogueEntry()));
        }

        public void ToNextPage()
        {
            if (_currentPage + 1 < GetCurrentDialogueData().DialogueList.Count)
            {
                SetDialoguePage(_currentPage + 1);
            }
            else if (IsEndOfDialogue())
            {
                ResumeTurnState();
            }
        }

        public void ToNextEntry()
        {
            if (_currentEntry + 1 < GetCurrentDialogueData().DialogueList[_currentPage].Count)
            {
                SetDialogueEntry(_currentEntry + 1);
            }
            else if (IsEndOfDialoguePage())
            {
                DialogueEntry lastDialogueEntry = GetCurrentDialogueEntry();
                ResumeTurnState();
                RaiseDialoguePageEndEvent?.Invoke(this, new DialoguePageEndEventArgs(lastDialogueEntry));
            }
        }

        public bool IsEndOfDialoguePage()
        {
            return _currentEntry + 1 >= GetCurrentDialogueData().DialogueList[_currentPage].Count;
        }

        public bool IsEndOfDialogue()
        {
            return _currentPage + 1 >= GetCurrentDialogueData().DialogueList.Count;
        }

        public DialogueData GetCurrentDialogueData()
        {
            return _currentDialogueData;
        }

        private void HandleSetTurnStateEvent(object sender, SetTurnStateEventArgs e)
        {
            if (e.turnState == TurnState.Dialogue || _turnManager.GetTurnState() != e.turnState)
            {
                return;
            }

            int currentTurn = _turnManager.GetCurrentTurn();
            DialogueTriggerKey triggerKey = new DialogueTriggerKey(currentTurn, e.turnState);
            if (_playedDialogueTriggers.Contains(triggerKey))
            {
                return;
            }

            if (TryGetDialogueData(currentTurn, e.turnState, out DialogueData dialogueData))
            {
                _playedDialogueTriggers.Add(triggerKey);
                _currentDialogueData = dialogueData;
                _resumeTurnState = e.turnState;
                _hasResumeTurnState = true;
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
                   dialogueData != null;
        }

        private void ResumeTurnState()
        {
            TurnState nextTurnState = _hasResumeTurnState ? _resumeTurnState : TurnState.PlayerIdle;
            _hasResumeTurnState = false;
            _turnManager.SetTurnState(nextTurnState);
        }

        private struct DialogueTriggerKey : IEquatable<DialogueTriggerKey>
        {
            private readonly int _turn;
            private readonly TurnState _turnState;

            public DialogueTriggerKey(int turn, TurnState turnState)
            {
                _turn = turn;
                _turnState = turnState;
            }

            public bool Equals(DialogueTriggerKey other)
            {
                return _turn == other._turn && _turnState == other._turnState;
            }

            public override bool Equals(object obj)
            {
                return obj is DialogueTriggerKey other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (_turn * 397) ^ (int)_turnState;
                }
            }
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
