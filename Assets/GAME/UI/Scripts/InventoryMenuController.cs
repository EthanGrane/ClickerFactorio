using System;
using DG.Tweening;
using UnityEngine;

public class InventoryMenuController : MonoBehaviour
{
    public Transform inventoryLayoutItemsParent;
    
    PlayerBuilding playerBuilding;
    int currentInventoryIndex;

    private void Awake()
    {
        playerBuilding = FindFirstObjectByType<PlayerBuilding>();

        playerBuilding.onCurrentInventoryBuildingChanged += HighlightInventoryBuilding;
    }

    void HighlightInventoryBuilding(int index)
    {
        if(currentInventoryIndex != index)
            inventoryLayoutItemsParent.GetChild(currentInventoryIndex).GetChild(0).DOScale(Vector3.one , 0.5f);

        currentInventoryIndex = index;
        inventoryLayoutItemsParent.GetChild(index).GetChild(0).DOScale(Vector3.one * 1.5f, 0.5f);
    }
}
