using UnityEngine;
using System.Collections.Generic;

namespace Investigation
{
public class Inv_InteractionObj_Map1_Note_On_Ground: Inv_InteractionObj
    {
        override public void CheckState()
        {
            object notePossessed = saveManager.LoadProgress("notePossessed");
            if(notePossessed != null && ((bool)notePossessed)==true)
            {
                Destroy(gameObject);
            }
        }
    }
}