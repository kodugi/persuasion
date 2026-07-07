using UnityEngine;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace Investigation
{
    public class Inv_Interact : MonoBehaviour
    {
        [SerializeField] private GameObject dialogueBox;
        [SerializeField] private Inv_GameManager manager;
        [SerializeField] private GameObject interactionGuide;
        [SerializeField] private List<int> anchorPosition;
        [SerializeField] private GameObject interactables;
        private Vector2 anchorPos;
        private List<string> interactionQueue = new List<string>();
        private bool isInteracting = false;
        void Start()
        {
            interactionGuide.SetActive(false);
            anchorPos = new Vector2(anchorPosition[0], anchorPosition[1]);
            manager = GameObject.Find("GameManager").GetComponent<Inv_GameManager>();
        }
        void Update()
        {
            if (!isInteracting && interactionQueue.Count > 0 && Input.GetKeyDown(KeyCode.X))
            {
                Interact(interactionQueue[interactionQueue.Count - 1]);
            }
        }
        public void QueueInteraction(string name, bool insertion)
        {
            if (insertion) interactionQueue.Add(name);
            else {
                if (interactionQueue.Contains(name)) interactionQueue.Remove(name);
                else Debug.LogWarning("Attempted to remove an interaction that was not in the queue: " + name);
            }
            InteractionGuideUpdate();
        }
        private void InteractionGuideUpdate(string mode = "default")
        {
            bool targetState=true;
            if (mode == "default")
            {
                if (interactionQueue.Count > 0) targetState = true;
                else targetState = false;
            }
            else if (mode == "on") targetState = true;
            else if (mode == "off") targetState = false;
            
            if (targetState)
            {
                interactionGuide.SetActive(true);
                interactionGuide.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = "Press X to interact with " + interactionQueue[interactionQueue.Count - 1];
            }
            else
            {
                interactionGuide.SetActive(false);
            }
        }
        private void Interact(string name)
        {
            isInteracting = true;
            InteractionGuideUpdate("off");

            Inv_InteractionObj interactingObj = interactables.transform.Find(name).GetComponent<Inv_InteractionObj>();
            string path = "Assets/Scripts/Investigation/Dialogue/" + name + "/Dialogue" + interactingObj.state + ".json";
            //interactingObj.variation();
            string json = File.ReadAllText(path);
            JObject data = JObject.Parse(json);
            GameObject obj = Instantiate(dialogueBox, GameObject.Find("Canvas").transform);
            obj.GetComponent<RectTransform>().anchoredPosition = anchorPos;
            Inv_DialogueBox dialogueScript = obj.GetComponent<Inv_DialogueBox>();
            dialogueScript.interactionScript = this;
            dialogueScript.data = data;
            dialogueScript.Initialize();
        }
        public void InteractionEnd()
        {
            isInteracting = false;
            InteractionGuideUpdate();
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
                case "variation":
                    string target = (string)effect["target"];
                    List<string> parameters = JsonConvert.DeserializeObject<List<string>>(effect["parameters"].ToString());
                    GameObject.Find("Interactables").transform.Find(target).GetComponent<Inv_InteractionObj>().variation(parameters);
                    break;
                case "item":
                    string item = (string)effect["name"];
                    manager.AddItem(item);
                    break;
            }
        }
    }
}