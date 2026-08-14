using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Investigation
{
    public class CustomScrollRect : ScrollRect
    {
        public int selectedObjIdx=-1;
        public Inv_GameManager inventoryManager;

        public override void OnBeginDrag(PointerEventData eventData) { }
        public override void OnDrag(PointerEventData eventData) { }
        public override void OnEndDrag(PointerEventData eventData) { }

        void Update()
        {
            if (!Input.GetMouseButtonDown(0))
            {
                if(selectedObjIdx >=0) selectedObjIdx=-1;
                return;
            }

            PointerEventData data = new PointerEventData(EventSystem.current);
            data.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, results);
            
            if(results.Count <= 0) return;
            GameObject selectedObj = results[0].gameObject;
            if (selectedObj.tag == "InventoryObj")
            {
                //print(selectedObj.name);
                selectedObjIdx = int.Parse(selectedObj.name.Substring(selectedObj.name.LastIndexOf('_')+1));
                inventoryManager.ItemClicked(selectedObjIdx, selectedObj);
            }
        }
    }
}