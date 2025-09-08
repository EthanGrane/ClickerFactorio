using System;
using UnityEngine;

[System.Serializable]
public class CellObject
{
    public GameObject prefab;      // el objeto que se instanciará
    public GameObject obj;      // el objeto que se ha instanciado
    public Chunk chunk;
    public Vector2Int position = Vector2Int.zero; // tamaño en celdas
    public int size = 1; // tamaño en celdas
    public CellType type;            // "Resource", "Building", etc.
    public int rotation;           // múltiplo de 90
    
    public CellObject(CellObject other)
    {
        this.prefab   = other.prefab;
        this.size     = other.size;
        this.type     = other.type;
        this.rotation = other.rotation;
    }
    
    public CellObject(BuildingObject other)
    {
        this.prefab   = other.prefab;
        this.size     = other.size;
        this.rotation = other.rotation;
        this.type     = CellType.Building;
    }
    
    public void Initialize(GameObject obj, Chunk chunk, Vector2Int position, int rotation)
    {
        this.obj = obj;
        this.chunk = chunk;
        this.position = position;
        this.rotation = rotation;
        
        RotateObject(rotation);
    }
    
    public void RotateObject(int rotation)
    {
        this.rotation = rotation;
        obj.transform.rotation = Quaternion.Euler(0,rotation * 90,0);
    }
    
    public Vector2Int GetForwardDir()
    {
        switch (rotation % 4)
        {
            case 0: return Vector2Int.up;
            case 1: return Vector2Int.right;
            case 2: return Vector2Int.down;
            case 3: return Vector2Int.left;
        }
        return Vector2Int.zero;
    }

    public Vector2Int GetRightDir()
    {
        switch (rotation % 4)
        {
            case 0: return Vector2Int.right;
            case 1: return Vector2Int.down;
            case 2: return Vector2Int.left;
            case 3: return Vector2Int.up;
        }
        return Vector2Int.zero;
    }
}
    
public enum CellType
{
    Empty,
    Building,
    Resource,
}