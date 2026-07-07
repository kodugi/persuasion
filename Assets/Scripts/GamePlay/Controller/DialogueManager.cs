using System;

namespace GamePlay
{
    public class DialogueManager: Singleton<DialogueManager>
    {
        private DialogueData _dialogueData;
        private GameStateManager _gameStateManager;
        private int _currentPage;
        private int _currentEntry;
        public event EventHandler<SetDialogueEntryEventArgs> RaiseSetDialogueEntryEvent;
        
        public void Initialize(DialogueData data)
        {
            _dialogueData = data;
            _gameStateManager = GameStateManager.Instance;
            _currentPage = 0;
            _currentEntry = 0;
        }

        public DialogueEntry GetCurrentDialogueEntry()
        {
            return _dialogueData.DialogueList[_currentPage][_currentEntry];
        }

        public void ToNextPage()
        {
            if (_currentPage < _dialogueData.DialogueList.Count)
            {
                _currentPage++;
                _currentEntry = 0;
                RaiseSetDialogueEntryEvent.Invoke(this, new SetDialogueEntryEventArgs(GetCurrentDialogueEntry()));
            }
        }

        public void ToNextEntry()
        {
            if (_currentEntry + 1 < _dialogueData.DialogueList[_currentPage].Count)
            {
                _currentEntry++;
                RaiseSetDialogueEntryEvent.Invoke(this, new SetDialogueEntryEventArgs(GetCurrentDialogueEntry()));
            }
        }

        public bool IsDialogueOver()
        {
            return _currentEntry >= _dialogueData.DialogueList[_currentPage].Count;
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
}