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
                        state=3;
                        break;
                    case "PenGiven":
                        state=4;
                        break;
                    case "PigeonDistracted":
                        state=1;
                        break;
                    case "PigeonRemoved":
                        state=2;
                        break;
                }
            }
            base.variation();
        }
    }
}