using System;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public Inventory inventory;
    PlayerHarvest _playerHarvest;

    private void Awake()
    {
        inventory = new Inventory(32, Int32.MaxValue, "Player Inventory");
        
        _playerHarvest = GetComponentInParent<PlayerHarvest>();
        _playerHarvest.OnHarvestResourceMaterial += AddResource;
    }
    
    public void AddResource(ResourceMaterial resourceMaterial)
    {
        inventory.AddItemToInventory(resourceMaterial.resourceResourceItem);
    }
}
