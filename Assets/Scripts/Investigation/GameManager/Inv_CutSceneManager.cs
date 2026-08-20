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
        [SerializeField] GameObject screenReddener;
        GameObject _staringPeople = null;
        GameObject staringPeople{
            get{
                if(_staringPeople == null)
                {
                    _staringPeople = FindFirstObjectByType<Inv_Obj_Staring_People>().gameObject;
                }
                return _staringPeople;
            }
            set
            {
                _staringPeople = value;
            }
        }
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
                    staringPeople.GetComponent<Inv_Obj_Staring_People>().StartRunning();
                    yield return new WaitForSeconds(10f);
                    Destroy(staringPeople);
                    if(staringPeopleHandle.IsValid()) Addressables.Release(staringPeopleHandle);
                    break;


                case "PeopleStaringAfterReceivingPen":
                    bool penPossessed = false;
                    if (saveManager.TryLoadProgress("penPossessed", out object result0))
                    {
                        penPossessed = (bool)result0;
                    }
                    bool notePossessed = false;
                    if (saveManager.TryLoadProgress("notePossessed", out object result1))
                    {
                        notePossessed = (bool)result1;
                    }
                    if(penPossessed && notePossessed)
                    {
                        interactManager.Effects(
                            new JObject
                            {
                                ["type"] = "variation",
                                ["target"] = "Map1/House_Gathering",
                                ["parameters"] = new JArray
                                {
                                    "Gathered"
                                }
                            }
                        );
                        interactManager.Effects(
                            new JObject
                            {
                                ["type"] = "variation",
                                ["target"] = "Map1/Player",
                                ["parameters"] = new JArray
                                {
                                    2
                                }
                            }
                        );
                        interactManager.Effects(
                            new JObject
                            {
                                ["type"] = "variation",
                                ["target"] = "Map1/Road_Running_Trigger",
                                ["parameters"] = new JArray
                                {
                                    1
                                }
                            }
                        );
                        staringPeopleHandle = Addressables.LoadAssetAsync<GameObject>("PeopleStaring");
                        yield return staringPeopleHandle;
                        if (staringPeopleHandle.Status == AsyncOperationStatus.Succeeded)
                        {
                            GameObject obj = staringPeopleHandle.Result;
                            staringPeople = Instantiate(obj);
                            staringPeople.GetComponent<Inv_Obj_Staring_People>().player = playerCTRL.gameObject;
                            staringPeople.GetComponent<Inv_Obj_Staring_People>().manager = this;
                            staringPeople.GetComponent<Inv_Obj_Staring_People>().house_gather = interactManager.FindInteractableObj("Map1/House_Gathering").gameObject;
                        }
                        else
                        {
                            Debug.LogError("Couldn't Load PeopleStaringAsset");
                        }
                    }
                    else
                    {
                        print("note/pen not yet possessed");
                    }
                    break;


                case "IntoDream":
                    print("잠에 들었다.");
                    playerCTRL.CanPlayerMove(false);
                    playerCTRL.gameObject.SetActive(false);
                    playerCTRL.gameObject.transform.position = interactManager.FindInteractableObj("Map1/House_WitchMother").position;
                    yield return FadeScreen(true);
                    yield return FadeScreen(false);
                    interactManager.FindInteractableObj("Map1/House_WitchMother").gameObject.GetComponent<Inv_InteractionObj>().FadeSwitch(0,1,0,0);
                    interactManager.ForceInteraction("Map1/Player");
                    break;


                case "LoadDreamScene":
                    yield return FadeScreen(true);
                    interactManager.Effects(
                        new JObject
                        {
                            ["type"]="anotherMap",
                            ["title"]="Map_Dream"
                        }
                    );
                    break;

                    
                case "GuideLeavesHouse":
                    int hidingTime = 10;
                    interactManager.Effects(
                        new JObject
                        {
                            ["type"] = "thought",
                            ["thought"] = hidingTime.ToString()+"초 안에 숨어야 한다!"
                        }
                    );
                    float timePassed = 0f;
                    timerObj.SetActive(true);
                    while (timePassed < hidingTime)
                    {
                        timePassed += Time.deltaTime;
                        timerObj.transform.GetChild(0).GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = (hidingTime - timePassed).ToString("F2");
                        yield return null;
                    }
                    timerObj.SetActive(false);
                    if (!playerCTRL.isHiding)
                    {
                        // 들키는 연출
                        chiefManager.GameOver("엿듣고 있던 것을 들켜버렸다.");
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

                    
                case "ObservePeopleOnRoad":
                    int movingTime = 5;
                    float timePassed_o = 0f;
                    while (timePassed_o < movingTime)
                    {
                        timePassed_o += Time.deltaTime;
                        yield return null;
                    }
                    interactManager.Effects(
                        new JObject
                        {
                            ["type"] = "variation",
                            ["target"] = "Map1/Player",
                            ["parameters"] = new JArray
                            {
                                7
                            }
                        }
                    );
                    interactManager.ForceInteraction("Map1/Player");
                    break;

                case "Teleport_After_Cave_Interaction":
                    playerCTRL.CanPlayerMove(false);
                    yield return FadeScreen(true);
                    playerCTRL.gameObject.transform.position = new Vector2(0,0);
                    yield return new WaitForSeconds(1f);
                    yield return FadeScreen(false);
                    //playerCTRL.CanPlayerMove(true);
                    interactManager.Effects(
                        new JObject
                        {
                            ["type"]="variation",
                            ["target"]="Map1/Player",
                            ["parameters"]=new JArray{5}
                        }
                    );
                    interactManager.ForceInteraction("Map1/Player");
                    break;
                
                case "screenRed":
                    yield return FadeScreen(true, duration:0.5f, obj:screenReddener, highOpacity:0.3f);
                    yield return FadeScreen(false, duration:0.5f, obj:screenReddener, highOpacity:0.3f);
                    break;
            }
        }
        public Coroutine FadeScreen(bool fadeIn, float delay=0f, float duration=1f, GameObject obj=null, float lowOpacity = 0f, float highOpacity = 1f)
        {
            return StartCoroutine(FadeScreenCoroutine(fadeIn, delay, duration, obj, lowOpacity, highOpacity));
        }
        IEnumerator FadeScreenCoroutine(bool fadeIn, float delay, float duration, GameObject obj, float lowOpacity, float highOpacity){
            if(obj==null) obj = screenHider;
            
            if(fadeIn) obj.SetActive(true);
            yield return FadeObject(obj,fadeIn,delay, duration, false, lowOpacity, highOpacity);
            if(!fadeIn) obj.SetActive(false);
        }
    }
}