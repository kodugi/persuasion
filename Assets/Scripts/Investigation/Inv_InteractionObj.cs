using UnityEngine;
using System.Collections.Generic;
using System;

namespace Investigation
{
public class Inv_InteractionObj : Utility
    {
        public string obj_name;
        public int state=0;
        protected SaveManager saveManager;
        void Start()
        {
            obj_name = gameObject.name;
            saveManager = GameObject.Find("SaveManager").GetComponent<SaveManager>();
            CheckState();
            Starter();
        }
        virtual public void StartInteraction()
        {
            CheckState();
        }
        virtual public void CheckState()
        {
            if (saveManager.progress.ContainsKey(obj_name+"state"))
            {
                print(saveManager.progress[obj_name+"state"]);
                state = Convert.ToInt32(saveManager.progress[obj_name + "state"]);
            }
            else
            {
                state = 0;
            }
        }
        virtual protected void Starter()
        {
            
        }
        virtual public void variation(List<string> parameters=null)
        {
            saveManager.AddProgress(obj_name+"state", state);
        }
    }
}