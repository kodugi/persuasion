using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Investigation
{
    public partial class Inv_GameManager
    {
        public InputActions inputAction;
        [SerializeField] private GameObject inventoryPanel;
        private GameObject inventoryContentHolder;
        [SerializeField] private GameObject inventoryItemPrefab;
        List<string> inventoryItems = new List<string>();
        List<AsyncOperationHandle<Sprite>> inventoryHandles = new List<AsyncOperationHandle<Sprite>>();

        void InventoryAwake()
        {
            saveManager.LoadData<List<string>>("inventory", out inventoryItems);
        }
        void InventoryStart()
        {
            inventoryContentHolder = inventoryPanel.transform.Find("Scroll").Find("Viewport").Find("Content").gameObject;
            PreviewInventory();
        }
        void CheckInventoryKey()
        {
            if(inputAction.Player.Interact.WasPressedThisFrame())
            {
                if(inventoryPanel.activeSelf) CloseInventory();
                else OpenInventory();
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
                newItem.name = item;
                SetSpriteImage<Image>(newItem, item, inventoryHandles);
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
        public void AddItem(string itemName)
        {            
            bool doQuit = AddItemException(itemName);
            if(doQuit) return;
            inventoryItems.Add(itemName);
            saveManager.SaveData("inventory", inventoryItems);
            PreviewInventory();
        }
        bool AddItemException(string itemName)
        {
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
        public void RemoveItem(string itemName)
        {
            inventoryItems.Remove(itemName);
            saveManager.SaveData("inventory", inventoryItems);
            PreviewInventory();
        }
        void PreviewInventory()
        {
            OpenInventory();
            FadeObject(inventoryPanel, false, 2f, 2f, false);
            // delay
            //CloseInventory();
        }
    }
}