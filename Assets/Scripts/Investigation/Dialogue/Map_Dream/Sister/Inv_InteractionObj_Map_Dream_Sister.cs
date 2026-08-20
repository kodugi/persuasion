using System.Collections;
using UnityEngine;

namespace Investigation
{
    public class Inv_InteractionObj_Map_Dream_Sister : Inv_InteractionObj
    {
        private Inv_Interact interactManager;
        private bool dialogueTriggered;

        protected override void Starter()
        {
            interactManager = FindFirstObjectByType<Inv_Interact>();
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
            interactManager.ForceInteraction(obj_name);
        }
    }
}
