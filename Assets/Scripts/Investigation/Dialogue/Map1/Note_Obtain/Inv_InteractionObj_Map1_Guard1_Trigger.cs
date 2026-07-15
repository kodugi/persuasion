using UnityEngine;
using System.Collections.Generic;

namespace Investigation
{
public class Inv_InteractionObj_Map1_Note_Obtain: Inv_InteractionObj
    {
        override public void CheckState()
        {
            object noteLock = saveManager.LoadProgress("noteLock");
            if(noteLock != null && ((bool)noteLock)==false)
            {
                Destroy(gameObject);
            }
        }
    }
}