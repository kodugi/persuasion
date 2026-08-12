using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.EventSystems;

namespace Investigation
{
    public partial class Inv_GameManager
    {
        public InputActions inputAction;
        [SerializeField] private GameObject inventoryPanel;
        private GameObject inventoryContentHolder;
        private CustomScrollRect inventoryOperator;
        [SerializeField] private GameObject inventoryItemPrefab;
        [SerializeField] private GameObject inventoryItemFloatingPrefab;
        List<string> inventoryItems = new List<string>();
        List<AsyncOperationHandle<Sprite>> inventoryHandles = new List<AsyncOperationHandle<Sprite>>();
        private Coroutine panelFading;
        string floatingItemName="";
        void InventoryAwake()
        {
            saveManager.LoadData<List<string>>("inventory", out inventoryItems);
        }
        void InventoryStart()
        {
            inventoryContentHolder = inventoryPanel.transform.Find("Scroll").Find("Viewport").Find("Content").gameObject;
            inventoryOperator = inventoryPanel.transform.Find("Scroll").GetComponent<CustomScrollRect>();
            inventoryOperator.inventoryManager = this;
            PreviewInventory();
        }
        void InventoryOnApplicationQuit()
        {
            /*
            if (saveManager != null && saveManager.resetOnQuit)
            {
                return;
            }

            saveManager.SaveData("inventory", inventoryItems);*/
        }
        void InventoryUpdate()
        {
            if(inputAction.Player.Interact.WasPressedThisFrame())
            {
                if(inventoryPanel.activeSelf) {
                    if(panelFading!=null) {
                        StopCoroutine(panelFading);
                        StopFading(inventoryPanel, 0.5f);
                    }
                    else CloseInventory();
                }
                else {
                    if(panelFading!=null) {
                        StopCoroutine(panelFading);
                        StopFading(inventoryPanel, 0.5f);
                    }
                    OpenInventory();
                }
            }
        }
        Vector2 InventoryItemPosCalc(int index)
        {
            Vector2 pos = new Vector2(0,0);
            return pos;
        }
        void OpenInventory()
        {
            CloseInventory();
            for(int i = 0; i < inventoryItems.Count; i++)
            {
                string item = inventoryItems[i];
                GameObject newItem = Instantiate(inventoryItemPrefab, inventoryContentHolder.transform);
                //RectTransform rt = newItem.GetComponent<RectTransform>();
                //rt.anchoredPosition = InventoryItemPosCalc(i);
                newItem.name = item+"_"+i.ToString();
                newItem.transform.GetChild(0).name = item+"_"+i.ToString();
                SetSpriteImage<Image>(newItem.transform.GetChild(0).gameObject, "Inventory_"+item, inventoryHandles);
            }
            inventoryPanel.SetActive(true);
        }
        void CloseInventory()
        {
            inventoryPanel.SetActive(false);
            if (inventoryContentHolder.transform.childCount > 0)
            {
                foreach (Transform child in inventoryContentHolder.transform)
                {
                    Destroy(child.gameObject);
                }
            }
            ClearHandles(inventoryHandles);
        }
        void PreviewInventory()
        {
            OpenInventory();
            panelFading = FadeObject(inventoryPanel, false, 2f, 2f, false);
            StartCoroutine(ResetPanelFading(4f));
            // delay
            //CloseInventory();
        }
        IEnumerator ResetPanelFading(float time)
        {
            yield return new WaitForSeconds(time);
            if(panelFading != null) panelFading = null;
        }
        public void AddItem(string itemName, bool doPreview=true)
        {            
            bool doQuit = AddItemException(itemName);
            if(doQuit) return;
            inventoryItems.Add(itemName);
            saveManager.SaveData("inventory", inventoryItems);
            if(doPreview) PreviewInventory();
        }
        bool AddItemException(string itemName)
        {
            print("This Shouldn't be Printed");
            switch (itemName)
            {
                case "note":
                    saveManager.AddProgress("notePossessed", true);
                    return true;
                    //break;
                default:
                    return false;
                    //break;
            }
        }
        public void RemoveItem(string itemName, bool doPreview=true)
        {
            inventoryItems.Remove(itemName);
            saveManager.SaveData("inventory", inventoryItems);
            if(doPreview) PreviewInventory();
        }
        public void ItemClicked(int selectionIdx, GameObject selectionObj)
        {
            string item = inventoryItems[selectionIdx];
            GameObject floatingItem = Instantiate(inventoryItemFloatingPrefab, selectionObj.transform.position, Quaternion.identity, FindFirstObjectByType<Canvas>().gameObject.transform);
            floatingItem.name = item+"_"+selectionIdx.ToString()+"_Floating";
            SetSpriteImage<Image>(floatingItem, item, inventoryHandles);
            Inv_FloatItemCTRL obj_inv_FloatItemCTRL = floatingItem.AddComponent<Inv_FloatItemCTRL>();
            obj_inv_FloatItemCTRL.inventoryManager = this;
            obj_inv_FloatItemCTRL.index = selectionIdx;
            floatingItemName = item;
            RemoveItem(item,false);
            OpenInventory();
        }
        public void ItemRelease(int floatingIdx)
        {
            PointerEventData data = new PointerEventData(EventSystem.current);
            data.position = Input.mousePosition;

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(data, results);
            
            int targetObjIdx = -1;
            foreach(var result in results)
            {
                //print("161"+result.gameObject.name);
                if(result.gameObject.tag == "InventoryObj")
                {
                    if(result.gameObject.name.Contains("Float")) continue;
                    if(int.Parse(result.gameObject.name.Substring(result.gameObject.name.LastIndexOf('_')+1))==floatingIdx) continue;
                    targetObjIdx = int.Parse(result.gameObject.name.Substring(result.gameObject.name.LastIndexOf('_')+1));
                }
            }
            AddItem(floatingItemName, false);
            OpenInventory();
            if(targetObjIdx != -1)
            {
                CombinationEffect(floatingIdx, targetObjIdx);
            }

            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D[] hits = Physics2D.OverlapPointAll(mousePos);
            foreach (Collider2D hit in hits)
            {
                //print(hit.gameObject.name);
                if(hit.gameObject.tag == "Inv_Interactable")
                {
                    hit.gameObject.GetComponent<Inv_InteractionObj>().InventoryItemDraggedOn(floatingItemName);
                }
                else if(hit.gameObject.tag == "Player")
                {
                    hit.gameObject.GetComponent<Inv_PlayerCTRL>().InventoryItemDraggedOn(floatingItemName);
                }
            }
        }
        Dictionary<string, string> combinations = new Dictionary<string, string>
        {
            { "Inventory_FishNet&Inventory_ButterflyNet_torn", "Inventory_ButterflyNet" }
        };
        void CombinationEffect(int id1, int id2)
        {
            string newItem = CombinationEffectChecker(inventoryItems[id1], inventoryItems[id2]);
            if(newItem == "") return;
            RemoveItem(inventoryItems[id1], false);
            RemoveItem(inventoryItems[id2], false);
            AddItem(newItem);
            CloseInventory();
            OpenInventory();
        }
        string CombinationEffectChecker(string name1, string name2)
        {
            string combination = name1 + "&" + name2;
            if (combinations.ContainsKey(combination)) return combinations[combination];
            combination = name2 + "&" + name1;
            if (combinations.ContainsKey(combination)) return combinations[combination];
            return "";
        }
    }
}