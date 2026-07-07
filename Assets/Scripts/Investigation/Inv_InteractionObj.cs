using UnityEngine;
using System.Collections.Generic;

namespace Investigation
{
public class Inv_InteractionObj : MonoBehaviour
    {
        public string obj_name;
        public int state=0;
        private SaveManager saveManager;
        void Start()
        {
            obj_name = gameObject.name;
            saveManager = GameObject.Find("SaveManager").GetComponent<SaveManager>();
            CheckState();
        }
        virtual protected void CheckState()
        {
            if (saveManager.progress.ContainsKey(obj_name+"state"))
            {
                print(saveManager.progress[obj_name+"state"]);
                state = (int)(long)saveManager.progress[obj_name+"state"];
            }
            else
            {
                state = 0;
            }
        }
        virtual public void variation(List<string> parameters=null)
        {
            //if (state==0) state=1;
            saveManager.AddProgress(obj_name+"state", state);
        }
    }
}