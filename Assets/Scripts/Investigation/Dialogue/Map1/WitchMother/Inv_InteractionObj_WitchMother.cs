using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Investigation
{
public class Inv_InteractionObj_WitchMother: Inv_InteractionObj
    {
        Inv_PlayerCTRL playerCTRL;
        Inv_Interact interactManager;
        bool amIInteracting = false;
        bool haveWarned = false;
        float walkingSpeed = 1.5f;
        bool isPlayerInside = false;
        
        override protected void Starter()
        {
            playerCTRL = GameObject.FindFirstObjectByType<Inv_PlayerCTRL>();
            interactManager = GameObject.FindFirstObjectByType<Inv_Interact>();
        }
        override public void CheckState()
        {
            if(playerCTRL==null) playerCTRL = GameObject.FindFirstObjectByType<Inv_PlayerCTRL>();
            if(interactManager==null) interactManager = GameObject.FindFirstObjectByType<Inv_Interact>();
            int original_state = state;
            base.CheckState();
            //print(state);
            if (state==0)
            {
                gameObject.transform.GetChild(0).GetComponent<BoxCollider2D>().enabled=false;
                return;
            }
            else if(state==1 || state == 2)
            {
                if (playerCTRL.isHiding)
                {
                    state = 2;
                }
                else state=1;
            }
            else if (state ==3)
            {
                object possessed = saveManager.LoadProgress("possessedWitchsCloth");
                bool isPossessed = possessed switch
                {
                    bool b => b,
                    JValue j => j.Value<bool>(),
                    _ => false
                };
                if (isPossessed)
                {
                    state = 4;
                }
            }
            saveManager.AddProgress(obj_name + "state", state);
            if(amIInteracting && original_state != state && !(state==1 && !haveWarned))
            {
                interactManager.ForceInteraction(obj_name);
            }
        }
        override public string StartInteraction()
        {
            amIInteracting = true;
            return base.StartInteraction();
        }
        override public void EndInteraction()
        {
            amIInteracting = false;
            base.EndInteraction();
        }
        override public void variation(List<string> parameters)
        {
            foreach(string parameter in parameters)
            {
                switch (parameter)
                {
                    case "CreateCollider":
                        state=1;
                        gameObject.transform.GetChild(0).GetComponent<BoxCollider2D>().enabled=true;
                        break;
                    case "Persuade":
                        state = 5;
                        break;
                    case "Accepted":
                        // 여자 애니메이션
                        interactManager.Effects(
                            new JObject
                            {
                                ["type"]="variation",
                                ["target"]="Map1/Dream_Trigger",
                                ["parameters"]=new JArray{1}
                            }
                        );
                        interactManager.Effects(
                            new JObject
                            {
                                ["type"]="variation",
                                ["target"]="Map1/Player",
                                ["parameters"]=new JArray{3}
                            }
                        );
                        state=6;
                        StartCoroutine(WalkingMotion());
                        break;
                    case "alone":
                        state=3;
                        gameObject.GetComponent<BoxCollider2D>().size = new Vector2(0.4f, 1f);
                        gameObject.transform.GetChild(0).GetComponent<BoxCollider2D>().enabled=false;
                        break;
                    case "Dispersed":
                        FadeSwitch(2, 0, 0, 0f);
                        break;
                    case "haveWarned":
                        haveWarned = true;
                        break;
                }
            }
            base.variation();
        }
        private IEnumerator WalkingMotion()
        {
            playerCTRL.CanPlayerMove(false);

            var pathHandle =
                Addressables.LoadAssetAsync<GameObject>("Map1_WitchMotherPathPrefab");

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

            foreach (Transform child in pathObj.transform)
            {
                Vector3 targetP = child.localPosition;

                while (Vector3.Distance(transform.position,targetP) > 0.1f)
                {
                    transform.position =
                        Vector3.MoveTowards(
                            transform.position,
                            targetP,
                            walkingSpeed * Time.deltaTime
                        );

                    yield return null;
                }
                transform.position = targetP;
            }
            // Addressables 해제
            Destroy(pathObj);
            Addressables.Release(pathHandle);
            gameObject.SetActive(false);

            playerCTRL.CanPlayerMove(true);
        }
        void OnTriggerEnter2D(Collider2D col)
        {
            if (state==1 && col.CompareTag("Player"))
            {
                isPlayerInside = true;
                playerCTRL = col.gameObject.GetComponent<Inv_PlayerCTRL>();
                if(!haveWarned)
                {
                    interactManager.Effects(
                        new JObject
                        {
                            ["type"]="changeCamera",
                            ["target"]="Map1/WitchMother",
                            ["duration"]=1
                        }
                    );
                    interactManager.Effects(
                        new JObject
                        {
                            ["type"]="variation",
                            ["target"]="Map1/Player",
                            ["parameters"]=new JArray{4}
                        }
                    );
                    interactionManager.ForceInteraction("Map1/Player");
                }
                else
                {
                    interactManager.ForceInteraction("Map1/WitchMother");
                }
            }
        }
        void OnTriggerExit2D(Collider2D col)
        {
            if (state==1 && col.CompareTag("Player"))
            {
                isPlayerInside = false;
            }
        }
        void FixedUpdate()
        {
            if (state==1 && isPlayerInside)
            {
                if (playerCTRL.isHiding)
                {
                    state=2;
                    interactManager.ForceInteraction("Map1/WitchMother");
                    interactManager.Effects(
                        new JObject
                        {
                            ["type"]="changeCamera",
                            ["target"]="Map1/WitchMother",
                            ["duration"]=1
                        }
                    );
                }
            }
        }
    

    }
}