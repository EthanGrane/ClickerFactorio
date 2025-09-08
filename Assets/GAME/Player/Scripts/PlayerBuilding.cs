using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class PlayerBuilding : MonoBehaviour
{
    public int buildingRotation = 0;
    public Material ghostMaterial;
    public Material obstructedMaterial;
    public Material worldGridMaterial;
    
    public BuildingObject[] inventoryBuildingObjects = new BuildingObject[10];
    GameObject[] ghostObjects = new GameObject[10];
    private int currentInventoryIndex = 0;
    
    Vector2Int iBuildPos = -Vector2Int.one;
    Vector2Int iBuildDirection = -Vector2Int.one;
    List<CellObject> lastObjectsBuilded = new List<CellObject>();
    CellObject currentBuilding;

    void Start()
    {
        HideAllBuildingGhost();
        HideGrid();
        ChangeCurrentBuilding(inventoryBuildingObjects[currentInventoryIndex]);
    }
    
    public void HandleBuilding()
    {
        // ***** RESET *****
        // On Mouse up reset snap
        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            StopBuilding();
        
        // ***** Remove ***** 
        if (Input.GetMouseButton(1))
            RemoveBuilding();
        
        // ***** Build ***** 
        if (Input.GetMouseButton(0))
        {
            Build(currentBuilding);
        }       
        
        // ***** Rotate ***** 
        if (Input.GetAxis("Mouse ScrollWheel") > 0f)
            buildingRotation = (buildingRotation + 1) % 4;
        if (Input.GetAxis("Mouse ScrollWheel") < -0f)
        {
            buildingRotation = buildingRotation - 1;
            if (buildingRotation < 0) buildingRotation = 3;
        }

        // Rotate current objects
        if (lastObjectsBuilded.Count > 0)
            foreach (CellObject obj in lastObjectsBuilded)
                obj.RotateObject(rotation: buildingRotation);

        HandleInventory();
        ShowBuildingGhost(currentBuilding);
    }

    void HandleInventory()
    {
        for (int i = 0; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                if (i < inventoryBuildingObjects.Length)
                {
                    if (i == 9)
                        ChangeCurrentBuilding(inventoryBuildingObjects[i]);
                    else
                        currentBuilding = new CellObject(inventoryBuildingObjects[i]);

                    currentInventoryIndex = i;
                }
            }
        }
    }
    public void ChangeCurrentBuilding(BuildingObject newBuilding)
    {
        currentBuilding = new CellObject(newBuilding);
    }

    public void ShowGrid()
    {
        worldGridMaterial.DOFloat(1f,"_Amount",0.1f).SetEase(Ease.InBounce);
    }

    public void HideGrid()
    {
        worldGridMaterial.DOFloat(0f,"_Amount",0.2f).SetEase(Ease.Linear);
    }

    public void StopBuilding()
    {
        iBuildPos = -Vector2Int.one;
        iBuildDirection = -Vector2Int.one;
        lastObjectsBuilded.Clear();
    }

    public void HideAllBuildingGhost()
    {
        foreach (GameObject ghostObject in ghostObjects)
            if(ghostObject)
                ghostObject.SetActive(false);
    }

    void Build(CellObject building)
    {
        HideAllBuildingGhost();

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 5))
        {
            Chunk chunk = WorldManager.Instance.GetChunk(hit.point);
            Vector2Int cellPosition = chunk.GetCellCoords(hit.point);

            // Snap to same XZ direction
            if(iBuildPos == -Vector2Int.one)
                iBuildPos = cellPosition;

            int currentMoney = GameManager.Instance.GetPlayerMoney();
            int buildingCost = inventoryBuildingObjects[currentInventoryIndex].BuildValue;
            
            if (IsSnappedPosition(cellPosition))
            {
                if (currentMoney >= buildingCost)
                {
                    building.rotation = buildingRotation;
                    var placed = chunk.PlaceCellObject(cellPosition, building);
                    if (placed != null)
                    {
                        GameManager.Instance.AddMoney(-buildingCost);
                        lastObjectsBuilded.Add(placed);
                    }
                }
            }
        }
    }

    void RemoveBuilding()
    {
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 5))
        {
            Chunk chunk = WorldManager.Instance.GetChunk(hit.point);
            Vector2Int cellPosition = chunk.GetCellCoords(hit.point);

            if(iBuildPos == -Vector2Int.one)
                iBuildPos = cellPosition;

            if (IsSnappedPosition(cellPosition))
            {
                chunk.RemoveCellBuilding(cellPosition);
            }
        }
    }

    void ShowBuildingGhost(CellObject building)
    {
        if(!building.prefab)
            return;
        
        if (!ghostObjects[currentInventoryIndex])
        {
            ghostObjects[currentInventoryIndex] = Instantiate(building.prefab).gameObject;
            
            if(ghostObjects[currentInventoryIndex].GetComponent<Collider>())
                ghostObjects[currentInventoryIndex].GetComponent<Collider>().enabled = false;
        }

        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, 5))
        {
            if (!ghostObjects[currentInventoryIndex].activeSelf)
                ghostObjects[currentInventoryIndex].SetActive(true);

            Chunk chunk = WorldManager.Instance.GetChunk(hit.point);
            Vector2Int cellPosition = chunk.GetCellCoords(hit.point);
            building.rotation = buildingRotation;

            Vector3 placement = Chunk.GetPlacementWorldPosition(cellPosition, building.size, chunk.transform.position, building.rotation);

            ghostObjects[currentInventoryIndex].transform.position = placement + Vector3.up * 0.01f;
            ghostObjects[currentInventoryIndex].transform.rotation = Quaternion.Euler(0, building.rotation * 90, 0);

            bool canPlace = chunk.CanPlaceCellObject(cellPosition, building.size, building.rotation);
            
            int currentMoney = GameManager.Instance.GetPlayerMoney();
            int buildingCost = inventoryBuildingObjects[currentInventoryIndex].BuildValue;
            
            if(currentMoney < buildingCost)
                canPlace = false;
            
            ghostObjects[currentInventoryIndex].GetComponent<MeshRenderer>().material = canPlace ? ghostMaterial : obstructedMaterial;
        }
        else
        {
            ghostObjects[currentInventoryIndex].transform.position = Vector3.up * -100;
        }
    }
    
    bool IsSnappedPosition(Vector2Int cellPosition)
    {
        if (iBuildPos == -Vector2Int.one) return false;

        if (iBuildDirection == -Vector2Int.one && cellPosition != iBuildPos)
        {
            Vector2Int delta = cellPosition - iBuildPos;

            if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
                iBuildDirection = Vector2Int.right;
            else
                iBuildDirection = Vector2Int.up;
        }

        if (iBuildDirection == -Vector2Int.one)
            return true; 

        if (iBuildDirection == Vector2Int.right)
            return cellPosition.y == iBuildPos.y;

        if (iBuildDirection == Vector2Int.up)
            return cellPosition.x == iBuildPos.x;

        return false;
    }
    
    void OnDrawGizmos()
    {
        for (int i = 0; i < lastObjectsBuilded.Count; i++)      
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawCube(lastObjectsBuilded[i].chunk.GetCellWorldPosition(lastObjectsBuilded[i].position) + Vector3.up, Vector3.one * 0.5f);
        }
    }
}