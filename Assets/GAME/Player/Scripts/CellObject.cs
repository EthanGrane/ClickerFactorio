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
}

public enum CellType
{
    Empty,
    Building,
    Resource,
}