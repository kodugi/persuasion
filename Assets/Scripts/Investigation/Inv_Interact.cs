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
        Inv_DialogueBox _dialogueScript;
        Inv_DialogueBox dialogueScript
        {
            get
            {
                if (_dialogueScript == null)
                {
                    _dialogueScript = FindFirstObjectByType<Inv_DialogueBox>();
                }
                return _dialogueScript;
            }
            set
            {
                _dialogueScript = value;
            }
        }
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
        public string GetLastQueue()
        {
            for(int i = interactionQueue.Count - 1; i >=0; i--)
            {
                string id = interactionQueue[i];
                if (playerCTRL.AlreadyHiding(FindInteractableObj(id).gameObject))
                {
                    continue;
                }
                return id;
            }
            return null;
        }
        void Update()
        {
            if (!isInteracting && GetLastQueue()!=null && Input.GetKeyDown(KeyCode.X))
            {
                string id = GetLastQueue();
                if(id != null) Interact(id);
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
            string id = GetLastQueue();
            if(id == null) {
                FinishBlinking();
                curr_img = null;
            }
            else {
                GameObject temp_obj = FindInteractableObj(id).gameObject;
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
        }
        public void ForceInteraction(string name)
        {
            Interact(name);/*
            if (interactionQueue.Contains(name)) Interact(name);
            else Debug.LogWarning("Attempted to force an interaction that was not in the queue: " + name);*/
        }
        private void Interact(string name)
        {
            string id = name;
            EndInteraction(true);
            isInteracting = true;
            playerCTRL.CanPlayerMove(false);
            FinishBlinking();
            //InteractionGuideUpdate("off");

            int state = 0;
            if(FindInteractableObj(id) != null)
            {
                Inv_InteractionObj interactingObj = FindInteractableObj(id).GetComponent<Inv_InteractionObj>();
                id = interactingObj.StartInteraction();
                state = interactingObj.state;
                if(id.Contains("Hidable")) state = 0;
            }
            string path = "Assets/Scripts/Investigation/Dialogue/" + id + "/Dialogue" + state.ToString() + ".json";
            string json = File.ReadAllText(path);
            JObject data = JObject.Parse(json);
            GameObject obj = Instantiate(dialogueBox, GameObject.Find("Canvas").transform);
            //obj.GetComponent<RectTransform>().anchoredPosition = anchorPos;
            dialogueScript = obj.GetComponent<Inv_DialogueBox>();
            dialogueScript.interactionName = name;
            dialogueScript.interactionScript = this;
            dialogueScript.data = data;
            dialogueScript.Initialize();
        }
        public void SignalEnding(string name)
        {
            //print("ending"+name);
            Inv_InteractionObj interactingObj = null;
            if(FindInteractableObj(name) != null) interactingObj = FindInteractableObj(name).GetComponent<Inv_InteractionObj>();
            if(interactingObj != null) interactingObj.EndInteraction();
        }
        public void InteractionEnd()
        {
            dialogueScript = null;
            isInteracting = false;
            playerCTRL.CanPlayerMove(true);
            InteractionGuideUpdate();
        }
        public void EndInteraction(bool isStarting=false)
        {
            if (dialogueScript != null && !isStarting) Destroy(dialogueScript.gameObject);
            
            InteractionEnd();
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
        public void SaveObjPos(string obj_name, Vector3 currPos)
        {
            saveManager.SaveCharacterPosition(manager.getID(), obj_name, currPos);
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
                    //print(target);
                    List<string> parameters = JsonConvert.DeserializeObject<List<string>>(effect["parameters"].ToString());
                    if(FindInteractableObj(target) != null) FindInteractableObj(target).GetComponent<Inv_InteractionObj>().variation(parameters);
                    else Debug.LogWarning("Tried to apply variation on a not-existing object: "+target);
                    break;
                case "item":
                    string item = (string)effect["name"];
                    manager.AddItem(item);
                    break;
                case "item_remove":
                    string item_2_remove= (string)effect["name"];
                    manager.RemoveItem(item_2_remove);
                    break;
                case "persuade":
                    string autoInteractionOnReturn = null;
                    string returnInvestigationScene = null;
                    if(effect.ContainsKey("autoReturn")) autoInteractionOnReturn = (string)effect["autoReturn"];
                    if(effect.ContainsKey("returnMap")) returnInvestigationScene = (string)effect["returnMap"];
                    manager.LoadGameScene(
                        (string)effect["title"],
                        autoInteractionOnReturn,
                        returnInvestigationScene);
                    break;
                case "anotherMap":
                    //string autoInteractionOnReturn = null;
                    //if(effect.ContainsKey("autoReturn")) autoInteractionOnReturn = (string)effect["autoReturn"];
                    print("hi");
                    manager.LoadAnotherInvestigationScene((string)effect["title"]);
                    break;
                case "delete":
                    string target_deletion = (string)effect["target"];
                    if(FindInteractableObj(target_deletion) != null) Destroy(FindInteractableObj(target_deletion).gameObject);
                    else Debug.LogWarning("Tried to delete a not-existing object: "+target_deletion);
                    break;
                case "changeTitle":
                    dialogueScript.ChangeTitle((string)effect["title"]);
                    break;
                case "changeImage":
                    dialogueScript.ChangeImage((string)effect["image"], int.Parse((string)effect["position"]));
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
                case "changeMap":
                    ChiefManager.Instance.ChangeInvestigationMap((string)effect["title"]);
                    break;
                case "forceInteraction":
                    string target_interaction = (string)effect["target"];
                    ForceInteraction(target_interaction);
                    break;
                case "hide":
                    playerCTRL.Hide((string)effect["name"]);
                    break;
                case "FinalMap":
                    saveManager.AddGeneralSave("FinalMap", (string)effect["title"]);
                    break;
                case "changeCamera":
                    string camTarget = (string)effect["target"];
                    float camMoveDuration = (float)effect["duration"];
                    if(camTarget == "Player") manager.ChangeCamera(playerCTRL.gameObject.transform,camMoveDuration);
                    else manager.ChangeCamera(FindInteractableObj(camTarget),camMoveDuration);
                    break;
            }
        }
        public void JumpDialogue(int destination)
        {
            dialogueScript.DisplayDialogue(destination, true);
        }
    }
}
