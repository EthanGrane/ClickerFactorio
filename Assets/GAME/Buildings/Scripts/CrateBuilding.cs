using UnityEngine;
using UnityEngine.Serialization;

public class CrateBuilding : MonoBehaviour, IBuilding, IInventory
{
    public int numberOfSlots;
    public int slotSize;
    public Inventory inventory;
    CellObject cellObject;
    
    public void Initialize(CellObject cellObject)
    {
        this.cellObject = cellObject;
        inventory = new Inventory(numberOfSlots, slotSize);
    }

    public CellObject GetCellObject()
    {
        return cellObject;
    }

    public void PlanTick()
    {
        
    }

    public void ActionTick()
    {
        
    }

    public Inventory GetInventory()
    {
        return inventory;
    }
}
