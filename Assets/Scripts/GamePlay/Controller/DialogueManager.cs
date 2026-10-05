using System;
using SingletonUtils;

namespace GamePlay
{
    /// <summary>Presentation only. The caller owns progression and gameplay input policy.</summary>
    public sealed class DialogueManager : Singleton<DialogueManager>, IDisposable
    {
        private DialogueEntry _entry;
        private Action _advance;
        private bool _blocksInteraction;
        public bool CanAdvance => _entry != null && _advance != null;
        public float CharacterInterval { get; private set; } = 0.02f;
        public string Instruction { get; private set; }
        public DialoguePresentation Presentation { get; private set; }
        public event EventHandler<SetDialogueEntryEventArgs> RaiseSetDialogueEntryEvent;
        public event EventHandler<DialoguePageEndEventArgs> RaiseDialoguePageEndEvent;
        public event Action<DialogueCue> Cue;

        public void Show(DialogueEntry entry, Action advance, bool blocksInteraction, float characterInterval = 0.02f,
            string instruction = null, DialoguePresentation presentation = DialoguePresentation.Narrative)
        {
            _entry = entry ?? throw new ArgumentNullException(nameof(entry));
            _advance = advance;
            _blocksInteraction = blocksInteraction;
            CharacterInterval = characterInterval;
            Instruction = instruction;
            Presentation = presentation;
            RaiseSetDialogueEntryEvent?.Invoke(this, new SetDialogueEntryEventArgs(entry));
        }

        public void ToNextEntry()
        {
            var advance = _advance;
            _advance = null;
            advance?.Invoke();
        }

        public void Hide()
        {
            var last = _entry;
            _entry = null;
            _advance = null;
            _blocksInteraction = false;
            Instruction = null;
            if (last != null) RaiseDialoguePageEndEvent?.Invoke(this, new DialoguePageEndEventArgs(last));
        }

        public void EmitCue(DialogueCue cue) => Cue?.Invoke(cue);
        public DialogueEntry GetCurrentDialogueEntry() => _entry;
        public bool HasCurrentDialogueData() => _entry != null;
        public bool ShouldBlockInteractionOutsideDialogue() => _entry != null && _blocksInteraction;
        public void ResetGame() => Hide();
        public void Dispose()
        {
            Hide();
            RaiseSetDialogueEntryEvent = null;
            RaiseDialoguePageEndEvent = null;
            Cue = null;
            ReleaseInstance();
        }
    }

    public enum DialoguePresentation { Narrative, Tutorial }
    public enum DialogueCue { DreamReveal, ScriptedDefeat }
    public sealed class SetDialogueEntryEventArgs : EventArgs
    {
        private readonly DialogueEntry _entry;
        public SetDialogueEntryEventArgs(DialogueEntry entry) { _entry = entry; }
        public DialogueEntry GetDialogueEntry() => _entry;
    }
    public sealed class DialoguePageEndEventArgs : EventArgs
    {
        private readonly DialogueEntry _entry;
        public DialoguePageEndEventArgs(DialogueEntry entry) { _entry = entry; }
        public DialogueEntry GetLastDialogueEntry() => _entry;
    }
}
