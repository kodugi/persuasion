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
            if (itemName == "Inventory_ButterflyNet")
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
                        ["name"] = "Inventory_ButterflyNet"
                    }
                );
                interactManager.ForceInteraction("Map1/Writer");
                saveManager.AddProgress("pigeonCaught", true);
                //Temp
                Destroy(gameObject);
            }
            else if (itemName == "Inventory_ButterflyNet_torn")
            {
                interactManager.Effects(
                    new JObject
                    {
                        ["type"] = "thought",
                        ["thought"] = "찢어진 잠자리채로는 비둘기를 잡을 수 없다."
                    }
                );
            }
            else if (itemName == "Inventory_FishNet")
            {
                interactManager.Effects(
                    new JObject
                    {
                        ["type"] = "thought",
                        ["thought"] = "그물을 던질만큼 가까운 거리까지 가면 비둘기가 도망가 버릴 것 같다."
                    }
                );
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
