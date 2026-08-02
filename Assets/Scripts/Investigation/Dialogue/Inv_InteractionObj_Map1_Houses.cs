using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Investigation
{
public class Inv_InteractionObj_Map1_Houses: Inv_InteractionObj
    {
        GameObject faceImg;
        private Inv_GameManager gameManager;
        List<AsyncOperationHandle<Sprite>> handles = new List<AsyncOperationHandle<Sprite>>();
        override protected void Starter()
        {
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
            gameManager.SetSpriteImage<SpriteRenderer>(faceImg, path, handles);
            /*
            faceImg = Instantiate(gameObject, gameObject.transform.position, Quaternion.identity, gameObject.transform);
            Destroy(faceImg.GetComponent<BoxCollider2D>());
            Destroy(faceImg.GetComponent(GetType()));
            Destroy(faceImg.transform.GetChild(0));
            Vector3 originalScale = faceImg.transform.localScale;
            faceImg.transform.localScale = originalScale/2;
            string path = obj_name.Replace("/", "_")+"_Face";
            faceImg.SetActive(false);
            gameManager.SetSpriteImage<SpriteRenderer>(faceImg, path, handles);
            faceImg.GetComponent<SpriteRenderer>().sortingOrder = gameObject.GetComponent<SpriteRenderer>().sortingOrder+1;
            */
        }
        void OnDestroy()
        {
            if(gameObject.name.Contains("Clone")) return;
            gameManager.ClearHandles(handles);
        }
        override public void variation(List<string> parameters = null)
        {
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