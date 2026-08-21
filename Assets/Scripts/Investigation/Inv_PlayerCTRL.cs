using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
 
namespace Investigation
{
    public class Inv_PlayerCTRL : Utility
    {
        public InputActions inputAction;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private GameObject thoughtObj;
        [SerializeField] GameObject footCollider;
        [SerializeField] AudioClip footstepSound1;
        [SerializeField] AudioClip footstepSound2;
        AudioSource audioSource;
        bool isFootStepLeft;
        private Inv_Interact interactionScript;
        private Vector2 movementInput;
        Rigidbody2D rigidbody_my;
        bool playerCanMove = true;
        Animator animator_my;

        List<GameObject> layer_consideredObjs = new List<GameObject>();
        int layer_maxBehind;
        public bool isHiding = false;
        public bool canHide = false;
        string hidingBehind="";
        bool isThinking = false;
        Queue<string> thoughtQueue = new Queue<string>();
        Coroutine thoughtCoroutine;

        void Awake()
        {
            inputAction = new InputActions();
            inputAction.Player.Enable();
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            interactionScript = GameObject.FindFirstObjectByType<Inv_Interact>().GetComponent<Inv_Interact>();
            rigidbody_my = GetComponent<Rigidbody2D>();
            animator_my = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
        }
        private void OnDestroy()
        {
            if (inputAction != null)
            {
                inputAction.Player.Disable();
                inputAction.Dispose();
                inputAction = null;
                interactionScript.SaveObjPos("Player", gameObject.transform.position);
            }
        }
        Vector2 prevPosition = new Vector2();
        float speedThreshold = 0.1f;
        // Update is called once per frame
        void FixedUpdate()
        {
            Vector2 velocity = (rigidbody_my.position - prevPosition) / Time.fixedDeltaTime;
            float xSpeed = (Mathf.Abs(velocity.x)>=speedThreshold)?(velocity.x/Mathf.Abs(velocity.x)):0;
            float ySpeed = (Mathf.Abs(velocity.y)>=speedThreshold)?(velocity.y/Mathf.Abs(velocity.y)):0;
            animator_my.SetFloat("xSpeed", xSpeed);
            animator_my.SetFloat("ySpeed", ySpeed);
            prevPosition = rigidbody_my.position;

            if (inputAction.Player.Move.ReadValue<Vector2>().sqrMagnitude > 0.01f)
            {
                if(isHiding) {
                    isHiding=false;
                    Think("더이상 숨어 있지 않다.");//hidingBehind+"뒤에서 나왔다.");
                    footCollider.GetComponent<BoxCollider2D>().enabled = true;
                }
            }

            if(!interactionScript.isInteracting && playerCanMove){
                movementInput = inputAction.Player.Move.ReadValue<Vector2>();
                rigidbody_my.MovePosition(rigidbody_my.position +new Vector2(movementInput.x, movementInput.y) * Time.deltaTime * moveSpeed);
            }
            layer_maxBehind = 0;
            int hidingCnt=0;
            foreach(GameObject obj in layer_consideredObjs)
            {
                if(gameObject.transform.position.y <= obj.transform.position.y + obj.GetComponent<Inv_InteractionObj>().hideCriteria)
                {
                    layer_maxBehind = Math.Max(layer_maxBehind, obj.GetComponent<SpriteRenderer>().sortingOrder);
                }
                else if(obj.GetComponent<SpriteRenderer>().color.a > 0.05)
                {
                    hidingCnt++;
                }
            }
            canHide = (hidingCnt)>0;
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = layer_maxBehind+1;
        }
        public bool AlreadyHiding(GameObject obj)
        {
            return (isHiding && obj.GetComponent<Inv_InteractionObj>() is Inv_InteractionObj_Hidable);
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Inv_Interactable"))
            {
                layer_consideredObjs.Add(collision.gameObject);
                if(collision.GetComponent<Inv_InteractionObj>().manuallyTouchable)
                {
                    if(AlreadyHiding(collision.gameObject))
                    {
                        //nothing
                    }
                    else interactionScript.QueueInteraction(collision.GetComponent<Inv_InteractionObj>().obj_name, true);
                    
                }
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Inv_Interactable"))
            {
                layer_consideredObjs.Remove(collision.gameObject);
                if(collision.GetComponent<Inv_InteractionObj>().manuallyTouchable)
                {
                    interactionScript.QueueInteraction(collision.GetComponent<Inv_InteractionObj>().obj_name, false);
                    
                }
            }
        }
        public void Think(string thought)
        {
            thoughtQueue.Enqueue(thought);
            if (thoughtCoroutine == null)
            {
                thoughtCoroutine = StartCoroutine(ThinkC());
            }
        }
        IEnumerator ThinkC()
        {
            while (thoughtQueue.Count > 0)
            {
                isThinking = true;
                string thought = thoughtQueue.Dequeue();
                thoughtObj.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = thought;
                thoughtObj.transform.SetAsLastSibling();
                thoughtObj.SetActive(true);
                yield return new WaitForSeconds(2f);
                thoughtObj.SetActive(false);
            }

            isThinking = false;
            thoughtCoroutine = null;
        }
        public void CanPlayerMove(bool canMove)
        {
            playerCanMove = canMove;
            //print(canMove);
        }
        public void InventoryItemDraggedOn(string itemName)
        {
            switch (itemName)
            {
                case "Inventory_WitchesCloth":
                    interactionScript.Effects(
                        new JObject
                        {
                            ["type"] = "progress",
                            ["key"] = "possessedWitchsCloth",
                            ["value"] = true
                        }
                    );
                    interactionScript.Effects(
                        new JObject
                        {
                            ["type"] = "item_remove",
                            ["name"] = "Inventory_WitchesCloth"
                        }
                    );
                    Think("어린 여자아이의 옷을 입었다.");
                    break;
            }
        }
        public void Hide(string id)
        {
            if(canHide) {
                if(id=="") id = interactionScript.GetLastQueue();
                Vector3 obstacleP = interactionScript.FindInteractableObj(id).position;
                footCollider.GetComponent<BoxCollider2D>().enabled = false;
                StartCoroutine(MoveSmoothly(new Vector3(obstacleP.x, gameObject.transform.position.y, gameObject.transform.position.z)));
                isHiding = true;
                hidingBehind = id;
                Think("숨었다.");//hidingBehind+"뒤에 숨었다.");
            }
            else
            {
                Think("좀 더 몸을 가려야 숨길 수 있을 것 같다.");
            }
        }
        public void PlayWalkingSound()
        {
            if(isFootStepLeft) audioSource.PlayOneShot(footstepSound1);
            else audioSource.PlayOneShot(footstepSound2);
            isFootStepLeft = !isFootStepLeft;
        }
    }
}
