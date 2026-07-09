using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Investigation
{
    public partial class Inv_GameManager : MonoBehaviour
    {
        private SaveManager saveManager;
        void Awake()
        {
            saveManager = GameObject.Find("SaveManager").GetComponent<SaveManager>();
            NoteAwake();
            InventoryAwake();
        }
        private void Start()
        {
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

        void SetScene()
        {
            string currScene = getID();
            string path = "Assets/Scripts/Investigation/Dialogue/Maps/" + currScene + ".json";
            string json = System.IO.File.ReadAllText(path);
            JObject data = JObject.Parse(json);
            List<InteractableObj> objects = JsonConvert.DeserializeObject<List<InteractableObj>>(data["interactables"].ToString());
            foreach(InteractableObj obj in objects)
            {
                GameObject interactableParent = GameObject.Find("Interactables");
                Vector2 position = new Vector2(obj.Position.x, obj.Position.y);
                GameObject interactable = Instantiate(interactablePrefab,position, Quaternion.identity, interactableParent.transform);
                interactable.name = obj.title;/*
                Addressables.LoadAssetAsync<Sprite>(obj.image).Completed += handle =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        interactable.GetComponent<SpriteRenderer>().sprite = handle.Result;
                    }
                };*/
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
        
    }
}