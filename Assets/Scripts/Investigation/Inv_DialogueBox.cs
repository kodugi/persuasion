using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Generic;

namespace Investigation
{
    public class Inv_DialogueBox : Utility
    {
        public JObject data;
        public Inv_Interact interactionScript;
        public string interactionName;
        [SerializeField] private GameObject buttonPrefab;
        int singleOption_nextIndex = -100;
        List<JObject> effectList = new List<JObject>();
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void Initialize()
        {
            //interactionScript.dialogueScript = this;
            transform.Find("Title").GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = data["title"].ToString();
            transform.Find("Title").gameObject.SetActive(data["title"].ToString()!="");
            DisplayDialogue(0);
        }
        public void ChangeTitle(string title)
        {
            transform.Find("Title").GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = title;
            transform.Find("Title").gameObject.SetActive(title!="");
        }
        void Update()
        {
            if(singleOption_nextIndex == -100) return;
            if(Input.GetMouseButtonDown(0))
            {
                foreach (JObject effect in effectList)
                {
                    interactionScript.Effects(effect);
                }
                if(singleOption_nextIndex == -1)
                {
                    interactionScript.SignalEnding(interactionName);
                    Destroy(gameObject);
                }
                else
                {
                    DisplayDialogue(singleOption_nextIndex);
                }
            }
        }
        void DisplayDialogue(int index)
        {
            JObject dialogue = (JObject)data["path"][index];
            transform.Find("Description").GetComponent<TMPro.TextMeshProUGUI>().text = dialogue["description"].ToString();
            if(transform.Find("Buttons").childCount > 0)
            {
                foreach(Transform child in transform.Find("Buttons"))
                {
                    Destroy(child.gameObject);
                }
            }

            effectList = new List<JObject>();

            if(((JArray)dialogue["buttons"]).Count == 1)
            {
                singleOption_nextIndex = (int)dialogue["buttons"][0]["next"];
                for (int j = 0; j < ((JArray)dialogue["buttons"][0]["effects"]).Count; j++)
                {
                    effectList.Add(
                        (JObject)dialogue["buttons"][0]["effects"][j]
                    );
                }
            }
            else
            {
                singleOption_nextIndex = -100;
                for(int i = 0; i < ((JArray)dialogue["buttons"]).Count; i++)
                {
                    GameObject button = Instantiate(buttonPrefab, transform.Find("Buttons"));
                    button.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = dialogue["buttons"][i]["title"].ToString();
                    button.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -100* i);
                    
                    int nextIndex = (int)dialogue["buttons"][i]["next"];
                    for(int j = 0; j < ((JArray)dialogue["buttons"][i]["effects"]).Count; j++)
                    {
                        JObject effect = (JObject)dialogue["buttons"][i]["effects"][j];
                        button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => interactionScript.Effects(effect));
                    }
                    if(nextIndex == -1)
                    {
                        button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => Destroy(gameObject));
                        button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => interactionScript.SignalEnding(interactionName));
                    }
                    else
                    {
                        button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => DisplayDialogue(nextIndex));
                    }
                }
            }
        }
        void OnDestroy()
        {
            if (interactionScript != null)
            {
                interactionScript.InteractionEnd();
            }
        }
    }   
}