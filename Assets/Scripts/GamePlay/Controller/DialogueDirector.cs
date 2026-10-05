using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    /// <summary>Owns narrative triggers and game-over playback independently of tutorial lessons.</summary>
    public sealed class DialogueDirector : IDisposable
    {
        private readonly DialogueManager _dialogue;
        private readonly TurnManager _turn;
        private readonly HashSet<DialogueTriggerData> _played = new HashSet<DialogueTriggerData>();
        private readonly List<DialogueEntry> _lines = new List<DialogueEntry>();
        private int _index;
        private int _generation;
        private TurnState _resume;
        private bool _shouldResume;
        private Action _completed;
        private float _delay = -1f;

        public DialogueDirector(DialogueManager dialogue, TurnManager turn)
        {
            _dialogue = dialogue;
            _turn = turn;
            _turn.RaiseSetTurnStateEvent += HandleTurn;
        }

        public bool TryPlayDialogue(DialogueData data, Action onCompleted = null, bool resumeAfterDialogue = true)
        {
            if (_lines.Count > 0 || _dialogue.HasCurrentDialogueData() || data?.DialogueList == null) return false;
            foreach (var page in data.DialogueList)
                if (page != null) foreach (var line in page) if (line != null) _lines.Add(line);
            if (_lines.Count == 0) return false;
            _index = 0;
            _resume = _turn.GetTurnState();
            _shouldResume = resumeAfterDialogue;
            _completed = onCompleted;
            _turn.SetTurnState(TurnState.Dialogue);
            ShowCurrent();
            return true;
        }

        public void Tick(float delta)
        {
            if (_delay < 0f) return;
            _delay -= Mathf.Max(0f, delta);
            if (_delay > 0f) return;
            _delay = -1f;
            Advance();
        }

        public void ResetGame()
        {
            _generation++;
            _lines.Clear();
            _index = 0;
            _delay = -1f;
            _completed = null;
            _dialogue.Hide();
        }

        public void Dispose()
        {
            _turn.RaiseSetTurnStateEvent -= HandleTurn;
            ResetGame();
            _played.Clear();
        }

        private void HandleTurn(object sender, SetTurnStateEventArgs e)
        {
            if (e.turnState == TurnState.Dialogue || _turn.GetTurnState() != e.turnState || _lines.Count > 0) return;
            var info = GameInfoHolder.GetCurrentGameInfo();
            bool puzzleOnly = ChiefManager.Instance != null && ChiefManager.Instance.IsPuzzleOnly;
            if (info.GetTutorial() != null || !info.TryGetDialogueTrigger(_turn.GetCurrentTurn(), e.turnState,
                    out var trigger, puzzleOnly) || _played.Contains(trigger)) return;
            if (trigger.TryCreateDialogueData(out var data) && TryPlayDialogue(data)) _played.Add(trigger);
        }

        private void ShowCurrent()
        {
            int generation = _generation;
            var line = _lines[_index];
            _dialogue.Show(line, () => { if (generation == _generation) FinishLine(); }, true,
                line.StateToTrigger == TutorialState.Dream2 ? 0.005f : 0.02f);
        }

        private void FinishLine()
        {
            var line = _lines[_index];
            // Only serialized story cues use the old enum. Tutorial steps have explicit completion gates.
            if (line.StateToTrigger == TutorialState.Dream1)
            {
                _dialogue.Hide();
                _dialogue.EmitCue(DialogueCue.DreamReveal);
                _delay = 1f;
                return;
            }
            if (line.StateToTrigger == TutorialState.Dream2) _dialogue.EmitCue(DialogueCue.ScriptedDefeat);
            Advance();
        }

        private void Advance()
        {
            if (++_index < _lines.Count) { ShowCurrent(); return; }
            var completed = _completed;
            bool resume = _shouldResume;
            var state = _resume;
            ResetGame();
            if (resume && _turn.GetTurnState() != TurnState.Paused) _turn.SetTurnState(state);
            completed?.Invoke();
        }
    }
}
