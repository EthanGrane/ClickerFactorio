using System;
using UnityEngine;

public class ConveyorBuilding : MonoBehaviour, IBuilding, IInventory
{
    private CellObject cellObject;
    public Inventory inventory;
    
    // Esto deberia de ser Inventory???
    public CellObject inputCellObject;
    public CellObject outputCellObject;

    private bool alreadyCheckedOnThisTick = false;
    private bool alreadyMovedItemOnThisTick = false;

    public void Initialize(CellObject cellObject)
    {
        this.cellObject = cellObject;
        // Un conveyor simultaneamente solo puede tener un objeto
        inventory = new Inventory(1, 1);
        
        CheckNeighbor();
    }
    
    public CellObject GetCellObject()
    {
        return cellObject;
    }

    public void Tick()
    {
        alreadyCheckedOnThisTick = false;
        alreadyMovedItemOnThisTick = false;

        if (outputCellObject != null && outputCellObject.obj && !alreadyMovedItemOnThisTick)
        {
            if (inventory.isInventoryFull()) // Tengo un item en mi inventario entonces lo desplazo al proximo inventario
            {
                Inventory outputInventory = outputCellObject.obj.GetComponent<IInventory>().GetInventory();

                if (outputInventory.isInventoryFull() == false)
                {
                    outputInventory.AddItemToInventory(inventory.DequeueItemFromInventory());
                }
            }
        }
        
        if (inputCellObject != null && inputCellObject.obj && !alreadyCheckedOnThisTick)
        {
            if (inventory.isInventoryFull() == false)   // Tengo hueco en el inventario entonces recojo un item
            {
                inventory.AddItemToInventory(inputCellObject.obj.GetComponent<IInventory>().GetInventory().DequeueItemFromInventory());
            }
        }
    }
    
    public void CheckNeighbor()
    {
        if (alreadyCheckedOnThisTick) return;
        alreadyCheckedOnThisTick = true;

        Vector2Int myForward = cellObject.GetForwardDir();
        Vector2Int myPos = cellObject.position;
        
        // ---------------- OUTPUT ----------------
        if (outputCellObject.obj == null)
        {
            Vector2Int frontLocal = myPos + myForward;
            Vector3 frontWorldPos = cellObject.chunk.GetCellWorldPosition(frontLocal);

            Chunk fwdChunk = WorldManager.Instance.GetChunk(frontWorldPos);
            Vector2Int fwdCell = fwdChunk.GetCellCoords(frontWorldPos);
            CellObject forwardCell = fwdChunk.cellData[fwdCell.x, fwdCell.y];

            if (forwardCell?.obj)
            {
                // Si el forward es un conveyor
                if (forwardCell.obj.TryGetComponent(out ConveyorBuilding fwdConv))
                {
                    // Si el conveyor de adelante mira hacia mi, yo soy su input
                    if (fwdConv.cellObject.position + fwdConv.cellObject.GetForwardDir() == myPos)
                    {
                        SetBuildingInput(forwardCell);
                        fwdConv.SetBuildingOutput(cellObject);
                    }
                    else // Si no, él es mi output
                    {
                        SetBuildingOutput(forwardCell);
                        fwdConv.SetBuildingInput(cellObject);
                    }
                }

                if (forwardCell.obj.TryGetComponent(out IInventory fwdInventory))
                {
                    Debug.Log("forwardCell.obj.TryGetComponent(out IInventory fwdInventory)");
                    SetBuildingOutput(fwdInventory.GetCellObject());
                }
                else
                {
                    Debug.Log("forwardCell.obj.TryGetComponent(out IInventory fwdInventory) == NULL");
                }
            }
        }

        // ---------------- INPUTS ----------------
        if (inputCellObject.obj == null)
        {
            Vector2Int[] directions =
            {
                -myForward,
                cellObject.GetRightDir(),
                -cellObject.GetRightDir()
            };

            foreach (Vector2Int dir in directions)
            {
                Vector2Int inputLocalCellPos = myPos + dir;
                Vector3 inputWorldPos = cellObject.chunk.GetCellWorldPosition(inputLocalCellPos);

                Chunk inputChunk = WorldManager.Instance.GetChunk(inputWorldPos);
                Vector2Int inputCellCoords = inputChunk.GetCellCoords(inputWorldPos);
                CellObject inputCell = inputChunk.cellData[inputCellCoords.x, inputCellCoords.y];

                if (inputCell?.obj)
                {
                    // Si el INPUT es un conveyor
                    if (inputCell.obj.TryGetComponent(out ConveyorBuilding inputConv))
                    {
                        // Confirmar que este vecino apunta hacia mí
                        if (inputConv.cellObject.position + inputConv.cellObject.GetForwardDir() == myPos)
                        {
                            SetBuildingInput(inputCell);
                            inputConv.SetBuildingOutput(cellObject);
                        }
                    }

                    if (inputCell.obj.TryGetComponent(out IInventory inputInv))
                    {
                        SetBuildingInput(inputInv.GetCellObject());
                    }
                }
            }
        }
    }

    public void SetBuildingInput(CellObject input)
    {
        if (!input.obj) return;
        if (!input.obj.TryGetComponent<IInventory>(out _)) return;

        inputCellObject = input;
        input.OnDestroy += (inputCell) =>
        {
            if (inputCellObject == inputCell)
                inputCellObject = null;
        };
    }

    public void SetBuildingOutput(CellObject output)
    {
        if (!output.obj) return;
        if (!output.obj.TryGetComponent<IInventory>(out _)) return;

        outputCellObject = output;
        output.OnDestroy += (outputCell) =>
        {
            if (outputCellObject == outputCell)
                outputCellObject = null;
        };
    }
    
    #region DEBUG
    
    void Update()
    {
        DrawDebugConnections();
    }
    
    
    private void DrawDebugConnections()
    {
        if (inputCellObject?.obj != null)
        {
            Debug.DrawLine(
                cellObject.chunk.GetCellWorldPosition(cellObject.position) + Vector3.up * 1.2f,
                inputCellObject.chunk.GetCellWorldPosition(inputCellObject.position) + Vector3.up * 1.2f,
                Color.green
            );
        }

        if (outputCellObject?.obj != null)
        {
            Debug.DrawLine(
                cellObject.chunk.GetCellWorldPosition(cellObject.position) + Vector3.up * 1.5f,
                outputCellObject.chunk.GetCellWorldPosition(outputCellObject.position) + Vector3.up * 1.5f,
                Color.red
            );
        }
    }
    
    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        if(inventory.isInventoryFull())
            Gizmos.color = Color.red;

        Gizmos.DrawSphere(transform.position + Vector3.up, 0.1f);

    }

    public Inventory GetInventory()
    {
        return inventory;
    }
}
