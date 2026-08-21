using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    [CreateAssetMenu(fileName = "DialogueTriggerData", menuName = "GamePlay/Dialogue Trigger Data")]
    public class DialogueTriggerData : ScriptableObject
    {
        [SerializeField, Min(0)] private int _turn;
        [SerializeField] private TurnState _turnState;
        [SerializeField] private List<DialoguePageData> _pages = new List<DialoguePageData>();

        public int Turn
        {
            get { return Math.Max(0, _turn); }
        }

        public TurnState TurnState
        {
            get { return _turnState; }
        }

        public bool TryCreateDialogueData(out DialogueData dialogueData)
        {
            dialogueData = null;
            List<List<DialogueEntry>> dialogueList = new List<List<DialogueEntry>>();

            if (_pages != null)
            {
                foreach (DialoguePageData page in _pages)
                {
                    if (page == null || !page.TryCreateDialogueEntries(out List<DialogueEntry> entries))
                    {
                        continue;
                    }

                    dialogueList.Add(entries);
                }
            }

            if (dialogueList.Count == 0)
            {
                return false;
            }

            dialogueData = new DialogueData(dialogueList);
            return true;
        }

        public static DialogueTriggerData FromDialogueData(int turn, TurnState turnState, DialogueData dialogueData)
        {
            if (dialogueData == null || dialogueData.DialogueList == null)
            {
                return null;
            }

            DialogueTriggerData trigger = CreateInstance<DialogueTriggerData>();
            trigger._turn = Math.Max(0, turn);
            trigger._turnState = turnState;

            foreach (List<DialogueEntry> dialoguePage in dialogueData.DialogueList)
            {
                DialoguePageData page = DialoguePageData.FromDialogueEntries(dialoguePage);
                if (page != null)
                {
                    trigger._pages.Add(page);
                }
            }

            return trigger._pages.Count == 0 ? null : trigger;
        }
    }

    [Serializable]
    public class DialoguePageData
    {
        public List<DialogueEntryData> Entries = new List<DialogueEntryData>();

        public bool TryCreateDialogueEntries(out List<DialogueEntry> entries)
        {
            entries = new List<DialogueEntry>();

            if (Entries != null)
            {
                foreach (DialogueEntryData entry in Entries)
                {
                    if (entry == null)
                    {
                        continue;
                    }

                    entries.Add(entry.CreateDialogueEntry());
                }
            }

            return entries.Count > 0;
        }

        public static DialoguePageData FromDialogueEntries(List<DialogueEntry> dialogueEntries)
        {
            if (dialogueEntries == null)
            {
                return null;
            }

            DialoguePageData page = new DialoguePageData();

            foreach (DialogueEntry dialogueEntry in dialogueEntries)
            {
                if (dialogueEntry != null)
                {
                    page.Entries.Add(DialogueEntryData.FromDialogueEntry(dialogueEntry));
                }
            }

            return page.Entries.Count == 0 ? null : page;
        }
    }

    [Serializable]
    public class DialogueEntryData
    {
        public string SpeakerName;
        [TextArea(2, 6)] public string DialogueText;
        public TutorialState StateToTrigger = TutorialState.None;
        public TutorialStateTriggerTiming StateTriggerTiming = TutorialStateTriggerTiming.AfterDialogue;
        [Header("Figure Change")]
        [Tooltip("None keeps the currently displayed figures unchanged. A position with no sprite hides that figure.")]
        public DialogueFigurePosition FigurePosition = DialogueFigurePosition.None;
        public Sprite FigureSprite;
        [Tooltip("Optional second figure change applied with the primary figure change.")]
        public DialogueFigurePosition AdditionalFigurePosition = DialogueFigurePosition.None;
        public Sprite AdditionalFigureSprite;
        [Tooltip("Optional third figure change applied with the primary figure change.")]
        public DialogueFigurePosition TertiaryFigurePosition = DialogueFigurePosition.None;
        public Sprite TertiaryFigureSprite;
        [Tooltip("Hides center and right figures when this dialogue's page ends.")]
        public bool HideFiguresAfterDialogue;

        public DialogueEntry CreateDialogueEntry()
        {
            return new DialogueEntry(
                SpeakerName,
                DialogueText,
                StateToTrigger,
                StateTriggerTiming,
                FigurePosition,
                FigureSprite,
                AdditionalFigurePosition,
                AdditionalFigureSprite,
                TertiaryFigurePosition,
                TertiaryFigureSprite,
                HideFiguresAfterDialogue);
        }

        public static DialogueEntryData FromDialogueEntry(DialogueEntry dialogueEntry)
        {
            if (dialogueEntry == null)
            {
                return null;
            }

            return new DialogueEntryData
            {
                SpeakerName = dialogueEntry.SpeakerName,
                DialogueText = dialogueEntry.DialogueText,
                StateToTrigger = dialogueEntry.StateToTrigger,
                StateTriggerTiming = dialogueEntry.StateTriggerTiming,
                FigurePosition = dialogueEntry.FigurePosition,
                FigureSprite = dialogueEntry.FigureSprite,
                AdditionalFigurePosition = dialogueEntry.AdditionalFigurePosition,
                AdditionalFigureSprite = dialogueEntry.AdditionalFigureSprite,
                TertiaryFigurePosition = dialogueEntry.TertiaryFigurePosition,
                TertiaryFigureSprite = dialogueEntry.TertiaryFigureSprite,
                HideFiguresAfterDialogue = dialogueEntry.HideFiguresAfterDialogue
            };
        }
    }
}
