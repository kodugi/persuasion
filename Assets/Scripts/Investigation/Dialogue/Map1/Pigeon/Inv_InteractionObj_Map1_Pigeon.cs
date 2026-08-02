using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;


namespace Investigation
{
public class Inv_InteractionObj_Map1_Pigeon: Inv_InteractionObj
    {
        private Inv_Interact interactManager;
        override protected void Starter()
        {
            interactManager = GameObject.FindFirstObjectByType<Inv_Interact>();
        }
        override public void InventoryItemDraggedOn(string itemName)
        {
            if (itemName == "잠자리채")
            {
                interactManager.Effects(
                    new JObject
                    {
                        ["type"] = "variation",
                        ["target"] = "Map1/Writer",
                        ["parameters"] = new JArray
                        {
                            "PigeonDistracted"
                        }
                    }
                );
                interactManager.Effects(
                    new JObject
                    {
                        ["type"] = "item_remove",
                        ["name"] = "잠자리채"
                    }
                );
                interactManager.ForceInteraction("Map1/Writer");
                saveManager.AddProgress("pigeonCaught", true);
                //Temp
                Destroy(gameObject);
            }
        }
        override public void CheckState()
        {
            base.CheckState();
            object pigeonCaught = saveManager.LoadProgress("pigeonCaught");
            if(pigeonCaught != null && ((bool)pigeonCaught)==true)
            {
                Destroy(gameObject);
            }
        }
    }
}
