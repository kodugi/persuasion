using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Investigation
{
public class Inv_InteractionObj_Map1_Road_Running_Trigger: Inv_InteractionObj
    {
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
                interactionManager.Effects(
                    new JObject
                    {
                        ["type"]="cutScene",
                        ["title"]="screenRed"
                    }
                );
                interactionManager.ForceInteraction("Map1/Player");
            }
        }
    }
}