using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Investigation
{
    public class Inv_InteractionObj_Map1_Cave: Inv_InteractionObj_Map1_Houses
    {
        override public void CheckState()
        {
            base.CheckState();
            switch (state)
            {
                case 1:
                    manuallyTouchable = true;
                    break;
                case 2: 
                    manuallyTouchable = true;
                    variation(new List<string>(){"faceOn"});
                    break;
                case 3: 
                    manuallyTouchable = true;
                    break;
            }
            base.CheckState();
        }
        override public void variation(List<string> parameters = null)
        {
            List<string> newParam = new List<string>();
            foreach(string parameter in parameters)
            {
                switch (parameter)
                {
                    case "visible":
                        state=1;
                        manuallyTouchable = true;
                        break;
                    case "knock":
                        newParam.Add("faceOn");
                        break;
                    case "Met":
                        state=2;
                        break;
                    case "Resolved":
                        state=3;
                        break;
                }
            }
            base.variation(newParam); // faceOn/Off, DoorCTRL, firstTalkDone(Not Used)
        }
    }
}