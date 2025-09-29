using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerInventoryUI : MonoBehaviour
{
    public PlayerInventory playerInventory;

    public GameObject inventoryItemPrefab;
    
    GameObject[] inventoryItems;

    public ResourceItem[] resourceItemsOrder;

    private void Awake()
    {
        playerInventory = FindFirstObjectByType<PlayerInventory>();
    }

    private void Start()
    {
        playerInventory.inventory.OnItemAdded += ()=>
        {
            Debug.Log("OnItemAdded");
            UpdateUI();
        };
        playerInventory.inventory.OnItemRemoved += ()=>
        {
            Debug.Log("OnItemRemoved");
            UpdateUI();
        };

        inventoryItems = new GameObject[resourceItemsOrder.Length];
        for (int i = 0; i < resourceItemsOrder.Length; i++)
        {
            inventoryItems[i] = Instantiate(inventoryItemPrefab, transform);
            inventoryItems[i].GetComponentInChildren<Image>().sprite = resourceItemsOrder[i].itemIcon;
        }
    }

    void UpdateUI()
    {
        for (int i = 0; i < inventoryItems.Length; i++)
        {
            ResourceItem resource = resourceItemsOrder[i];

            int count = 0;
            foreach (var slot in playerInventory.inventory.GetSlots())
            {
                if (slot.resourceItemFilter == resource)
                {
                    count = slot.numberOfItems;
                    inventoryItems[i].GetComponentInChildren<TextMeshProUGUI>().text = count.ToString();
                    break;
                }
            }

        }
    }

}
