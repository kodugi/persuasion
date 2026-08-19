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
                        if(saveManager.progress.ContainsKey("notePossessed") && (bool)saveManager.progress["notePossessed"] == true) state = 5;
                        else state=4;
                        break;
                    case "PenGiven":
                        state=6;
                        break;
                    case "PigeonDistracted":
                        state=2;
                        break;
                    //case "PigeonRemoved":
                        //state=3;
                        //break;
                    case "NeedNote":
                        state=1;
                        break;
                }
            }
            base.variation();
        }
    }
}