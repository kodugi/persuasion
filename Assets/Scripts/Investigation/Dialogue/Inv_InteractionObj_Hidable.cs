using UnityEngine;
using System.Collections.Generic;

namespace Investigation
{
public class Inv_InteractionObj_Hidable: Inv_InteractionObj
    {
        string naturalName;
        Inv_Interact interactCTRL;
        bool _isHidingMode =true;
        bool original_mt;
        protected bool isHidingMode
        {
            get
            {
                return _isHidingMode;
            }
            set
            {
                _isHidingMode = value;
                if(_isHidingMode) {
                    original_mt = manuallyTouchable;
                    manuallyTouchable = true;
                }
                else
                {
                    manuallyTouchable = original_mt;
                }
            }
        }
        public void SetName(string rawText)
        {
            naturalName = rawText;
        }
        override public string StartInteraction()
        {
            if (saveManager != null)
            {
                CheckState();
            }
            if(isHidingMode) return "Map1/Hidable";
            else return obj_name;
        }
    }
}