using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Investigation
{
    public class Inv_GameManager : MonoBehaviour
    {
        private Dictionary<string, List<string>> notes = new Dictionary<string, List<string>>();
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
        private void Awake()
        {
            //LoadDataFromFile();
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
        private void Start()
        {
            GameObject.FindFirstObjectByType<Canvas>().transform.Find("NoteButton").GetComponent<Button>().onClick.AddListener(ViewNotes);
        }
        public GameObject notePanel;
        public GameObject noteTitlePrefab;
        public GameObject noteContentPrefab;
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
    }
}