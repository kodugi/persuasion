using UnityEngine;

namespace Investigation
{
    public class Inv_InteractionObj_Map_House_AfterDream : Inv_InteractionObj
    {
        private const string BedObjectName = "Map_House/Bed";

        public override string StartInteraction()
        {
            string interactionName = base.StartInteraction();
            Transform bedTransform = interactionManager.FindInteractableObj(BedObjectName);

            if (bedTransform != null &&
                bedTransform.TryGetComponent(out Inv_InteractionObj bed))
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
