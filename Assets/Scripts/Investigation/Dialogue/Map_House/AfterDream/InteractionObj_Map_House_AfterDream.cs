using UnityEngine;

namespace Investigation
{
    public class InteractionObj_Map_House_AfterDream : InteractionObj
    {
        private const string BedObjectName = "Map_House/Bed";

        public override string StartInteraction()
        {
            string interactionName = base.StartInteraction();
            Transform bedTransform = interactionManager.FindInteractableObj(BedObjectName);

            if (bedTransform != null &&
                bedTransform.TryGetComponent(out InteractionObj bed))
            {
                bed.state = 1;
                bed.variation();
            }
            else
            {
                Debug.LogWarning($"Could not change the image for {BedObjectName}.");
            }

            return interactionName;
        }
    }
}
