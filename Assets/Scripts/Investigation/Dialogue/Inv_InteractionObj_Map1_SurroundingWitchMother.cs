using UnityEngine;
using System.Collections.Generic;

namespace Investigation
{
public class Inv_InteractionObj_Map1_SurroundingWitchMother: Inv_InteractionObj
    {
        override public void variation(List<string> parameters = null)
        {
            int to_state = int.Parse(parameters[0]);
            state = to_state;
            CheckState();
            base.variation();
        }
        override public void CheckState()
        {
            string temp_obj_name = obj_name;
            obj_name = "Map1/SurroundingWitchMother";
            base.CheckState();
            if(state == 1)
            {
                gameObject.GetComponent<SpriteRenderer>().enabled = true;
            }
            else
            {
                gameObject.GetComponent<SpriteRenderer>().enabled = false;
            }
            obj_name = temp_obj_name;
        }
    }
}