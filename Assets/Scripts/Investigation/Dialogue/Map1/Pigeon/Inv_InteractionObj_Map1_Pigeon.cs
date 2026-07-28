using UnityEngine;
using System.Collections.Generic;

namespace Investigation
{
public class Inv_InteractionObj_Map1_Pigeon: Inv_InteractionObj
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
    }
}