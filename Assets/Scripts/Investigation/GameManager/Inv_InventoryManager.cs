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
        [SerializeField] private GameObject inventoryItemPrefab;
        List<string> inventoryItems = new List<string>();
        List<AsyncOperationHandle<Sprite>> handles = new List<AsyncOperationHandle<Sprite>>();
        void InventoryAwake()
        {
            inventoryItems = saveManager.LoadData<List<string>>("inventory");
        }
        void InventoryStart()
        {
            inputAction = new InputActions();
            inputAction.Player.Enable();
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
            for(int i = 0; i < inventoryItems.Count; i++)
            {
                string item = inventoryItems[i];
                GameObject newItem = Instantiate(inventoryItemPrefab, inventoryPanel.transform);
                RectTransform rt = newItem.GetComponent<RectTransform>();
                rt.anchoredPosition = InventoryItemPosCalc(i);
                newItem.name = item;
                Addressables.LoadAssetAsync<Sprite>(item).Completed += handle =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        newItem.GetComponent<Image>().sprite = handle.Result;
                        handles.Add(handle);
                    }
                };
            }
            inventoryPanel.SetActive(true);
        }
        void CloseInventory()
        {
            inventoryPanel.SetActive(false);
            if (inventoryPanel.transform.childCount > 0)
            {
                foreach (Transform child in inventoryPanel.transform)
                {
                    Destroy(child.gameObject);
                }
            }
            foreach (var handle in handles)
            {
                if(handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            handles.Clear();
        }
        public void AddItem(string itemName)
        {
            inventoryItems.Add(itemName);
            saveManager.SaveData("inventory", inventoryItems);
            PreviewInventory();
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
            // delay
            //CloseInventory();
        }
    }
}