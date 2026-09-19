using System.Collections;
using UnityEngine;

namespace Investigation
{
    public class InteractionObj_Map_Dream_Sister : InteractionObj
    {
        private InteractionCTRL interactManager;
        private bool dialogueTriggered;

        protected override void Starter()
        {
            interactManager = FindFirstObjectByType<InteractionCTRL>();
        }

        public override string StartInteraction()
        {
            base.StartInteraction();
            return "Map_Dream/Sister";
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!dialogueTriggered && collision.gameObject.CompareTag("Player"))
            {
                dialogueTriggered = true;
                StartCoroutine(StartDialogueWhenAvailable());
            }
        }

        private IEnumerator StartDialogueWhenAvailable()
        {
            yield return new WaitUntil(() => !interactManager.isInteracting);
            interactManager.ForceInteraction("Map_Dream/SisterApproach");
        }
    }
}
