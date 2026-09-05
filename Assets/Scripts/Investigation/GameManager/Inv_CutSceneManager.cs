using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Unity.Cinemachine;

namespace Investigation
{
    public partial class Inv_GameManager : Utility
    {
        [SerializeField] GameObject screenHider;
        [SerializeField] GameObject screenReddener;
        [SerializeField] GameObject peopleStaringPrefab;
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
                        penPossessed = Convert.ToBoolean(result0);
                    }
                    bool notePossessed = false;
                    if (saveManager.TryLoadProgress("notePossessed", out object result1))
                    {
                        notePossessed = Convert.ToBoolean(result1);
                    }
                    if(penPossessed && notePossessed)
                    {
                        // The objective must not depend on the Addressables load below.
                        // In a build that load can fail or complete later, which previously
                        // left this note in the saved note list indefinitely.
                        RemoveNote("목표", "그림 그릴 수 있는 것을 찾아보자.");
                        RemoveNote("목표", "아무 집 문이나 두드려 보자.");

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
                        if (peopleStaringPrefab != null)
                        {
                            staringPeople = Instantiate(peopleStaringPrefab);
                            staringPeople.GetComponent<Inv_Obj_Staring_People>().player = playerCTRL.gameObject;
                            staringPeople.GetComponent<Inv_Obj_Staring_People>().manager = this;
                            staringPeople.GetComponent<Inv_Obj_Staring_People>().house_gather = interactManager.FindInteractableObj("Map1/House_Gathering").gameObject;
                        }
                        else
                        {
                            // Fallback for scenes that have not assigned the direct reference yet.
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
                                Debug.LogError($"Couldn't load PeopleStaring: {staringPeopleHandle.OperationException}");
                            }
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
                    interactManager.FindInteractableObj("Map1/House_WitchMother").gameObject.GetComponent<Inv_InteractionObj>().FadeSwitch(0,1,0,0);
                    interactManager.FindInteractableObj("Map1/BlackCover").GetComponent<SpriteRenderer>().enabled = true;
                    interactManager.FindInteractableObj("Map1/BlackCover").GetComponent<SpriteRenderer>().sortingOrder = 200;
                    interactManager.FindInteractableObj("Map1/House_WitchMother").GetComponent<SpriteRenderer>().sortingOrder = 300;
                    playerCTRL.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 400;
                    yield return FadeScreen(false);
                    interactManager.ForceInteraction("Map1/Player");
                    break;


                case "LoadDreamScene":
                    //yield return FadeScreen(true);
                    interactManager.Effects(
                        new JObject
                        {
                            ["type"]="anotherMap",
                            ["title"]="Map_Dream"
                        }
                    );
                    yield return null;
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
                        chiefManager.Inv_GameOver("엿듣고 있던 것을 들켜버렸다.");
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
                    playerCTRL.gameObject.transform.position = new Vector2(-1,11.8f);
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
                
                case "Map1_Cave_Hide":
                    Vector3 caveHidingPos = new Vector3(41.7f, -1.7f, 0);
                    yield return StartCoroutine(MoveSmoothly(caveHidingPos, obj:playerCTRL.gameObject));
                    break;
                case "Map1_Cave_GrannyShowup":
                    Vector3 caveGrannyFinalPos = new Vector3(44f, -1.6f, 0);
                    
                    var caveGrannyPrefabHandle =
                        Addressables.LoadAssetAsync<GameObject>("CaveGrannyPrefab");
                    yield return caveGrannyPrefabHandle;
                    if (caveGrannyPrefabHandle.Status != AsyncOperationStatus.Succeeded)
                    {
                        Debug.LogError("caveGrannyPrefab 로드 실패");
                        Addressables.Release(caveGrannyPrefabHandle);
                        yield break;
                    }
                    GameObject caveGrannyPrefab = caveGrannyPrefabHandle.Result;

                    GameObject grannySD = Instantiate(caveGrannyPrefab,interactManager.FindInteractableObj("Map1/Cave").position, Quaternion.identity);
                    grannySD.GetComponent<Animator>().SetBool("isWalking", true);
                    yield return StartCoroutine(MoveSmoothly(caveGrannyFinalPos, obj:grannySD, duration:4));
                    grannySD.GetComponent<Animator>().SetBool("isWalking", false);
                    yield return new WaitForSeconds(2);

                    break;
                case "Map1_Cave_ShowUp":
                    Vector3 caveShowupPos = new Vector3(43.2f, -3f, 0);
                    yield return StartCoroutine(MoveSmoothly(caveShowupPos, obj:playerCTRL.gameObject));
                    break;


                case "cameraShake":
                    GetComponent<CinemachineImpulseSource>().GenerateImpulse(1f);
                    break;
            }
        }
        public Coroutine FadeScreen(bool fadeIn, float delay=0f, float duration=1f, GameObject obj=null, float lowOpacity = 0f, float highOpacity = 1f)
        {
            return StartCoroutine(FadeScreenCoroutine(fadeIn, delay, duration, obj, lowOpacity, highOpacity));
        }
        IEnumerator FadeScreenCoroutine(bool fadeIn, float delay, float duration, GameObject obj, float lowOpacity, float highOpacity){
            if(obj==null) obj = screenHider;
            
            obj.SetActive(true);
            //if(fadeIn) obj.SetActive(true);
            yield return FadeObject(obj,fadeIn,delay, duration, false, lowOpacity, highOpacity);
            if(!fadeIn) obj.SetActive(false);
        }
    }
}
