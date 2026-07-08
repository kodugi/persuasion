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

        public DialogueEntry(string speakerName, string dialogueText, TutorialState stateToTrigger)
        {
            SpeakerName = speakerName;
            DialogueText = dialogueText;
            StateToTrigger = stateToTrigger;
        }
    }
}