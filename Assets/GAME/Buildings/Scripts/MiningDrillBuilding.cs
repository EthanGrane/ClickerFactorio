using System;
using System.Collections.Generic;
using UnityEngine;

public class MiningDrillBuilding : MonoBehaviour, IBuilding, IInventory
{
    public event Action<CellObject> OnDestroy;

    public int ticksForInteract = 4;
    public Inventory inventory;
    public List<CellObject> affectedObjects;
    
    [Header("Visuals")]
    public ParticleSystem ps;
    
    private int currentTicks = 0;
    CellObject cellObject = null;
    
    public void Initialize(CellObject cellObject)
    {
        this.cellObject = cellObject;
        inventory = new Inventory(2, 16, "Mining Drill");
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
        currentTicks++;

        if (currentTicks >= ticksForInteract)
        {
            currentTicks = 0;

            if (affectedObjects == null || affectedObjects.Count == 0)
                SetupAffectedObjects();

            foreach (CellObject cell in affectedObjects)
            {
                if (cell.type == CellType.Resource)
                {
                    ResourceMaterial harvestMaterial = cell.obj.GetComponent<ResourceMaterial>();
                    if (harvestMaterial)
                    {
                        // Is inventory NOT full?
                        // Is resourceItem Avaliable on inventory slots?
                        if (inventory.isInventoryFull() == false || inventory.isItemAvaliableOnInventory(harvestMaterial.resourceResourceItem))
                        {
                            if(!ps.isPlaying)
                                ps.Play();
                            
                            harvestMaterial.BounceObject();
                            harvestMaterial.HarvestMaterial(1);
                            inventory.AddItemToInventory(harvestMaterial.resourceResourceItem);
                        }
                        else
                        {
                            if(ps.isPlaying)
                                ps.Stop();
                        }
                    }

                }
            }
        }
    }

    private void SetupAffectedObjects()
    {
        if (cellObject == null) return;

        affectedObjects = new List<CellObject>();

        Vector2Int forward = cellObject.GetForwardDir();
        Vector2Int right   = cellObject.GetRightDir();
        
        int dist = 1;
        if (cellObject.rotation < 2)
            dist = 2;
        
        Vector2Int frontLocalL = cellObject.position + forward * dist;
        Vector2Int frontLocalR = cellObject.position + forward * dist - right;

        switch (cellObject.rotation)
        {
            case 0: frontLocalR = cellObject.position + forward * dist + right; break;
            case 1: frontLocalR = cellObject.position + forward * dist - right; break;
            case 2: frontLocalR = cellObject.position + forward * dist - right; break;
            case 3: frontLocalR = cellObject.position + forward * dist + right; break;

        }
        
        Vector3 worldPosL = cellObject.chunk.GetCellWorldPosition(frontLocalL);
        Vector3 worldPosR = cellObject.chunk.GetCellWorldPosition(frontLocalR);

        // Obtenemos los chunks que contienen esas posiciones
        Chunk chunkL = WorldManager.Instance.GetChunk(worldPosL);
        Chunk chunkR = WorldManager.Instance.GetChunk(worldPosR);

        if (chunkL != null)
        {
            Vector2Int localL = chunkL.GetCellCoords(worldPosL);
            CellObject cellL = chunkL.cellData[localL.x, localL.y];
            if (cellL != null) affectedObjects.Add(cellL);
        }

        if (chunkR != null)
        {
            Vector2Int localR = chunkR.GetCellCoords(worldPosR);
            CellObject cellR = chunkR.cellData[localR.x, localR.y];
            if (cellR != null) affectedObjects.Add(cellR);
        }
    }
    
    void OnDrawGizmos()
    {
        if (cellObject == null) return;

        Vector2Int forward = cellObject.GetForwardDir();
        Vector2Int right   = cellObject.GetRightDir();

        int dist = 1;
        if (cellObject.rotation < 2)
            dist = 2;
        
        Vector2Int frontLocalL = cellObject.position + forward * dist;
        Vector2Int frontLocalR = cellObject.position + forward * dist - right;

        switch (cellObject.rotation)
        {
            case 0: frontLocalR = cellObject.position + forward * dist + right; break;
            case 1: frontLocalR = cellObject.position + forward * dist - right; break;
            case 2: frontLocalR = cellObject.position + forward * dist - right; break;
            case 3: frontLocalR = cellObject.position + forward * dist + right; break;

        }

        Vector3 worldPosL = cellObject.chunk.GetCellWorldPosition(frontLocalL);
        Vector3 worldPosR = cellObject.chunk.GetCellWorldPosition(frontLocalR);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(worldPosL + Vector3.up * 0.5f, Vector3.one * 0.25f);
        Gizmos.DrawWireCube(worldPosR + Vector3.up * 0.5f, Vector3.one * 0.25f);
    }

    public Inventory GetInventory()
    {
        return inventory;
    }
}
