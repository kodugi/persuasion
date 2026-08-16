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
        public void CutScene(string title)
        {
            StartCoroutine(CutSceneProgress(title));
        }
        private IEnumerator CutSceneProgress(string title)
        {
            switch (title)
            {
                case "PeopleRunningAfterReceivingPen":
                    print("사람들이 뛰어다닌대요");
                    yield return new WaitForSeconds(1);
                    interactManager.Effects(
                        new JObject
                        {
                            ["type"] = "thought",
                            ["thought"] = "나를 쳐다보던 사람들이 갑자기 어딘가로 몰려가기 시작했다."
                        }
                    );
                    break;


                case "PeopleStaringAfterReceivingPen":
                    print("사람들이 쳐다본대요");
                    yield return new WaitForSeconds(1);
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
            }
        }
    }
}