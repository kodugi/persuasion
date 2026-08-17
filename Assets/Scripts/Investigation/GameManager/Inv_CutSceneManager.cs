using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Investigation
{
    public partial class Inv_GameManager : Utility
    {
        [SerializeField] GameObject screenHider;
        GameObject staringPeople = null;
        AsyncOperationHandle<GameObject> staringPeopleHandle;

        public void CutScene(string title)
        {
            StartCoroutine(CutSceneProgress(title));
        }
        private IEnumerator CutSceneProgress(string title)
        {
            switch (title)
            {
                case "PeopleRunningAfterReceivingPen":
                    //running
                    staringPeople.GetComponent<Inv_Obj_Staring_People>().StartRunning();
                    yield return new WaitForSeconds(10f);
                    Destroy(staringPeople);
                    Addressables.Release(staringPeopleHandle);
                    break;


                case "PeopleStaringAfterReceivingPen":
                    staringPeopleHandle = Addressables.LoadAssetAsync<GameObject>("PeopleStaring");
                    yield return staringPeopleHandle;
                    if (staringPeopleHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        GameObject obj = staringPeopleHandle.Result;
                        staringPeople = Instantiate(obj);
                        staringPeople.GetComponent<Inv_Obj_Staring_People>().player = playerCTRL.gameObject;
                        staringPeople.GetComponent<Inv_Obj_Staring_People>().house_gather = interactManager.FindInteractableObj("Map1/House_Gathering").gameObject;
                    }
                    else
                    {
                        Debug.LogError("Couldn't Load PeopleStaringAsset");
                    }
                    break;


                case "IntoDream":
                    print("잠에 들었다.");
                    chiefManager.StartInvestigation("Dream");
                    yield return null;
                    break;


                case "CrowdAroundWitchMotherDisperse":
                    interactManager.Effects(
                        new JObject
                        {
                            ["type"] = "variation",
                            ["target"] = "Map1/WitchMother",
                            ["parameters"] = new JArray
                            {
                                "Dispersed"
                            }
                        }
                    );
                    print("사람들이 흩어진다.");
                    yield return null;
                    break;

                    
                case "GuideLeavesHouse":
                    interactManager.Effects(
                        new JObject
                        {
                            ["type"] = "thought",
                            ["thought"] = "3초 안에 숨어야 한다!"
                        }
                    );
                    float timePassed = 0f;
                    timerObj.SetActive(true);
                    while (timePassed < 3f)
                    {
                        timePassed += Time.deltaTime;
                        timerObj.transform.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = (3f - timePassed).ToString("F2");
                        yield return null;
                    }
                    timerObj.SetActive(false);
                    if (!playerCTRL.isHiding)
                    {
                        // 들키는 연출
                        chiefManager.GameOver();
                        break;
                    }
                    interactManager.Effects(
                        new JObject
                        {
                            ["type"] = "variation",
                            ["target"] = "Map1/GuideToCave",
                            ["parameters"] = new JArray
                            {
                                "walk"
                            }
                        }
                    );
                    break;
                case "Teleport_After_Cave_Interaction":
                    playerCTRL.CanPlayerMove(false);
                    yield return FadeScreen(true);
                    playerCTRL.gameObject.transform.position = new Vector2(0,0);
                    yield return new WaitForSeconds(1f);
                    yield return FadeScreen(false);
                    playerCTRL.CanPlayerMove(true);
                    break;
            }
        }
        Coroutine FadeScreen(bool fadeIn)
        {
            return StartCoroutine(FadeScreenCoroutine(fadeIn));
        }
        IEnumerator FadeScreenCoroutine(bool fadeIn){
            if(fadeIn) screenHider.SetActive(true);
            yield return FadeObject(screenHider,fadeIn,0f, 1f, false);
            if(!fadeIn) screenHider.SetActive(false);
        }
    }
}