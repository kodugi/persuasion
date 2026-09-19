using UnityEngine;
using System.Collections.Generic;

namespace Investigation
{
public class InteractionObj_Map1_ButterflyNet: InteractionObj
    {
        override public void variation(List<string> parameters = null)
        {
            int to_state = int.Parse(parameters[0]);
            state = to_state;
            base.variation();
        }
        override public void CheckState()
        {
            base.CheckState();
            object bfNetPossessed = saveManager.LoadProgress("bfNetPossessed");
            if(bfNetPossessed != null && ((bool)bfNetPossessed)==true)
            {
                //print("Butterfly net possessed");
                Destroy(gameObject);
            }
        }
    }
}