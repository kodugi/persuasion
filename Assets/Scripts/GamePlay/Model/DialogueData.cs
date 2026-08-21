using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public class DialogueData
    {
        public List<List<DialogueEntry>> DialogueList;

        public DialogueData(List<List<DialogueEntry>> dialogueList)
        {
            DialogueList = dialogueList;
        }
    }

    public class DialogueEntry
    {
        public string SpeakerName;
        public string DialogueText;
        public TutorialState StateToTrigger;
        public TutorialStateTriggerTiming StateTriggerTiming;
        public DialogueFigurePosition FigurePosition;
        public Sprite FigureSprite;
        public DialogueFigurePosition AdditionalFigurePosition;
        public Sprite AdditionalFigureSprite;
        public DialogueFigurePosition TertiaryFigurePosition;
        public Sprite TertiaryFigureSprite;
        public bool HideFiguresAfterDialogue;

        public DialogueEntry(
            string speakerName,
            string dialogueText,
            TutorialState stateToTrigger,
            TutorialStateTriggerTiming stateTriggerTiming = TutorialStateTriggerTiming.AfterDialogue,
            DialogueFigurePosition figurePosition = DialogueFigurePosition.None,
            Sprite figureSprite = null,
            DialogueFigurePosition additionalFigurePosition = DialogueFigurePosition.None,
            Sprite additionalFigureSprite = null,
            DialogueFigurePosition tertiaryFigurePosition = DialogueFigurePosition.None,
            Sprite tertiaryFigureSprite = null,
            bool hideFiguresAfterDialogue = false)
        {
            SpeakerName = speakerName;
            DialogueText = dialogueText;
            StateToTrigger = stateToTrigger;
            StateTriggerTiming = stateTriggerTiming;
            FigurePosition = figurePosition;
            FigureSprite = figureSprite;
            AdditionalFigurePosition = additionalFigurePosition;
            AdditionalFigureSprite = additionalFigureSprite;
            TertiaryFigurePosition = tertiaryFigurePosition;
            TertiaryFigureSprite = tertiaryFigureSprite;
            HideFiguresAfterDialogue = hideFiguresAfterDialogue;
        }
    }

    public enum DialogueFigurePosition
    {
        None,
        Left,
        Center,
        Right
    }

    public enum TutorialStateTriggerTiming
    {
        AfterDialogue,
        WithDialogue
    }
}
