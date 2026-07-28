using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Investigation
{
    public class Inv_InteractionObj_Map1_Cave: Inv_InteractionObj
    {
        override public void CheckState()
        {
            switch (state)
            {
                case 1:
                    manuallyTouchable = true;
                    break;
            }
        }
        override public void variation(List<string> parameters = null)
        {
            foreach(string parameter in parameters)
            {
                switch (parameter)
                {
                    case "visible":
                        state=1;
                        manuallyTouchable = true;
                        break;
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