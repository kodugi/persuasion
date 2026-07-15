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
        public void SetSpriteImage<T>(GameObject obj, string imagePath, List<AsyncOperationHandle<Sprite>> handles) where T : Component
        {
            Addressables.LoadAssetAsync<Sprite>(imagePath).Completed += handle =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    Sprite sprite = handle.Result;
                    if (typeof(T) == typeof(Image))
                    {
                        Image curr = ((Image)(object)obj.GetComponent<T>());
                        curr.sprite = sprite;
                        Color original = curr.color;
                        curr.color = new Color(original.r,original.g,original.b,1);
                    }
                    else if (typeof(T) == typeof(SpriteRenderer))
                    {
                        SpriteRenderer curr = ((SpriteRenderer)(object)obj.GetComponent<T>());
                        curr.sprite = sprite;
                        Color original = curr.color;
                        curr.color = new Color(original.r,original.g,original.b,1);
                    }
                    handles.Add(handle);
                }
            };
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
                Vector2 position = new Vector2(obj.Position.x, obj.Position.y);

                GameObject interactable;
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

                if(obj.title == "background") continue;

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