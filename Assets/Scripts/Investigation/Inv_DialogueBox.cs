using UnityEngine;
using Newtonsoft.Json.Linq;
using System.IO;

namespace Investigation
{
    public class Inv_DialogueBox : MonoBehaviour
    {
        public JObject data;
        public Inv_Interact interactionScript;
        [SerializeField] private GameObject buttonPrefab;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void Initialize()
        {
            transform.Find("Title").GetComponent<TMPro.TextMeshProUGUI>().text = data["title"].ToString();
            DisplayDialogue(0);
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
            for(int i = 0; i < ((JArray)dialogue["buttons"]).Count; i++)
            {
                GameObject button = Instantiate(buttonPrefab, transform.Find("Buttons"));
                button.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = dialogue["buttons"][i]["title"].ToString();
                button.GetComponent<RectTransform>().anchoredPosition = new Vector2(315, 50-50* i);
                
                int nextIndex = (int)dialogue["buttons"][i]["next"];
                for(int j = 0; j < ((JArray)dialogue["buttons"][i]["effects"]).Count; j++)
                {
                    JObject effect = (JObject)dialogue["buttons"][i]["effects"][j];
                    button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => interactionScript.Effects(effect));
                }
                if(nextIndex == -1)
                {
                    button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => Destroy(gameObject));
                }
                else
                {
                    button.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(() => DisplayDialogue(nextIndex));
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