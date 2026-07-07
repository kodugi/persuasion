using UnityEngine;
using System.Collections.Generic;

namespace Investigation
{
public class Inv_Interaction_Believers: Inv_InteractionObj
    {
        override public void variation(List<string> parameters=null)
        {
            if (state==0) state=1;
            base.variation(parameters);
        }
    }
}