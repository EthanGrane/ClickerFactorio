using UnityEngine;

public class CrateBuilding : MonoBehaviour, IBuilding, IInventory
{
    public int numberOfSlots;
    public int slotSize;
    public Inventory inventory;
    public Item DEBUG_ITEM;
    CellObject cellObject;
    
    public void Initialize(CellObject cellObject)
    {
        this.cellObject = cellObject;
        inventory = new Inventory(numberOfSlots, slotSize);

        for (int i = 0; i < slotSize; i++)
        {
            inventory.AddItemToInventory(DEBUG_ITEM);
        }
    }

    public CellObject GetCellObject()
    {
        return cellObject;
    }

    public void Tick()
    {
        
    }

    public Inventory GetInventory()
    {
        return inventory;
    }
}
