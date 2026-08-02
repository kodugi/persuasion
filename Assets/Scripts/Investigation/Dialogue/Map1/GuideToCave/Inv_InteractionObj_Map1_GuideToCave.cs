using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Investigation
{
public class Inv_InteractionObj_Map1_GuideToCave: Inv_InteractionObj
    {
        private Inv_Interact interactManager;
        private Inv_PlayerCTRL playerCTRL;
        override protected void Starter()
        {
            interactManager = GameObject.FindFirstObjectByType<Inv_Interact>();
            playerCTRL = GameObject.FindFirstObjectByType<Inv_PlayerCTRL>();
        }
        override public void CheckState()
        {
            base.CheckState();
            switch (state)
            {
                case 0:
                    gameObject.GetComponent<SpriteRenderer>().enabled = false;
                    gameObject.GetComponent<SpriteRenderer>().sortingOrder = -1;
                    break;
            }
        }
        override public void variation(List<string> parameters = null)
        {
            foreach(var parameter in parameters)
            {
                switch (parameter)
                {
                    case "walk":
                        print("walking");
                        StartCoroutine(WalkingMotion());
                        break;
                }
            }
        }
        private float walkingSpeed = 3f;
        private float footprintInterval = 0.3f;
        private IEnumerator WalkingMotion()
        {
            interactManager.Effects(
                new JObject
                {
                    ["type"]="thought",
                    ["thought"]="누군가 밖으로 나온다. 숨어있어야 한다!"
                }
            );
            interactManager.Effects(
                new JObject
                {
                    ["type"]="variation",
                    ["target"]="Map1/House_Gathering",
                    ["parameters"]=new JArray{"OpenDoor"}
                }
            );
            playerCTRL.CanPlayerMove(false);
            gameObject.GetComponent<SpriteRenderer>().enabled = true;
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = 4;
            yield return new WaitForSeconds(0.5f);
            interactManager.Effects(
                new JObject
                {
                    ["type"]="variation",
                    ["target"]="Map1/House_Gathering",
                    ["parameters"]=new JArray{"CloseDoor"}
                }
            );
            //temp
            Vector3 targetP = interactManager.FindInteractableObj("Map1/Cave").position;
            GameObject colliderChild = gameObject.transform.GetChild(0).gameObject;
            colliderChild.SetActive(false);
            
            AsyncOperationHandle<GameObject> footprintHandle = Addressables.LoadAssetAsync<GameObject>("FootprintPrefab");
            yield return footprintHandle;
            GameObject footprintPrefab = footprintHandle.Result;
            float footprintTimer = 0f;
            Vector3 positionDiff = new Vector3(0,-0.5f,0);
            while (Vector3.Distance(transform.position, targetP) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetP,
                    walkingSpeed * Time.deltaTime
                );
                footprintTimer += Time.deltaTime;
                if (footprintTimer >= footprintInterval)
                {
                    Vector2 direction = ((Vector2)targetP - (Vector2)transform.position).normalized;
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    GameObject footprint = Instantiate(
                        footprintPrefab,
                        transform.position+positionDiff,
                        Quaternion.Euler(0f, 0f, angle)
                    );
                    footprintTimer = 0f;
                    FadeObject(footprint, false, 10f, 1f, true);
                }
                yield return null;
            }
            transform.position = targetP;
            Addressables.Release(footprintHandle);

            colliderChild.SetActive(true);
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = -1;
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            playerCTRL.CanPlayerMove(true);
        }
    }
}