using UnityEngine;
using System.Collections.Generic;

namespace Investigation
{
public class Inv_InteractionObj : MonoBehaviour
    {
        public string obj_name;
        public int state=0;
        void Start()
        {
            obj_name = gameObject.name;
        }
        virtual public void variation(List<string> parameters=null)
        {
            if (state==0) state=1;
        }
    }
}