using UnityEngine;
using System.Collections.Generic;

namespace Investigation
{
public class Inv_InteractionObj_Map1_FishNet: Inv_InteractionObj
    {
        override public void variation(List<string> parameters = null)
        {
            int to_state = int.Parse(parameters[0]);
            state = to_state;
            base.variation();
        }
        override public void CheckState()
        {
            object fishNetPossessed = saveManager.LoadProgress("fishNetPossessed");
            if(fishNetPossessed != null && ((bool)fishNetPossessed)==true)
            {
                Destroy(gameObject);
            }
        }
    }
}