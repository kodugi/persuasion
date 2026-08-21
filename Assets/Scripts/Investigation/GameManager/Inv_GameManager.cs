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
        private SaveManager saveManager;
        private ChiefManager chiefManager;
        private Inv_Interact interactManager;
        private Inv_PlayerCTRL playerCTRL;
        [SerializeField] private GameObject interactablePrefab;
        [SerializeField] private GameObject backgroundPrefab;
        [SerializeField] private GameObject timerObj;
        [SerializeField] CinemachineCamera cam;
        List<AsyncOperationHandle<Sprite>> mapHandles = new List<AsyncOperationHandle<Sprite>>();
        BoxCollider2D footCollider;
        GameObject lastCameraMovementTarget=null;
        
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
            public List<string> image;
            //public bool singleImage;
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
            playerCTRL = GameObject.FindFirstObjectByType<Inv_PlayerCTRL>();
            NoteAwake();
            InventoryAwake();

        }
        private void Start()
        {
            inputAction = new InputActions();
            inputAction.Player.Enable();
            NoteStart();
            InventoryStart();
            StartCoroutine(SetScene());
        }
        private void Update()
        {
            if (!isDialogueOnlyScene)
            {
                InventoryUpdate();
            }
        }
        private void OnApplicationQuit()
        {
            if (
                saveManager == null ||
                SaveManager.IsQuittingAndResetting ||
                playerCTRL == null
            )
            {
                return;
            }

            saveManager.SaveCharacterPosition(getID(),"Player",playerCTRL.gameObject.transform.position);
            NoteOnApplicationQuit();
            InventoryOnApplicationQuit();
        }
        private void OnDestroy()
        {
            if (inputAction != null)
            {
                inputAction.Player.Disable();
                inputAction.Dispose();
                inputAction = null;
            }
        }
        void OnSceneChange()
        {
            ClearHandles(mapHandles);
        }
        IEnumerator SetScene()
        {
            yield return new WaitUntil(() =>
                saveManager != null &&
                saveManager.IsProgressLoaded);
            string currScene = getID();
            print(currScene);

            string json = InvestigationJsonLoader.LoadMap(currScene);
            JObject data = JObject.Parse(json);
            List<InteractableObj> objects = JsonConvert.DeserializeObject<List<InteractableObj>>(data["interactables"].ToString());
            
            bool isPlayerPosSaved = false;
            if(playerCTRL != null)
            {
                footCollider = playerCTRL.gameObject.transform.Find("FootCollider").GetComponent<BoxCollider2D>();
                if(saveManager.TryLoadCharacterPosition(currScene,"Player", out Vector3 savedPositionP))
                {
                    playerCTRL.gameObject.transform.position = savedPositionP;
                    isPlayerPosSaved = true;
                }
            }

            GameObject interactableParent = GameObject.Find("Interactables");
            
            foreach(InteractableObj obj in objects)
            {
                if(!isPlayerPosSaved && obj.title == currScene + "/Player")
                {
                    print("player");
                    playerCTRL.gameObject.transform.position = Vector_2D_to_Vector3(obj.position);
                    isPlayerPosSaved =true;
                }
                GameObject interactable;
                Vector2 position = Vector_2D_to_Vector2(obj.position);
                if(saveManager.TryLoadCharacterPosition(currScene,obj.title, out Vector3 savedPosition))
                {
                    position = savedPosition;
                }

                if(obj.title == "background")
                {
                    interactable = Instantiate(backgroundPrefab,position, Quaternion.identity, interactableParent.transform);
                }
                else
                {
                    interactable = Instantiate(interactablePrefab,position, Quaternion.identity, interactableParent.transform);
                }

                interactable.name = obj.title;
                interactable.GetComponent<SpriteRenderer>().sortingOrder = obj.sortingOrder;

                interactable.transform.localScale = Vector_2D_to_Vector3(obj.size);

                // background의 경우 collider & scripting 필요 없음
                if(obj.title == "background") {
                    if (currScene == "Map_House")
                    {
                        interactable.GetComponent<SpriteRenderer>().color = Color.black;
                    }
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

                if (obj.script == "Hidable")
                {
                    interactable.AddComponent<Inv_InteractionObj_Hidable>();
                    interactable.GetComponent<Inv_InteractionObj_Hidable>().SetName(obj.title);
                }
                else if (!string.IsNullOrEmpty(obj.script))
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
                interactable.GetComponent<Inv_InteractionObj>().state = -1;
                interactable.GetComponent<Inv_InteractionObj>().hideCriteria = obj.hideCriteria;
                interactable.GetComponent<Inv_InteractionObj>().manuallyTouchable = obj.manually_touchable;
                interactable.GetComponent<Inv_InteractionObj>().images = obj.image;
                if (obj.image != null &&obj.image.Count > 0 && obj.image[0] == "fullblack")
                {
                    interactable.GetComponent<SpriteRenderer>().sortingOrder = -200;
                    interactable.GetComponent<SpriteRenderer>().enabled = false;
                }
                //interactable.GetComponent<Inv_InteractionObj>().singleImage = obj.singleImage;
                if (Mathf.Abs(obj.colliderSize.x) + Mathf.Abs(obj.colliderSize.y) > 0.00001f)
                {
                    if(obj.colliderShape =="box"){
                        interactable.GetComponent<Inv_InteractionObj>().hideCriteria = obj.colliderOffset.y-obj.colliderSize.y/2-footCollider.offset.y-footCollider.size.y/2;
                    }
                }


            }

            string automaticDialogue = null;
            bool isDialogueOnlyMap = false;
            if (currScene == "Map_House")
            {
                isDialogueOnlyMap = true;
                if (!chiefManager.HasPendingAutoInteractionOnReturn)
                {
                    automaticDialogue = "Map_House/Cutscene";
                }
                Camera.main.backgroundColor = Color.black;
            }
            else if (currScene == "Map1_Intro")
            {
                isDialogueOnlyMap = true;
                automaticDialogue = "Map1_Intro/Cutscene";
                Camera.main.backgroundColor = Color.black;
            }
            else if (currScene == "Map_Dream")
            {
                automaticDialogue = "Map_Dream/Intro";
                Camera.main.backgroundColor = new Color32(233, 183, 223, 255);
            }

            if (isDialogueOnlyMap)
            {
                isDialogueOnlyScene = true;
                Inv_PlayerCTRL player = FindFirstObjectByType<Inv_PlayerCTRL>();
                player.CanPlayerMove(false);
                player.GetComponent<SpriteRenderer>().enabled = false;
                CloseInventory();
                noteButton.SetActive(false);
                notePanel.SetActive(false);
            }

            if (automaticDialogue != null)
            {
                StartCoroutine(StartAutomaticDialogue(automaticDialogue));
            }
        }
        private bool isDialogueOnlyScene;
        private IEnumerator StartAutomaticDialogue(string interactionName)
        {
            // Inv_Interact initializes its dialogue anchor during Start, so wait one frame.
            yield return null;
            interactManager.ForceInteraction(interactionName);
        }
        public string getID()
        {
            //print(chiefManager.inv_Scene_ID);
            return chiefManager.inv_Scene_ID;
        }
        public void LoadGameScene(
            string id,
            string autoInteractionOnReturn,
            string returnInvestigationScene = null)
        {
            print(id);
            chiefManager.StartPersuasion(id, autoInteractionOnReturn, returnInvestigationScene);
        }
        public void LoadAnotherInvestigationScene(string id, string autoInteractionOnReturn=null)
        {
            print(id);
            chiefManager.StartInvestigation(id);
        }
        public void ForceInteract(string title)
        {
            interactManager.ForceInteraction(title);
        }
        public void ChangeCamera(Transform target, float duration=1f)
        {
            StartCoroutine(MoveCameraTarget(target, duration));
        }
        IEnumerator MoveCameraTarget(Transform target, float duration)
        {
            lastCameraMovementTarget = target.gameObject;
            GameObject cameraTarget = new GameObject();
            Vector3 start = cam.Target.TrackingTarget.position;
            Vector3 end = target.position;

            cameraTarget.transform.position = start;
            cam.Target.TrackingTarget = cameraTarget.transform;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);

                // 시작/끝 부분을 부드럽게
                t = Mathf.SmoothStep(0f, 1f, t);
                end = target.position;
                cameraTarget.transform.position =
                    Vector3.Lerp(start, end, t);

                yield return null;
            }

            cameraTarget.transform.position = end;
            if(lastCameraMovementTarget == target.gameObject) {
                cam.Target.TrackingTarget = target;
                lastCameraMovementTarget = null;
            }
        }
    }
}
