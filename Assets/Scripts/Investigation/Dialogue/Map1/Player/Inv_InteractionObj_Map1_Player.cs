using UnityEngine;
using System.Collections.Generic;

namespace Investigation
{
public class Inv_InteractionObj_Map1_Player: Inv_InteractionObj
    {
        override public void variation(List<string> parameters=null)
        {
            state = int.Parse(parameters[0]);
            base.variation();
        }
        override public void StartInteraction()
        {
            base.StartInteraction();
            print(state);
        }
    }
}