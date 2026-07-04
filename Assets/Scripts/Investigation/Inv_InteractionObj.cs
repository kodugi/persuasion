using UnityEngine;

namespace Investigation
{
public class Inv_InteractionObj : MonoBehaviour
    {
        public string name;
        public int state=0;
        void Start()
        {
            name = gameObject.name;
        }
        virtual public void variation()
        {
            if (state==0) state=1;
        }
    }
}