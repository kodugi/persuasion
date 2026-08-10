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
        // Update is called once per frame
        void FixedUpdate()
        {
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
                if(collision.GetComponent<Inv_InteractionObj>().manuallyTouchable)
                {
                    interactionScript.QueueInteraction(collision.GetComponent<Inv_InteractionObj>().obj_name, true);
                    layer_consideredObjs.Add(collision.gameObject);
                }
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Inv_Interactable"))
            {
                if(collision.GetComponent<Inv_InteractionObj>().manuallyTouchable)
                {
                    interactionScript.QueueInteraction(collision.GetComponent<Inv_InteractionObj>().obj_name, false);
                    layer_consideredObjs.Remove(collision.gameObject);
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
        }
        public void InventoryItemDraggedOn(string itemName)
        {
            switch (itemName)
            {
                case "WitchesCloth":
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
                            ["name"] = "WitchesCloth"
                        }
                    );
                    break;
            }
        }
    }
}