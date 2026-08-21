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

        public DialogueEntry(
            string speakerName,
            string dialogueText,
            TutorialState stateToTrigger,
            TutorialStateTriggerTiming stateTriggerTiming = TutorialStateTriggerTiming.AfterDialogue)
        {
            SpeakerName = speakerName;
            DialogueText = dialogueText;
            StateToTrigger = stateToTrigger;
            StateTriggerTiming = stateTriggerTiming;
        }
    }

    public enum TutorialStateTriggerTiming
    {
        AfterDialogue,
        WithDialogue
    }
}
