using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections;
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
        SaveManager saveManager;
        private List<string> interactionQueue = new List<string>();
        public bool isInteracting = false;
        void Start()
        {
            interactionGuide.SetActive(false);
            anchorPos = new Vector2(anchorPosition[0], anchorPosition[1]);
            manager = GameObject.FindFirstObjectByType<Inv_GameManager>().GetComponent<Inv_GameManager>();
            playerCTRL = GameObject.FindFirstObjectByType<Inv_PlayerCTRL>().GetComponent<Inv_PlayerCTRL>();
            saveManager = GameObject.FindFirstObjectByType<SaveManager>().GetComponent<SaveManager>();
        }
        void Update()
        {
            if (!isInteracting && interactionQueue.Count > 0 && Input.GetKeyDown(KeyCode.X))
            {
                Interact(interactionQueue[interactionQueue.Count - 1]);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Object Name</param>
        public void QueueInteraction(string name, bool insertion)
        {
            if (insertion) interactionQueue.Add(name);
            else {
                if (interactionQueue.Contains(name)) interactionQueue.Remove(name);
                else Debug.LogWarning("Attempted to remove an interaction that was not in the queue: " + name);
            }
            InteractionGuideUpdate();
        }

        void FixedUpdate()
        {
            ImageBlink();
        }
        [SerializeField] float blinkDuration = 2f;
        [SerializeField] float minOpacity = 0.2f;
        float blinkCurr = 0;
        SpriteRenderer curr_img=null;
        Color original_Color;
        void ImageBlink()
        {
            if(curr_img == null || isInteracting) return;
            blinkCurr+=Time.deltaTime;
            if(blinkCurr >= blinkDuration) blinkCurr = 0;
            float opacity = (Math.Abs(blinkCurr-(blinkDuration/2))/blinkDuration)*(1-minOpacity)+minOpacity;
            Color new_Color = new Color(original_Color.r, original_Color.g, original_Color.b, opacity);
            curr_img.color = new_Color;
        }
        void FinishBlinking()
        {
            if(curr_img != null) curr_img.color = new Color(original_Color.r, original_Color.g, original_Color.b, 1);
        }
        private void InteractionGuideUpdate(string mode = "default")
        {
            if(interactionQueue.Count <= 0) {
                FinishBlinking();
                curr_img = null;
            }
            else {
                GameObject temp_obj = FindInteractableObj(interactionQueue[interactionQueue.Count-1]).gameObject;
                if (temp_obj.GetComponent<Inv_InteractionObj>().manuallyTouchable)
                {
                    SpriteRenderer temp_img = temp_obj.GetComponent<SpriteRenderer>();
                    if(curr_img != temp_img) {
                        FinishBlinking();
                    }
                    curr_img = temp_img;
                    original_Color = curr_img.color;
                }
            }
            // do we need it?
            /*
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
            }*/
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
            FinishBlinking();
            //InteractionGuideUpdate("off");

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
        public Transform FindInteractableObj(string target)
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
                    if(!((bool)saveManager.LoadProgress("noteLock"))) manager.AddNote(topic, notes);
                    break;
                case "variation":
                    string target = (string)effect["target"];
                    List<string> parameters = JsonConvert.DeserializeObject<List<string>>(effect["parameters"].ToString());
                    if(FindInteractableObj(target) != null) FindInteractableObj(target).GetComponent<Inv_InteractionObj>().variation(parameters);
                    else Debug.LogWarning("Tried to apply variation on a not-existing object: "+target);
                    break;
                case "item":
                    string item = (string)effect["name"];
                    manager.AddItem(item);
                    break;
                case "persuade":
                    string autoInteractionOnReturn = null;
                    if(effect.ContainsKey("autoReturn")) autoInteractionOnReturn = (string)effect["autoReturn"];
                    manager.LoadGameScene((string)effect["title"], autoInteractionOnReturn);
                    /*
                    previewMap.SetActive(true);
                    previewMap.transform.Find("ProgressButton").GetComponent<Button>().onClick.RemoveAllListeners();
                    previewMap.transform.Find("ProgressButton").GetComponent<Button>().onClick.AddListener(()=>manager.LoadGameScene((string)effect["title"]));
                    */
                    break;
                case "delete":
                    string target_deletion = (string)effect["target"];
                    if(FindInteractableObj(target_deletion) != null) Destroy(FindInteractableObj(target_deletion).gameObject);
                    else Debug.LogWarning("Tried to delete a not-existing object: "+target_deletion);
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
                case "progress":
                    string key = (string)effect["key"];
                    object value = effect["value"];
                    saveManager.AddProgress(key, value);
                    break;
                case "cutScene":
                    manager.CutScene((string)effect["title"]);
                    break;
            }
        }
    }
}