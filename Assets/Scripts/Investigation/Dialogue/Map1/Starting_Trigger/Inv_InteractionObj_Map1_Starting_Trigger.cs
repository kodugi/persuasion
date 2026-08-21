using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Investigation
{
public class Inv_InteractionObj_Map1_Starting_Trigger: Inv_InteractionObj
    {
        override public void CheckState()
        {
            base.CheckState();
            GetComponent<BoxCollider2D>().enabled = state==0;
        }
        override public void variation(List<string> parameters=null)
        {
            state = int.Parse(parameters[0]);
            base.variation();
            CheckState();
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player") && state == 0) {
                interactionManager.Effects(
                    new JObject
                    {
                        ["type"]="variation",
                        ["target"]="Map1/Player",
                        ["parameters"]=new JArray{8}
                    }
                );
                interactionManager.ForceInteraction("Map1/Player");
            }
        }
    }
}