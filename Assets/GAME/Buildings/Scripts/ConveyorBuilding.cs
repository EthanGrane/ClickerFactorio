using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

public class ConveyorBuilding : MonoBehaviour, IBuilding, IInventory
{
    private CellObject cellObject;
    public Inventory inventory;
    public GameObject conveyorItem;
    [Space]
    public CellObject[] inputCellObjects = new CellObject[3];
    public CellObject outputCellObject;

    // Colas para planificar movimiento
    private Queue<ResourceItem> plannedInputItems = new Queue<ResourceItem>();
    private Queue<ResourceItem> plannedOutputItems = new Queue<ResourceItem>();
    private Queue<CellObject> plannedInputSources = new Queue<CellObject>();

    public void Initialize(CellObject cellObject)
    {
        this.cellObject = cellObject;
        inventory = new Inventory(1, 1, "Conveyor Belt");
    
        for (int i = 0; i < inputCellObjects.Length; i++)
            inputCellObjects[i] = null;
        outputCellObject = null;
    }

    public CellObject GetCellObject() => cellObject;

    public void PlanTick()
    {
        CheckNeighbor();

        // 1. Planificar salida primero (vaciar el conveyor)
        if (outputCellObject != null)
        {
            Inventory outputInv = outputCellObject.obj.GetComponent<IInventory>().GetInventory();
            while (!inventory.isInventoryEmpty() && !outputInv.isInventoryFull())
            {
                ResourceItem item = inventory.DequeueItemFromInventory();
                plannedOutputItems.Enqueue(item);
            }
        }

        // 2. Planificar entrada desde todos los inputs
        for (int i = 0; i < inputCellObjects.Length; i++)
        {
            var input = inputCellObjects[i];
            if (input == null) continue;

            Inventory inputInv = input.obj.GetComponent<IInventory>().GetInventory();
            while (!inventory.isInventoryFull() && !inputInv.isInventoryEmpty())
            {
                ResourceItem item = inputInv.DequeueItemFromInventory();
                plannedInputItems.Enqueue(item);
            }
        }
    }

    public void ActionTick()
    {
        // Ejecutar entrada (llenar el conveyor desde la cola planificada)
        while (plannedInputItems.Count > 0 && !inventory.isInventoryFull())
        {
            ResourceItem item = plannedInputItems.Dequeue();
            inventory.AddItemToInventory(item);
        }

        // Ejecutar salida (mover hacia el siguiente)
        while (plannedOutputItems.Count > 0)
        {
            ResourceItem item = plannedOutputItems.Dequeue();
            Inventory outputInv = outputCellObject.obj.GetComponent<IInventory>().GetInventory();
            if (!outputInv.isInventoryFull())
            {
                inventory.ClearInventory();
                outputInv.AddItemToInventory(item);
            }           
            else
                break; // Si no hay espacio, detenemos
        }
        
        HandleConveyorItem();
    }

    void HandleConveyorItem()
    {
        if (inventory.PeekItemFromInventory() != null)
        {
            conveyorItem.SetActive(true);
            conveyorItem.GetComponent<MeshRenderer>().material.mainTexture =
                inventory.PeekItemFromInventory().itemIcon.texture;
        }
        else
        {
            conveyorItem.SetActive(false);
        }
    }
    
    public void CheckNeighbor()
    {
        if (outputCellObject == null)
        {
            // -- OUTPUT --
            CellObject fwd = GetCellObjectFromDirection(cellObject.GetForwardDir());
            if (fwd != null && fwd.obj.GetComponent<IInventory>() != null)
            {
                outputCellObject = fwd;
                outputCellObject.OnDestroy += (CellObject o) => outputCellObject = null;
            }
        }

        // -- INPUT --
        Vector2Int[] directions =
        {
            -cellObject.GetForwardDir(),
            cellObject.GetRightDir(),
            -cellObject.GetRightDir()
        };

        for (int i = 0; i < directions.Length; i++)
        {
            CellObject inputCell = GetCellObjectFromDirection(directions[i]);
            if (inputCell != null && inputCell.obj.GetComponent<IInventory>() != null)
            {
                inputCellObjects[i] = inputCell;
                int index = i;
                inputCell.OnDestroy += (CellObject o) => inputCellObjects[index] = null;
            }
        }
    }

    private CellObject GetCellObjectFromDirection(Vector2Int direction)
    {
        Vector3 worldPos = cellObject.chunk.GetCellWorldPosition(direction + cellObject.position);
        Chunk chunk = WorldManager.Instance.GetChunk(worldPos);
        if (chunk != null)
            return chunk.GetCellObject(chunk.GetCellCoords(worldPos));
        return null;
    }
    
    void OnDrawGizmos()
    {
        if (!inventory.isInventoryEmpty())
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position + Vector3.up, 0.25f);
        }
    }
    
    public Inventory GetInventory() => inventory;
}
