using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InventoryMenuController : MonoBehaviour
{
    public Transform inventoryLayoutItemsParent;
    public Sprite lockedSprite;
    
    PlayerBuilding playerBuilding;
    int currentInventoryIndex;
    
    BuildingObject[] allBuildings;

    private void Awake()
    {
        playerBuilding = FindFirstObjectByType<PlayerBuilding>();
        playerBuilding.OnStartBuilding += UpdateIcons;

        playerBuilding.onCurrentBuildingChanged += HighlightInventoryBuilding;
    }

    private void Start()
    {
        UpdateIcons();
    }

    void UpdateIcons()
    {
        allBuildings = GameManager.Instance.unlockedBuildings.ToArray();

        int childCount = inventoryLayoutItemsParent.childCount;
        for (int i = 0; i < childCount - 1; i++)
        {
            transform.GetChild(i).GetChild(0).GetComponent<Image>().sprite = lockedSprite;
        }
        
        if (allBuildings == null || allBuildings.Length == 0)
        {
            return;
        }

        for (int i = 0; i < allBuildings.Length; i++)
        {
            transform.GetChild(allBuildings[i].inventoryIndex).GetChild(0).GetComponent<Image>().sprite = allBuildings[i].buildingSprite;
        }
    }

    void HighlightInventoryBuilding(BuildingObject building)
    {
        if (building == null)
        {
            inventoryLayoutItemsParent.GetChild(currentInventoryIndex).GetChild(0).DOScale(Vector3.one , 0.5f);
            return;
        }
        
        int index = building.inventoryIndex;
        if(currentInventoryIndex != index)
            inventoryLayoutItemsParent.GetChild(currentInventoryIndex).GetChild(0).DOScale(Vector3.one , 0.5f);

        currentInventoryIndex = index;
        inventoryLayoutItemsParent.GetChild(index).GetChild(0).DOScale(Vector3.one * 1.5f, 0.5f);
    }
}
