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
        private SaveManager saveManager;
        private ChiefManager chiefManager;
        private Inv_Interact interactManager;
        [SerializeField] private GameObject interactablePrefab;
        [SerializeField] private GameObject backgroundPrefab;
        List<AsyncOperationHandle<Sprite>> mapHandles = new List<AsyncOperationHandle<Sprite>>();
        BoxCollider2D footCollider;
        
        private class Vector_2D
        {
            public float x;
            public float y;
        }
        private class InteractableObj
        {
            public string title;
            public Vector_2D position;
            public Vector_2D size;
            public string colliderShape; // circle / box
            public Vector_2D colliderSize;
            public Vector_2D colliderOffset;
            public Vector_2D triggerSize;
            public Vector_2D triggerOffset;
            public float hideCriteria; // y offset (float) from centre
            public int sortingOrder;
            public string image;
            public string script;
            public bool manually_touchable;
        }
        private Vector2 Vector_2D_to_Vector2(Vector_2D v)
        {
            return new Vector2(v.x, v.y);
        }
        private Vector3 Vector_2D_to_Vector3(Vector_2D v)
        {
            return new Vector3(v.x, v.y, 1);
        }
        void Awake()
        {
            saveManager = GameObject.FindFirstObjectByType<SaveManager>();
            chiefManager = GameObject.FindFirstObjectByType<ChiefManager>();
            interactManager = GameObject.FindFirstObjectByType<Inv_Interact>();
            NoteAwake();
            InventoryAwake();

        }
        private void Start()
        {
            inputAction = new InputActions();
            inputAction.Player.Enable();
            NoteStart();
            InventoryStart();
            footCollider = FindFirstObjectByType<Inv_PlayerCTRL>().gameObject.transform.Find("FootCollider").GetComponent<BoxCollider2D>();
            SetScene();
        }
        private void Update()
        {
            InventoryUpdate();
        }
        private void OnApplicationQuit()
        {
            if (saveManager != null && saveManager.resetOnQuit)
            {
                return;
            }

            NoteOnApplicationQuit();
            InventoryOnApplicationQuit();
        }
        void OnSceneChange()
        {
            ClearHandles(mapHandles);
        }
        public void ClearHandles(List<AsyncOperationHandle<Sprite>> handles)
        {
            foreach (var handle in handles)
            {
                if(handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            handles.Clear();
        }
        void SetScene()
        {
            string currScene = getID();
            string path = "Assets/Scripts/Investigation/Dialogue/Maps/" + currScene + ".json";
            string json = System.IO.File.ReadAllText(path);
            JObject data = JObject.Parse(json);
            List<InteractableObj> objects = JsonConvert.DeserializeObject<List<InteractableObj>>(data["interactables"].ToString());
            
            GameObject interactableParent = GameObject.Find("Interactables");
            
            foreach(InteractableObj obj in objects)
            {
                GameObject interactable;
                Vector2 position = Vector_2D_to_Vector2(obj.position);

                if(obj.title == "background")
                {
                    interactable = Instantiate(backgroundPrefab,position, Quaternion.identity, interactableParent.transform);
                }
                else
                {
                    interactable = Instantiate(interactablePrefab,position, Quaternion.identity, interactableParent.transform);
                }

                interactable.name = obj.title;
                if (!string.IsNullOrEmpty(obj.image)) SetSpriteImage<SpriteRenderer>(interactable, obj.image, mapHandles);
                interactable.GetComponent<SpriteRenderer>().sortingOrder = obj.sortingOrder;

                interactable.transform.localScale = Vector_2D_to_Vector3(obj.size);

                // background의 경우 collider & scripting 필요 없음
                if(obj.title == "background") {
                    continue;
                }

                interactable.GetComponent<BoxCollider2D>().size = Vector_2D_to_Vector2(obj.triggerSize);
                interactable.GetComponent<BoxCollider2D>().offset = Vector_2D_to_Vector2(obj.triggerOffset);

                // Create Child for Physical Collider
                if (Mathf.Abs(obj.colliderSize.x) + Mathf.Abs(obj.colliderSize.y) > 0.00001f)
                {
                    GameObject interactableCollider = new GameObject("Collider");
                    interactableCollider.transform.SetParent(interactable.transform);
                    interactableCollider.transform.localPosition = Vector3.zero;
                    interactableCollider.transform.localRotation = Quaternion.identity;
                    interactableCollider.transform.localScale = Vector3.one;

                    if (obj.colliderShape == "box")
                    {
                        BoxCollider2D box = interactableCollider.AddComponent<BoxCollider2D>();
                        box.size = Vector_2D_to_Vector2(obj.colliderSize);
                        box.offset = Vector_2D_to_Vector2(obj.colliderOffset);
                        box.isTrigger = false;
                    }
                }

                if (!string.IsNullOrEmpty(obj.script))
                {
                    System.Type scriptType = System.Type.GetType("Investigation." + obj.script);
                    if (scriptType != null)
                    {
                        interactable.AddComponent(scriptType);
                    }
                    else
                    {
                        Debug.LogWarning("Script not found: " + "Investigation."+obj.script);
                    }
                }
                else
                {
                    interactable.AddComponent<Inv_InteractionObj>();
                }
                interactable.GetComponent<Inv_InteractionObj>().hideCriteria = obj.hideCriteria;
                interactable.GetComponent<Inv_InteractionObj>().manuallyTouchable = obj.manually_touchable;
                if (Mathf.Abs(obj.colliderSize.x) + Mathf.Abs(obj.colliderSize.y) > 0.00001f)
                {
                    if(obj.colliderShape =="box"){
                        interactable.GetComponent<Inv_InteractionObj>().hideCriteria = obj.colliderOffset.y-obj.colliderSize.y/2-footCollider.offset.y-footCollider.size.y/2;
                    }
                }


            }
        }
        string getID()
        {
            return chiefManager.inv_Scene_ID;
        }
        public void LoadGameScene(string id, string autoInteractionOnReturn)
        {
            print(id);
            chiefManager.StartPersuasion(id, autoInteractionOnReturn);
        }
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
                case "IntoDream":
                    print("잠에 들었다.");
                    yield return null;
                    break;
                case "CrowdAroundWitchMotherDisperse":
                    print("사람들이 흩어진다.");
                    yield return null;
                    break;
            }
        }
        public void ForceInteract(string title)
        {
            interactManager.ForceInteraction(title);
        }
    }
}