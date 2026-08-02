using UnityEngine;
using System.Collections.Generic;

namespace Investigation
{
public class Inv_InteractionObj_Map1_Guard1_Trigger: Inv_InteractionObj
    {
        private Inv_Interact interactManager;
        override protected void Starter()
        {
            if(state!=0) Destroy(gameObject);
            interactManager = GameObject.FindFirstObjectByType<Inv_Interact>();
        }
        override public void variation(List<string> parameters=null)
        {
            state = 1;
            base.variation();
            Destroy(gameObject);
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player") && state == 0) {
                interactManager.ForceInteraction(obj_name);
            }
        }
    }
}