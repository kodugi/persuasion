using UnityEngine;
using System.Collections.Generic;

namespace Investigation
{
public class Inv_InteractionObj_Map1_Writer: Inv_InteractionObj
    {
        override public void variation(List<string> parameters=null)
        {
            foreach(string parameter in parameters){
                switch (parameter)
                {
                    case "Met":
                        state=1;
                        break;
                    case "PenGiven":
                        state=2;
                        break;
                }
            }
            base.variation();
        }
    }
}