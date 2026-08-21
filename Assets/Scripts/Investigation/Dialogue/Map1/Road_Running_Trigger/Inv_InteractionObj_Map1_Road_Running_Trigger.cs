using UnityEngine;
using System.Collections.Generic;

namespace Investigation
{
public class Inv_InteractionObj_Map1_Road_Running_Trigger: Inv_InteractionObj
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
                interactManager.ForceInteraction("Map1/Player");
            }
        }
    }
}