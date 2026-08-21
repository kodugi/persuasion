using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    [CreateAssetMenu(fileName = "GameOverDialogueData", menuName = "GamePlay/Game Over Dialogue Data")]
    public class GameOverDialogueData : ScriptableObject
    {
        [SerializeField] private List<DialoguePageData> _pages = new List<DialoguePageData>();

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

        public static GameOverDialogueData FromDialogueData(DialogueData dialogueData)
        {
            if (dialogueData == null || dialogueData.DialogueList == null)
            {
                return null;
            }

            GameOverDialogueData data = CreateInstance<GameOverDialogueData>();
            foreach (List<DialogueEntry> dialoguePage in dialogueData.DialogueList)
            {
                DialoguePageData page = DialoguePageData.FromDialogueEntries(dialoguePage);
                if (page != null)
                {
                    data._pages.Add(page);
                }
            }

            return data._pages.Count == 0 ? null : data;
        }
    }
}
