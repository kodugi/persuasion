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
        private Inv_PlayerCTRL playerCTRL;
        [SerializeField] private GameObject interactablePrefab;
        [SerializeField] private GameObject backgroundPrefab;
        [SerializeField] private GameObject timerObj;
        [Header("Relevant Map BGM")]
        [SerializeField] private AudioClip introBgmClip;
        [SerializeField] private AudioClip dreamBgmClip;
        [SerializeField, Range(0f, 1f)] private float bgmVolume = 0.65f;
        List<AsyncOperationHandle<Sprite>> mapHandles = new List<AsyncOperationHandle<Sprite>>();
        BoxCollider2D footCollider;
        AudioSource bgmAudioSource;
        
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
            public float rotation;
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
            /*
            if (saveManager != null && saveManager.resetOnQuit)
            {
                return;
            }*/
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

            bgmAudioSource?.Stop();
        }
        void OnSceneChange()
        {
            ClearHandles(mapHandles);
        }
        IEnumerator SetScene()
        {
            if(!saveManager.isProgressLoaded) yield return null;
            string currScene = getID();
            PlayRelevantMapBgm(currScene);

            footCollider = playerCTRL.gameObject.transform.Find("FootCollider").GetComponent<BoxCollider2D>();
            if(saveManager.TryLoadCharacterPosition(currScene,"Player", out Vector3 savedPositionP))
            {
                playerCTRL.gameObject.transform.position = savedPositionP;
            }
            else if (currScene == "Map_Dream")
            {
                // Dream_reference marker: left-hand character position.
                playerCTRL.gameObject.transform.position = new Vector3(3.27f, 0.29f, 0f);
            }

            string path = "Assets/Scripts/Investigation/Dialogue/Maps/" + currScene + ".json";
            string json = System.IO.File.ReadAllText(path);
            JObject data = JObject.Parse(json);
            List<InteractableObj> objects = JsonConvert.DeserializeObject<List<InteractableObj>>(data["interactables"].ToString());
            
            GameObject interactableParent = GameObject.Find("Interactables");
            
            foreach(InteractableObj obj in objects)
            {
                GameObject interactable;
                Vector2 position = Vector_2D_to_Vector2(obj.position);
                if(saveManager.TryLoadCharacterPosition(currScene,obj.title, out Vector3 savedPosition))
                {
                    position = savedPosition;
                }

                if(obj.title == "background")
                {
                    interactable = Instantiate(backgroundPrefab,position, Quaternion.Euler(0f, 0f, obj.rotation), interactableParent.transform);
                }
                else
                {
                    interactable = Instantiate(interactablePrefab,position, Quaternion.Euler(0f, 0f, obj.rotation), interactableParent.transform);
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

        private void PlayRelevantMapBgm(string mapId)
        {
            AudioClip targetClip = null;
            switch (mapId)
            {
                case "Map1_Intro":
                    targetClip = introBgmClip;
                    break;
                case "Map_Dream":
                    targetClip = dreamBgmClip;
                    break;
                case "Map_House":
                    bgmAudioSource?.Stop();
                    return;
                default:
                    return;
            }

            if (targetClip == null)
            {
                return;
            }

            bgmAudioSource = gameObject.AddComponent<AudioSource>();
            bgmAudioSource.playOnAwake = false;
            bgmAudioSource.loop = true;
            bgmAudioSource.spatialBlend = 0f;
            bgmAudioSource.clip = targetClip;
            bgmAudioSource.volume = bgmVolume;
            bgmAudioSource.Play();
        }

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
            // A different map has its own spawn point; do not carry over the
            // previous map's world-space player position.
            chiefManager.StartInvestigation(id, false);
        }
        public void ForceInteract(string title)
        {
            interactManager.ForceInteraction(title);
        }
    }
}
