using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Investigation
{
    public partial class Inv_GameManager : Utility
    {
        private SaveManager saveManager;
        [SerializeField] private GameObject interactablePrefab;
        [SerializeField] private GameObject backgroundPrefab;
        List<AsyncOperationHandle<Sprite>> mapHandles = new List<AsyncOperationHandle<Sprite>>();
        
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
            saveManager = GameObject.Find("SaveManager").GetComponent<SaveManager>();
            NoteAwake();
            InventoryAwake();
        }
        private void Start()
        {
            inputAction = new InputActions();
            inputAction.Player.Enable();
            NoteStart();
            InventoryStart();
        }
        private void Update()
        {
            CheckInventoryKey();
        }
        private void OnApplicationQuit()
        {
            NoteOnApplicationQuit();
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
                if(obj.colliderSize.x+obj.colliderSize.y > 0.00001)
                {
                    GameObject interactable_collider=Instantiate(interactable, interactable.transform.position, Quaternion.identity, interactable.transform);
                    Destroy(interactable_collider.GetComponent<SpriteRenderer>());
                    if(obj.colliderShape == "box")
                    {
                        interactable_collider.GetComponent<BoxCollider2D>().size = Vector_2D_to_Vector2(obj.colliderSize);
                        interactable_collider.GetComponent<BoxCollider2D>().offset = Vector_2D_to_Vector2(obj.colliderOffset);
                        interactable_collider.GetComponent<BoxCollider2D>().isTrigger = false;
                    }
                    else // circle
                    {
                        
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


            }
        }
        string getID()
        {
            return "Map1";
        }
        public void LoadGameScene(string id)
        {
            print(id);
        }
    }
}