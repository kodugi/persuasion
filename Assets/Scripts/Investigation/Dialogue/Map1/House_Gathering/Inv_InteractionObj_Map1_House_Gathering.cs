using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Investigation
{
    public class Inv_InteractionObj_Map1_House_Gathering: Inv_InteractionObj
    {
        override public void variation(List<string> parameters = null)
        {
            foreach(string parameter in parameters)
            {
                switch (parameter)
                {
                    case "EnterDoor":
                        EnterDoor();
                        break;
                    case "OpenDoor":
                        OpenDoor();
                        break;
                    case "CloseDoor":
                        CloseDoor();
                        break;
                    case "Gathered":
                        state = 1;
                        break;
                    case "dispersed":
                        state = 2;
                        break;
                }
            }
            base.variation();
        }
        public void EnterDoor()
        {
            StartCoroutine(EnteringDoor());
        }
        private IEnumerator EnteringDoor()
        {
            OpenDoor();
            yield return new WaitForSeconds(1);
            CloseDoor();
        }
        public void OpenDoor()
        {
            print("DoorOpen");
        }
        public void CloseDoor()
        {
            print("DoorClosed");
        }
    }
}