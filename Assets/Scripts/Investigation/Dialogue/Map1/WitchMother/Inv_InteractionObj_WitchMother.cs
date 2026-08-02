using UnityEngine;
using System.Collections.Generic;

namespace Investigation
{
public class Inv_InteractionObj_WitchMother: Inv_InteractionObj
    {
        Inv_PlayerCTRL playerCTRL;
        Inv_Interact interactManager;
        bool amIInteracting = false;
        
        override protected void Starter()
        {
            playerCTRL = GameObject.FindFirstObjectByType<Inv_PlayerCTRL>();
            interactManager = GameObject.FindFirstObjectByType<Inv_Interact>();
        }
        override public void CheckState()
        {
            base.CheckState();
            //print(state);
            if (state==0)
            {
                gameObject.transform.GetChild(0).GetComponent<BoxCollider2D>().enabled=false;
                return;
            }
            else if(state==1 || state == 2)
            {
                if (playerCTRL.isHiding)
                {
                    state = 2;
                }
                else state=1;
            }
            else if (state == 3 || state ==4)
            {
                object possessed = saveManager.LoadProgress("possessedWitchsCloth");
                if (possessed is bool b && b == true)
                {
                    state = 5;
                }
            }
            saveManager.AddProgress(obj_name + "state", state);
            if(amIInteracting) interactManager.ForceInteraction(obj_name);
        }
        virtual public void StartInteraction()
        {
            amIInteracting = true;
            base.StartInteraction();
        }
        override public void EndInteraction()
        {
            amIInteracting = false;
            base.EndInteraction();
        }
        override public void variation(List<string> parameters)
        {
            foreach(string parameter in parameters)
            {
                switch (parameter)
                {
                    case "CreateCollider":
                        state=1;
                        gameObject.transform.GetChild(0).GetComponent<BoxCollider2D>().enabled=true;
                        break;
                    case "requested":
                        state=4;
                        break;
                    case "Accepted":
                        state=6;
                        break;
                    case "alone":
                        state=3;
                        gameObject.GetComponent<BoxCollider2D>().size = new Vector2(0.4f, 1f);
                        break;
                }
            }
            base.variation();
        }
    }
}