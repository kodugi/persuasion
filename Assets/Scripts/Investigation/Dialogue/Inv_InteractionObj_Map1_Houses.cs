using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Investigation
{
public class Inv_InteractionObj_Map1_Houses: Inv_InteractionObj_Hidable
    {
        GameObject faceImg;
        private Inv_GameManager gameManager;
        List<AsyncOperationHandle<Sprite>> handles = new List<AsyncOperationHandle<Sprite>>();
        override protected void Starter()
        {
            if(obj_name == "Map1/House1" && state==0) {
                isHidingMode = false;
                manuallyTouchable = true;
            }
            else isHidingMode = true; // 나중에 대사 생기면 false로하고
            gameManager = FindFirstObjectByType<Inv_GameManager>();

            
            faceImg = new GameObject($"{gameObject.name}_Face");

            faceImg.transform.SetParent(transform, false);
            faceImg.transform.localPosition = Vector3.zero;
            faceImg.transform.localRotation = Quaternion.identity;
            faceImg.transform.localScale = Vector3.one * 0.5f;

            SpriteRenderer originalRenderer = GetComponent<SpriteRenderer>();
            SpriteRenderer faceRenderer = faceImg.AddComponent<SpriteRenderer>();

            faceRenderer.sortingLayerID = originalRenderer.sortingLayerID;
            faceRenderer.sortingOrder = originalRenderer.sortingOrder + 1;

            string path = obj_name.Replace("/", "_") + "_Face";

            faceImg.SetActive(false);
            /*
            gameManager.SetSpriteImage<SpriteRenderer>(faceImg, path, handles);
            */
        }
        void OnDestroy()
        {
            if(gameObject.name.Contains("Clone")) return;
            if(handles != null) gameManager.ClearHandles(handles);
        }
        override public void variation(List<string> parameters = null)
        {
            if(state != 0 && obj_name!="Map1/Cave") isHidingMode = true;
            foreach(string parameter in parameters)
            {
                switch (parameter)
                {
                    case "faceOn":
                        OpenDoor();
                        faceImg.SetActive(true);
                        break;
                    case "faceOff":
                        CloseDoor();
                        faceImg.SetActive(false);
                        break;
                    case "firstTalkDone":
                        state = 1;
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
            
        }
        public void CloseDoor()
        {
            
        }
    }
}