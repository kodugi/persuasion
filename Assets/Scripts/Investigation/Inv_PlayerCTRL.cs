using UnityEngine;
using UnityEngine.InputSystem;
 
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

        // Update is called once per frame
        void FixedUpdate()
        {
            if(!interactionScript.isInteracting){
                movementInput = inputAction.Player.Move.ReadValue<Vector2>();
                rigidbody_my.MovePosition(rigidbody_my.position +new Vector2(movementInput.x, movementInput.y) * Time.deltaTime * moveSpeed);
            }
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Inv_Interactable"))
            {
                interactionScript.QueueInteraction(collision.GetComponent<Inv_InteractionObj>().obj_name, true);
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Inv_Interactable"))
            {
                interactionScript.QueueInteraction(collision.GetComponent<Inv_InteractionObj>().obj_name, false);
            }
        }
        public void Think(string thought)
        {
            thoughtObj.SetActive(true);
            thoughtObj.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>().text = thought;
            FadeObject(thoughtObj, false, 2f, 2f, false);
        }
    }
}