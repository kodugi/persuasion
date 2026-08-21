using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Investigation
{
public class Inv_InteractionObj_Map1_Granny_Sprite: Inv_InteractionObj
    {
        override public void CheckState()
        {
            if(saveManager.TryLoadProgress("Map1/Cavestate", out object result))
            {
                int caveState = (int)result;
                if(caveState == 2) return;
            }
            Destroy(gameObject);
        }
    }
}