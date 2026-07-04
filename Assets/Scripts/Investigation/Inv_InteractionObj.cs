using UnityEngine;

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
        virtual public void variation()
        {
            if (state==0) state=1;
        }
    }
}