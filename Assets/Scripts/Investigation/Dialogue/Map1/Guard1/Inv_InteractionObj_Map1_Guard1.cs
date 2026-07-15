using UnityEngine;
using System.Collections.Generic;

namespace Investigation
{
public class Inv_InteractionObj_Map1_Guard1: Inv_InteractionObj
    {
        [SerializeField] private float moveSpeed = 12f;
        private Inv_Interact interactManager;
        private bool chasing = false;
        private Transform player;

        override protected void Starter()
        {
            interactManager = GameObject.FindFirstObjectByType<Inv_Interact>();
            player = GameObject.FindFirstObjectByType<Inv_PlayerCTRL>().gameObject.transform;
        }
        void Update()
        {
            if(chasing) transform.position = Vector3.MoveTowards(transform.position,player.position,moveSpeed * Time.deltaTime);
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.CompareTag("Player") && chasing) {
                chasing = false;
                interactManager.ForceInteraction(obj_name);
            }
        }
        override public void variation(List<string> parameters)
        {
            foreach(string parameter in parameters)
            {
                switch (parameter)
                {
                    case "Chase":
                        if(state ==0) chasing = true;
                        break;
                    case "Met":
                        state = 1;
                        break;
                }
            }
            base.variation();
        }
    }
}