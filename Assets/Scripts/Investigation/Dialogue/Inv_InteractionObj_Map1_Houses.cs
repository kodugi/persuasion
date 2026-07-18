using UnityEngine;
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
            gameManager = GameObject.FindFirstObjectByType<Inv_GameManager>().GetComponent<Inv_GameManager>();
            faceImg = Instantiate(gameObject, gameObject.transform.position, Quaternion.identity, gameObject.transform);
            Destroy(faceImg.GetComponent<BoxCollider2D>());
            Destroy(faceImg.GetComponent(GetType()));
            Vector3 originalScale = faceImg.transform.localScale;
            faceImg.transform.localScale = originalScale/2;
            string path = obj_name.Replace("/", "_")+"_Face";
            faceImg.SetActive(false);
            gameManager.SetSpriteImage<SpriteRenderer>(faceImg, path, handles);
            faceImg.GetComponent<SpriteRenderer>().sortingOrder = gameObject.GetComponent<SpriteRenderer>().sortingOrder+1;
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
                        faceImg.SetActive(true);
                        break;
                    case "faceOff":
                        faceImg.SetActive(false);
                        break;
                    case "firstTalkDone":
                        state = 1;
                        break;
                }
            }
            base.variation();
        }
    }
}