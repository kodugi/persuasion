using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Generic;

namespace Investigation
{
    public class Inv_Interact : MonoBehaviour
    {
        [SerializeField] private GameObject dialogueBox;
        [SerializeField] private Inv_GameManager manager;
        public void Interact(string name)
        {
            string path = "Assets/Scripts/Investigation/" + name + ".json";
            string json = File.ReadAllText(path);
            JObject data = JObject.Parse(json);
            GameObject obj = Instantiate(dialogueBox, GameObject.Find("Canvas").transform);
            obj.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -100);
            Inv_DialogueBox dialogueScript = obj.GetComponent<Inv_DialogueBox>();
            dialogueScript.interactionScript = this;
            dialogueScript.data = data;
            dialogueScript.Initialize();
        }
        public void Effects(JObject effect)
        {
            string type = (string)effect["type"];
            switch (type)
            {
                case "note":
                    string topic = (string)effect["topic"];
                    List<string> notes = new List<string>();
                    foreach (var note in (JArray)effect["content"]) notes.Add((string)note);
                    manager.AddNote(topic, notes);
                    break;
            }
        }
    }
}