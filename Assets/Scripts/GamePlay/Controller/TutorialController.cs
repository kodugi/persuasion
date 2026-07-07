using System.Collections.Generic;

namespace GamePlay
{
    public class TutorialController: Singleton<TutorialController>
    {
        private Dictionary<TutorialState, List<Vector2Int>> _cellsToPlace;
        private TutorialState _currentState;

        private DialogueManager _dialogueManager;

        public void Initialize(Dictionary<TutorialState, List<Vector2Int>> cellsToPlace)
        {
            _cellsToPlace = cellsToPlace;
            _dialogueManager = DialogueManager.Instance;
            
            _dialogueManager.RaiseSetDialogueEntryEvent += HandleSetDialogueEntryEvent;
        }

        public bool CanPlaceCellAt(Vector2Int coord)
        {
            if (_currentState == TutorialState.None)
            {
                return true;
            }
            
            List<Vector2Int> allowedCells = _cellsToPlace[_currentState];
            return allowedCells.Contains(coord);
        }

        private void HandleSetDialogueEntryEvent(object sender, SetDialogueEntryEventArgs e)
        {
            if (_dialogueManager.IsDialogueOver() && e.GetDialogueEntry().StateToTrigger != TutorialState.None)
            {
                SetTutorialState(e.GetDialogueEntry().StateToTrigger);
            }
        }

        private void SetTutorialState(TutorialState tutorialState)
        {
            _currentState = tutorialState;
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
}