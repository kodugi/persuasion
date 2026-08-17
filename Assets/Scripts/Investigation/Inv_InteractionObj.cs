using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Investigation
{
public class Inv_InteractionObj : Utility
    {
        public string obj_name;
        public float hideCriteria;
        private int _state=0;
        public int state
        {
            get
            {
                return _state;
            }
            set
            {
                if(_state != value) SetImage(value);
                _state = value;
            }
        }
        protected SaveManager saveManager;
        public bool manuallyTouchable = true;
        public List<string> images = null;
        public bool singleImage;
        List<AsyncOperationHandle<Sprite>> handles = new List<AsyncOperationHandle<Sprite>>();

        void Start()
        {
            obj_name = gameObject.name;
            saveManager = GameObject.FindFirstObjectByType<SaveManager>();
            /*if (saveManager != null)
            {
                CheckState();
            }*/
            CheckState();
            Starter();
        }
        virtual public string StartInteraction()
        {
            if (saveManager != null)
            {
                CheckState();
            }
            return obj_name;
        }
        virtual public void EndInteraction()
        {
            
        }
        virtual public void CheckState()
        {
            /*
            if(check_name == "")
            {
                check_name = obj_name;
            }*/
            string check_name = obj_name;
            int original_state = state;
            if (saveManager.progress != null && saveManager.progress.ContainsKey(check_name + "state"))
            {
                state = Convert.ToInt32(saveManager.progress[check_name + "state"]);
                //dbg+="yes, "+state;
            }
            else
            {
                state = 0;
                //dbg+="no";
            }
            if(original_state != state)
            {
                SetImage(state);
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
            //print("hey!"+itemName);
        }
        virtual public void SetImage(int stateI)
        {
            if(images == null) return;
            foreach(var handle in handles)
            {
                if(handle.IsValid()) Addressables.Release(handle);
            }
            //if (!string.IsNullOrEmpty(obj.image)) 
            if(images.Count > stateI)
            {
                //print(images[stateI]);
                SetSpriteImage<SpriteRenderer>(gameObject, images[stateI], handles);
            }
            else if(images.Count == 1)//singleImage)
            {
                //print("ww");
                SetSpriteImage<SpriteRenderer>(gameObject, images[0], handles);
            }
            else
            {
                print("No Allocated Image");
            }
        }
        virtual protected void FadeSwitch(int curr_img_id, int change_img_id, float delay, float fadingTime)
        {
            GameObject fadingOut = new GameObject();
            fadingOut.transform.position = gameObject.transform.position;
            fadingOut.AddComponent<SpriteRenderer>();
            SetSpriteImage<SpriteRenderer>(fadingOut, images[curr_img_id]);
            SetImage(change_img_id);
            FadeObject(gameObject, true, delay, fadingTime, false);
            FadeObject(fadingOut, false, delay, fadingTime, true);
        }
    }
}