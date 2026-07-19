using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Investigation
{
    public partial class Inv_GameManager
    {
        private Dictionary<string, List<string>> notes = new Dictionary<string, List<string>>();
        [SerializeField] private GameObject noteButton;
        [SerializeField] private GameObject notePanel;
        [SerializeField] private GameObject noteTitlePrefab;
        [SerializeField] private GameObject noteContentPrefab;
        void NoteAwake()
        {
            saveManager.LoadData<Dictionary<string, List<string>>>("notes", out notes);
        }
        void NoteStart()
        {
            GameObject.FindFirstObjectByType<Canvas>().transform.Find("NoteButton").GetComponent<Button>().onClick.AddListener(ViewNotes);
        }
        void NoteOnApplicationQuit()
        {
            if (saveManager != null && saveManager.resetOnQuit)
            {
                return;
            }

            saveManager.SaveData("notes", notes);
        }
        public void NoteLock(bool doLock)
        {
            noteButton.SetActive(!doLock);
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
            saveManager.SaveData("notes", notes);
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
    }
}