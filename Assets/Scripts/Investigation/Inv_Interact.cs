using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Collections.Generic;

namespace Investigation
{
    public class Inv_Interact : Utility
    {
        [SerializeField] private GameObject dialogueBox;
        [SerializeField] private GameObject interactionGuide;
        [SerializeField] private List<int> anchorPosition;
        [SerializeField] private GameObject interactables;
        [SerializeField] private GameObject previewMap;
        Inv_GameManager manager;
        Inv_DialogueBox dialogueScript;
        Inv_PlayerCTRL playerCTRL;
        private Vector2 anchorPos;
        private List<string> interactionQueue = new List<string>();
        public bool isInteracting = false;
        void Start()
        {
            interactionGuide.SetActive(false);
            anchorPos = new Vector2(anchorPosition[0], anchorPosition[1]);
            manager = GameObject.FindFirstObjectByType<Inv_GameManager>().GetComponent<Inv_GameManager>();
            playerCTRL = GameObject.FindFirstObjectByType<Inv_PlayerCTRL>().GetComponent<Inv_PlayerCTRL>();
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
                if (!isInteracting)
                {
                    interactionGuide.SetActive(true);
                    interactionGuide.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = "Press X to interact";
                }
            }
            else
            {
                interactionGuide.SetActive(false);
            }
        }
        public void ForceInteraction(string name)
        {
            Interact(name);/*
            if (interactionQueue.Contains(name)) Interact(name);
            else Debug.LogWarning("Attempted to force an interaction that was not in the queue: " + name);*/
        }
        private void Interact(string name)
        {
            isInteracting = true;
            InteractionGuideUpdate("off");

            int state = 0;
            if(FindInteractableObj(name) != null)
            {
                Inv_InteractionObj interactingObj = FindInteractableObj(name).GetComponent<Inv_InteractionObj>();
                interactingObj.StartInteraction();
                state = interactingObj.state;
            }
            string path = "Assets/Scripts/Investigation/Dialogue/" + name + "/Dialogue" + state.ToString() + ".json";
            string json = File.ReadAllText(path);
            JObject data = JObject.Parse(json);
            GameObject obj = Instantiate(dialogueBox, GameObject.Find("Canvas").transform);
            obj.GetComponent<RectTransform>().anchoredPosition = anchorPos;
            dialogueScript = obj.GetComponent<Inv_DialogueBox>();
            dialogueScript.interactionScript = this;
            dialogueScript.data = data;
            dialogueScript.Initialize();
        }
        public void InteractionEnd()
        {
            dialogueScript = null;
            isInteracting = false;
            InteractionGuideUpdate();
        }
        Transform FindInteractableObj(string target)
        {
            Transform targetT = null;
            foreach (Transform child in GameObject.Find("Interactables").transform)
            {
                if(child.gameObject.name==target) targetT = child;
            }
            return targetT;
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
                    FindInteractableObj(target).GetComponent<Inv_InteractionObj>().variation(parameters);
                    break;
                case "item":
                    string item = (string)effect["name"];
                    manager.AddItem(item);
                    break;
                case "persuade":
                    previewMap.SetActive(true);
                    previewMap.transform.Find("ProgressButton").GetComponent<Button>().onClick.RemoveAllListeners();
                    previewMap.transform.Find("ProgressButton").GetComponent<Button>().onClick.AddListener(()=>manager.LoadGameScene((string)effect["title"]));
                    break;
                case "delete":
                    string target_deletion = (string)effect["target"];
                    Destroy(FindInteractableObj(target_deletion).gameObject);
                    break;
                case "changeTitle":
                    dialogueScript.ChangeTitle((string)effect["title"]);
                    break;
                case "playSound":
                    // play sound
                    break;
                case "thought":
                    string thought = (string)effect["thought"];
                    playerCTRL.Think(thought);
                    break;
            }
        }
    }
}