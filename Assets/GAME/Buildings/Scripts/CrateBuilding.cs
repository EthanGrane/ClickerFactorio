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
    
    // Crate no have logic
    public void PlanTick(){}
    public void ActionTick(){}

    // Get
    public CellObject GetCellObject() => cellObject;
    public Inventory GetInventory() => inventory;
}
