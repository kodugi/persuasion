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
        private Inv_Interact interactionScript;
        private Vector2 movementInput;
        Rigidbody2D rigidbody_my;
        bool playerCanMove = true;
        Animator animator_my;

        List<GameObject> layer_consideredObjs = new List<GameObject>();
        int layer_maxBehind;
        public bool isHiding = false;

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
        }
        private void OnDestroy()
        {
            if (inputAction != null)
            {
                inputAction.Player.Disable();
                inputAction.Dispose();
                inputAction = null;
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
            isHiding = (hidingCnt)>0;
            gameObject.GetComponent<SpriteRenderer>().sortingOrder = layer_maxBehind+1;
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Inv_Interactable"))
            {
                layer_consideredObjs.Add(collision.gameObject);
                if(collision.GetComponent<Inv_InteractionObj>().manuallyTouchable)
                {
                    interactionScript.QueueInteraction(collision.GetComponent<Inv_InteractionObj>().obj_name, true);
                    
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
            thoughtObj.SetActive(true);
            thoughtObj.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = thought;
            FadeObject(thoughtObj, false, 2f, 2f, false);
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
                    break;
            }
        }
    }
}