using System;
using UnityEngine;

public class ConveyorBuilding : MonoBehaviour, IBuilding
{
    private CellObject cellObject;

    public CellObject inputCellObject;
    public CellObject outputCellObject;

    private bool alreadyCheckedOnThisTick = false;

    public void Initialize(CellObject cellObject)
    {
        this.cellObject = cellObject;
        CheckNeighbor();
    }

    public void CheckNeighbor()
    {
        if (alreadyCheckedOnThisTick) return;
        alreadyCheckedOnThisTick = true;

        Vector2Int myForward = cellObject.GetForwardDir();
        Vector2Int myPos = cellObject.position;

        // ---------------- OUTPUT ----------------
        Vector2Int frontLocal = myPos + myForward;
        Vector3 frontWorldPos = cellObject.chunk.GetCellWorldPosition(frontLocal);

        Chunk fwdChunk = WorldManager.Instance.GetChunk(frontWorldPos);
        Vector2Int fwdCell = fwdChunk.GetCellCoords(frontWorldPos);
        CellObject forwardCell = fwdChunk.cellData[fwdCell.x, fwdCell.y];

        if (forwardCell?.obj && forwardCell.obj.TryGetComponent(out ConveyorBuilding fwdConv))
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

        // ---------------- INPUTS ----------------
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

            if (inputCell?.obj && inputCell.obj.TryGetComponent(out ConveyorBuilding inputConv))
            {
                // Confirmar que este vecino apunta hacia mí
                if (inputConv.cellObject.position + inputConv.cellObject.GetForwardDir() == myPos)
                {
                    SetBuildingInput(inputCell);
                    inputConv.SetBuildingOutput(cellObject);
                }
            }
        }
    }

    public void SetBuildingInput(CellObject input)
    {
        if (!input.obj) return;
        if (!input.obj.TryGetComponent<ConveyorBuilding>(out _)) return;

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
        if (!output.obj.TryGetComponent<ConveyorBuilding>(out _)) return;

        outputCellObject = output;
        output.OnDestroy += (outputCell) =>
        {
            if (outputCellObject == outputCell)
                outputCellObject = null;
        };
    }

    public void Tick()
    {
        alreadyCheckedOnThisTick = false;
    }
    
    #region DEBUG
    
    /*void Update()
    {
        DrawDebugConnections();
    }*/
    
    /*
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
    */
    #endregion
}
