using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Investigation
{
public class Inv_InteractionObj_Map1_Dream_Trigger: Inv_InteractionObj
    {
        private Inv_Interact interactManager;
        override protected void Starter()
        {
            interactManager = GameObject.FindFirstObjectByType<Inv_Interact>();
        }
        override public void CheckState()
        {
            base.CheckState();
            GetComponent<BoxCollider2D>().enabled = state==1;
        }
        override public void variation(List<string> parameters=null)
        {
            state = int.Parse(parameters[0]);
            base.variation();
            CheckState();
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player") && state == 1) {
                interactManager.Effects(
                    new JObject
                    {
                        ["type"]="variation",
                        ["target"]="Map1/Player",
                        ["parameters"]=new JArray{6}
                    }
                );
                interactManager.Effects(
                    new JObject
                    {
                        ["type"]="variation",
                        ["target"]="Map1/Dream_Trigger",
                        ["parameters"]=new JArray{2}
                    }
                );
                interactManager.Effects(
                    new JObject
                    {
                        ["type"]="cutScene",
                        ["title"]="IntoDream"
                    }
                );
            }
        }
    }
}