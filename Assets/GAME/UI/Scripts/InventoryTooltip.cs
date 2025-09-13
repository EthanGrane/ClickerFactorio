using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryTooltip : MonoBehaviour
{
    [Header("UI References")]
    public GameObject inventoryTooltipRootObject;
    public GameObject itemPrefab;
    public RectTransform layout;
    public TextMeshProUGUI inventoryNameText;
    
    [Header("Raycast")]
    public LayerMask buildingLayer;

    private readonly List<GameObject> pooledItems = new();
    private GameObject lastGameObjectHit;
    private Inventory lastInventory;

    private void Start()
    {
        HideInventoryTooltip();
    }

    private void Update()
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 25, buildingLayer))
        {
            if (hit.transform.gameObject == lastGameObjectHit)
            {
                ShowInventoryTooltip(lastInventory);
                return;
            }
            
            if (hit.transform.TryGetComponent(out IInventory inv))
            {
                lastGameObjectHit = hit.transform.gameObject;
                lastInventory = inv.GetInventory();
                ShowInventoryTooltip(lastInventory);
            }
        }
        else
        {
            lastGameObjectHit = null;
            HideInventoryTooltip();
        }
    }
    
    private void ShowInventoryTooltip(Inventory inventory)
    {
        inventoryTooltipRootObject.SetActive(true);
        inventoryNameText.text = inventory.inventoryName;

        Slot[] slots = inventory.GetSlots();

        EnsurePoolSize(slots.Length);

        for (int i = 0; i < pooledItems.Count; i++)
        {
            if (i < slots.Length && slots[i]?.resourceItemFilter != null)
            {
                var slot = slots[i];
                var item = pooledItems[i];
                item.SetActive(true);

                var img = item.GetComponent<Image>();
                var txt = item.GetComponentInChildren<TextMeshProUGUI>();

                img.sprite = slot.resourceItemFilter.itemIcon;
                txt.text = $"{slot.GetSlotQuantity()}/{slot.GetSlotSize()}";
            }
            else
            {
                pooledItems[i].SetActive(false);
            }
        }
    }

    private void EnsurePoolSize(int requiredSize)
    {
        while (pooledItems.Count < requiredSize)
        {
            GameObject newItem = Instantiate(itemPrefab, layout);
            pooledItems.Add(newItem);
        }
    }

    public void HideInventoryTooltip()
    {
        inventoryTooltipRootObject.SetActive(false);
    }
}
