using UnityEngine;
using System.Collections.Generic;

namespace Investigation
{
public class InteractionObj_Map1_Guard1_Trigger: InteractionObj
    {
        private InteractionCTRL interactManager;
        override protected void Starter()
        {
            if(state!=0) Destroy(gameObject);
            interactManager = GameObject.FindFirstObjectByType<InteractionCTRL>();
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