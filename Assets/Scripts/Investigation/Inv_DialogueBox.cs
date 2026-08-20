using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

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
        [SerializeField] private GameObject titleObj;
        [SerializeField] private GameObject descriptionObj;
        [SerializeField] private GameObject buttonsObj;
        [SerializeField] private GameObject charactersObj;
        List<AsyncOperationHandle<Sprite>> handles = new List<AsyncOperationHandle<Sprite>>();
        bool ignoreNextDialogueMovement=false;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void Initialize()
        {
            ChangeTitle(data["title"].ToString());
            for(int i = 0; i < charactersObj.transform.childCount; i++)
            {
                ChangeImage("", i);
            }
            if (data.ContainsKey("LD_images"))
            {
                List<string> images = data["LD_images"].ToObject<List<string>>();
                for(int i = 0; i < images.Count; i++)
                {
                    ChangeImage(images[i], i);
                }
            }
            DisplayDialogue(0);
        }
        public void ChangeImage(string img_name, int position)
        {
            Transform placeHolder = charactersObj.transform.GetChild(position);
            if (img_name == "")
            {
                placeHolder.GetComponent<Image>().enabled = false;
            }
            else{
                placeHolder.GetComponent<Image>().enabled = false;
                SetSpriteImage<Image>(placeHolder.gameObject, img_name, handles);
                placeHolder.GetComponent<Image>().enabled = true;
            }
        }
        public void ChangeTitle(string title)
        {
            titleObj.transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = title;
            titleObj.SetActive(title!="");
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
        public void DisplayDialogue(int index, bool ignoreNext=false)
        {
            if (ignoreNextDialogueMovement)
            {
                ignoreNextDialogueMovement = false;
                return;
            }
            ignoreNextDialogueMovement = ignoreNext;
            JObject dialogue = (JObject)data["path"][index];
            descriptionObj.GetComponent<TMPro.TextMeshProUGUI>().text = dialogue["description"].ToString();
            if(buttonsObj.transform.childCount > 0)
            {
                foreach(Transform child in buttonsObj.transform)
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
                    GameObject button = Instantiate(buttonPrefab, buttonsObj.transform);
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