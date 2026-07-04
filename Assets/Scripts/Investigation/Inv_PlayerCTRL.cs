using UnityEngine;
using UnityEngine.InputSystem;
 
namespace Investigation
{
    public class Inv_PlayerCTRL : MonoBehaviour
    {
        public InputActions inputAction;
        [SerializeField] private float moveSpeed = 5f;
        private Inv_Interact interactionScript;
        void Awake()
        {
            inputAction = new InputActions();
            inputAction.Player.Enable();
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            interactionScript = GetComponent<Inv_Interact>();
        }

        // Update is called once per frame
        void Update()
        {
            Vector2 movementInput = inputAction.Player.Move.ReadValue<Vector2>();
            transform.Translate(new Vector3(movementInput.x, movementInput.y, 0) * Time.deltaTime * moveSpeed);
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Inv_Interactable"))
            {
                interactionScript.QueueInteraction(collision.GetComponent<Inv_InteractionObj>().name, true);
            }
        }
        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Inv_Interactable"))
            {
                interactionScript.QueueInteraction(collision.GetComponent<Inv_InteractionObj>().name, false);
            }
        }
    }
}