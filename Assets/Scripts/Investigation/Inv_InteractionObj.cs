using UnityEngine;
using System.Collections.Generic;
using System;

namespace Investigation
{
public class Inv_InteractionObj : Utility
    {
        public string obj_name;
        public float hideCriteria;
        public int state=0;
        protected SaveManager saveManager;
        public bool manuallyTouchable = true;
        void Start()
        {
            obj_name = gameObject.name;
            saveManager = GameObject.FindFirstObjectByType<SaveManager>();
            /*if (saveManager != null)
            {
                CheckState();
            }*/
            Starter();
        }
        virtual public void StartInteraction()
        {
            if (saveManager != null)
            {
                CheckState();
            }
        }
        virtual public void EndInteraction()
        {
            
        }
        virtual public void CheckState()
        {
            //string dbg = obj_name;
            if (saveManager == null)
            {
                //dbg+="not at all";
                return;
            }

            if (saveManager.progress != null && saveManager.progress.ContainsKey(obj_name + "state"))
            {
                state = Convert.ToInt32(saveManager.progress[obj_name + "state"]);
                //dbg+="yes, "+state;
            }
            else
            {
                state = 0;
                //dbg+="no";
            }
            //print(dbg);
        }
        virtual protected void Starter()
        {
            
        }
        virtual public void variation(List<string> parameters=null)
        {
            if (saveManager != null)
            {
                saveManager.AddProgress(obj_name + "state", state);
            }
        }
        virtual public void InventoryItemDraggedOn(string itemName)
        {
            print("hey!"+itemName);
        }
    }
}