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

        public DialogueEntry(
            string speakerName,
            string dialogueText,
            TutorialState stateToTrigger,
            TutorialStateTriggerTiming stateTriggerTiming = TutorialStateTriggerTiming.AfterDialogue,
            DialogueFigurePosition figurePosition = DialogueFigurePosition.None,
            Sprite figureSprite = null)
        {
            SpeakerName = speakerName;
            DialogueText = dialogueText;
            StateToTrigger = stateToTrigger;
            StateTriggerTiming = stateTriggerTiming;
            FigurePosition = figurePosition;
            FigureSprite = figureSprite;
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
