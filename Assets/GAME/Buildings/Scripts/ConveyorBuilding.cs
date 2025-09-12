using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ConveyorBuilding : MonoBehaviour, IBuilding, IInventory
{
    private CellObject cellObject;
    public Inventory inventory;
    [Space]
    public CellObject[] inputCellObjects = new CellObject[3];
    public CellObject outputCellObject;
    
    [FormerlySerializedAs("plannedInputItem")] public ResourceItem plannedInputResourceItem = null;
    public CellObject plannedInputCellObject = null;
    [FormerlySerializedAs("plannedOutputItem")] public ResourceItem plannedOutputResourceItem = null;
    
    public void Initialize(CellObject cellObject)
    {
        this.cellObject = cellObject;
        // Un conveyor simultaneamente solo puede tener un objeto
        inventory = new Inventory(1, 1);
        
        for (int i = 0; i < inputCellObjects.Length; i++)
            inputCellObjects[i] = null;
        outputCellObject = null;
    }
    
    public CellObject GetCellObject()
    {
        return cellObject;
    }

    public void PlanTick()
    {
        CheckNeighbor();
        
        // Input
        if (!inventory.isInventoryFull())
        {
            for (int i = 0; i < inputCellObjects.Length; i++)
            {
                if (inputCellObjects[i] != null)
                {
                    Inventory inputInv = inputCellObjects[i].obj.GetComponent<IInventory>().GetInventory();
                    ResourceItem resourceItem = inputInv.PeekItemFromInventory();
                    if (resourceItem != null)
                    {
                        plannedInputResourceItem = resourceItem;
                        plannedInputCellObject = inputCellObjects[i];
                        break;
                    }
                }
            }
        }
        
        // Output
        if (outputCellObject != null && !inventory.isInventoryEmpty())
        {
            Inventory outputInv = outputCellObject.obj.GetComponent<IInventory>().GetInventory();
            if (!outputInv.isInventoryFull())
            {
                plannedOutputResourceItem = inventory.PeekItemFromInventory();
            }
        }    
    }
    
    public void ActionTick()
    {
        // Entrada: coger realmente el resourceItem
        if (plannedInputResourceItem != null && !inventory.isInventoryFull())
        {
            Inventory inputInv = plannedInputCellObject.obj.GetComponent<IInventory>().GetInventory(); // suponiendo que ResourceItem o Inventory guarda referencia
            ResourceItem resourceItem = inputInv.DequeueItemFromInventory();
            if (resourceItem != null) inventory.AddItemToInventory(resourceItem);
        }

        // En PlanAction para la salida
        if (plannedOutputResourceItem != null && !inventory.isInventoryEmpty())
        {
            Inventory outputInv = outputCellObject.obj.GetComponent<IInventory>().GetInventory();
            ResourceItem resourceItem = inventory.DequeueItemFromInventory();
            if (resourceItem != null) outputInv.AddItemToInventory(resourceItem);
        }

        plannedInputResourceItem = null;
        plannedInputCellObject = null;
        plannedOutputResourceItem = null;
    }
    
    public void CheckNeighbor()
    {
        if (outputCellObject == null)
        {
            // -- OUTPUT --
            CellObject fwd = GetCellObjectFromDirection(cellObject.GetForwardDir());
            if (fwd != null)
            {
                if (fwd.obj.GetComponent<IInventory>() != null)
                {
                    outputCellObject = fwd;
                    outputCellObject.OnDestroy += (CellObject o) =>
                    {
                        outputCellObject = null;
                    };
                }
            }
        }

        // -- INPUT --
        Vector2Int[] directions =
        {
            -cellObject.GetForwardDir(),
            cellObject.GetRightDir(),
            -cellObject.GetRightDir(),
        };
        for (int i = 0; i < directions.Length; i++)
        {
            CellObject inputCellObject = GetCellObjectFromDirection(directions[i]);
            if (inputCellObject != null)
            {
                if (inputCellObject.obj.GetComponent<IInventory>() != null)
                {
                    if (inputCellObjects != null)
                    {
                        inputCellObjects[i] = inputCellObject;
                        int index = i;
                        inputCellObjects[i].OnDestroy += (CellObject o) => { inputCellObjects[index] = null; };
                    }
                }
            }
        }
    }

    CellObject GetCellObjectFromDirection(Vector2Int direction)
    {
        CellObject neighborCellObject = null;

        Vector3 worldPos = cellObject.chunk.GetCellWorldPosition(direction + cellObject.position);
        Chunk chunk = WorldManager.Instance.GetChunk(worldPos);
        if (chunk != null)
        {
            neighborCellObject = chunk.GetCellObject(chunk.GetCellCoords(worldPos));
        }

        return neighborCellObject;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        
        if(inventory.isInventoryFull())
            Gizmos.DrawSphere(transform.position + Vector3.up, 0.1f);
        
    }

    public Inventory GetInventory()
    {
        return inventory;
    }
}
