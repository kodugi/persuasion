using UnityEngine;
using System.Collections.Generic;

namespace Investigation
{
public class InteractionObj_Map1_Player: InteractionObj
    {
        override public void variation(List<string> parameters=null)
        {
            state = int.Parse(parameters[0]);
            base.variation();
        }
    }
}