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
        
        private float secondFootprintDistance = 0.15f;
        private float secondFootprintInterval = 0.05f;
        private float footprintInterval = 0.6f;
        private float footprintIntervalMin = 0.3f;
        private float footprintIntervalMax = 1.5f;
        private float footprintLasting = 20f;
        private float sprintOutOfSight = 5f;
        private float distanceFromPlayerCriteria = 10f;
        private IEnumerator WalkingMotion()
        {
            interactManager.Effects(
                new JObject
                {
                    ["type"] = "thought",
                    ["thought"] = "누군가 밖으로 나온다. 숨어있어야 한다!"
                }
            );

            interactManager.Effects(
                new JObject
                {
                    ["type"] = "variation",
                    ["target"] = "Map1/House_Gathering",
                    ["parameters"] = new JArray { "OpenDoor" }
                }
            );

            playerCTRL.CanPlayerMove(false);

            var spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.enabled = true;
            spriteRenderer.sortingOrder = 4;

            yield return new WaitForSeconds(0.5f);

            interactManager.Effects(
                new JObject
                {
                    ["type"] = "variation",
                    ["target"] = "Map1/House_Gathering",
                    ["parameters"] = new JArray { "CloseDoor" }
                }
            );

            // -------------------------
            // GuideToCavePathPrefab 로드
            // -------------------------

            var pathHandle =
                Addressables.LoadAssetAsync<GameObject>("GuideToCavePathPrefab");

            yield return pathHandle;

            if (pathHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("GuideToCavePathPrefab 로드 실패");
                yield break;
            }

            GameObject pathObj = pathHandle.Result;


            GameObject colliderChild =
                transform.GetChild(0).gameObject;

            colliderChild.SetActive(false);


            // -------------------------
            // FootprintPrefab 로드
            // -------------------------

            var footprintHandle =
                Addressables.LoadAssetAsync<GameObject>("FootprintPrefab");

            yield return footprintHandle;

            if (footprintHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError("FootprintPrefab 로드 실패");

                Addressables.Release(pathHandle);

                yield break;
            }

            GameObject footprintPrefab = footprintHandle.Result;


            float footprintTimer = 0f;

            Vector3 positionDiff =
                new Vector3(0, -0.5f, 0);

            


            float shortenedTime = 0f;
            foreach (Transform child in pathObj.transform)
            {
                Vector3 targetP = child.localPosition;

                while (Vector3.Distance(transform.position,targetP) > 0.1f)
                {
                    float modifier = 1f;
                    if (Vector3.Distance(transform.position,playerCTRL.transform.position) > distanceFromPlayerCriteria)
                    {
                        modifier = sprintOutOfSight;
                    }
                    transform.position =
                        Vector3.MoveTowards(
                            transform.position,
                            targetP,
                            walkingSpeed * Time.deltaTime * modifier
                        );

                    footprintTimer += Time.deltaTime;

                    if (footprintTimer >= footprintInterval / modifier)
                    {
                        Vector2 direction =
                            ((Vector2)targetP -
                            (Vector2)transform.position).normalized;

                        float angle =
                            Mathf.Atan2(
                                direction.y,
                                direction.x
                            ) * Mathf.Rad2Deg+ 90f;
                        Vector3 secondPositionDiff =
                            new Vector3(direction.y, direction.x * (1-Random.Range(0,2)*2),0) * secondFootprintDistance;

                        GameObject footprint =
                            Instantiate(
                                footprintPrefab,
                                transform.position + positionDiff+secondPositionDiff,
                                Quaternion.Euler(0f, 0f, angle)
                            );

                        footprintTimer = 0f;
                        shortenedTime += footprintInterval * (1-(1/modifier));
                        if(footprintInterval == secondFootprintInterval) footprintInterval = Random.Range(footprintIntervalMin, footprintIntervalMax);
                        else footprintInterval = secondFootprintInterval;
                        FadeObject(
                            footprint,
                            false,
                            footprintLasting+shortenedTime,
                            1f,
                            true
                        );
                    }

                    yield return null;
                }

                transform.position = targetP;
            }


            // Addressables 해제
            Addressables.Release(pathHandle);
            Addressables.Release(footprintHandle);


            colliderChild.SetActive(true);

            spriteRenderer.sortingOrder = -1;
            spriteRenderer.enabled = false;

            interactManager.Effects(
                new JObject
                {
                    ["type"]= "variation",
                    ["target"]= "Map1/Player",
                    ["parameters"]= new JArray { 1 }
                }
            );
            interactManager.ForceInteraction("Map1/Player");

            playerCTRL.CanPlayerMove(true);
        }
    }
}