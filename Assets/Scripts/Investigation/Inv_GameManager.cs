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
        private void Awake()
        {
            //LoadDataFromFile();
            
        }
        private void Start()
        {
            GameObject.FindFirstObjectByType<Canvas>().transform.Find("NoteButton").GetComponent<Button>().onClick.AddListener(ViewNotes);
            SetScene();
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
            }
        }
        string getID()
        {
            return "Map1";
        }
        public void AddNote(string noteName, List<string> contents)
        {
            print("hi");
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
        private void LoadDataFromFile()
        {
            string path = "Assets/notes.json";
            if (System.IO.File.Exists(path))
            {
                string json = System.IO.File.ReadAllText(path);
                notes = JsonUtility.FromJson<Dictionary<string, List<string>>>(json);
            }
        }
        private void OnApplicationQuit()
        {
            //SaveDataToFile();
        }
        private void SaveDataToFile()
        {
            string path = "Assets/notes.json";
            string json = JsonUtility.ToJson(notes, true);
            System.IO.File.WriteAllText(path, json);
        }
    }
}