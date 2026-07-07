using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Investigation
{
    public class Inv_GameManager : MonoBehaviour
    {
        private Dictionary<string, List<string>> notes = new Dictionary<string, List<string>>();
        public GameObject notePanel;
        public GameObject noteTitlePrefab;
        public GameObject noteContentPrefab;
        public GameObject interactablePrefab;
        private SaveManager saveManager;
        private void Start()
        {
            GameObject.FindFirstObjectByType<Canvas>().transform.Find("NoteButton").GetComponent<Button>().onClick.AddListener(ViewNotes);
            SetScene();
            saveManager = GameObject.Find("SaveManager").GetComponent<SaveManager>();
            notes = saveManager.LoadData<Dictionary<string, List<string>>>("notes");
        }
        private class Position
        {
            public float x;
            public float y;
        }
        private class InteractableObj
        {
            public string title;
            public Position Position;
            public string image;
            public string script;
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
                interactable.name = obj.title;
                //interactable.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(obj.image);
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
        public void AddNote(string noteName, List<string> contents)
        {
            if (!notes.ContainsKey(noteName))
            {
                notes.Add(noteName, contents);
            }
            else
            {
                notes[noteName].AddRange(contents);
            }
        }
        public void ViewNotes()
        {
            notePanel.SetActive(true);
            Transform content = notePanel.transform.Find("Scroll").Find("Viewport").Find("Content");
            if(content.childCount > 0)
            {
                foreach(Transform child in content.transform)
                {
                    Destroy(child.gameObject);
                }
            }
            
            foreach (var kvp in notes)
            {
                GameObject noteTitleItem = Instantiate(noteTitlePrefab, content);
                noteTitleItem.GetComponent<TMPro.TextMeshProUGUI>().text = kvp.Key;
                foreach (var this_content in kvp.Value)
                {
                    GameObject noteContentItem = Instantiate(noteContentPrefab, content);
                    noteContentItem.GetComponent<TMPro.TextMeshProUGUI>().text = this_content;
                }
            }
        }
        private void OnApplicationQuit()
        {
            saveManager.SaveData("notes", notes);
        }
    }
}