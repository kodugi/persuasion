using UnityEngine;
using System.Collections.Generic;

namespace Investigation
{
public class Inv_InteractionObj_Hidable: Inv_InteractionObj
    {
        string naturalName;
        Inv_Interact interactCTRL;
        public void SetName(string rawText)
        {
            naturalName = rawText;
        }
        override public string StartInteraction()
        {
            if (saveManager != null)
            {
                CheckState();
            }
            return "Map1/Hidable";
        }
    }
}