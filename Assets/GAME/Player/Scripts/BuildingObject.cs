using System;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
[CreateAssetMenu(fileName = "New Building Object", menuName = "GAME/Building Object")]
public class BuildingObject : ScriptableObject
{
    public GameObject prefab;
    public int size = 1;
    public int rotation = 0;
    [FormerlySerializedAs("BuildValue")] public int BuildCost;
    public Sprite buildingSprite;
    public int inventoryIndex = 0;
    
    public BuildingObject(GameObject prefab, int size,int rotation, int buildCost)
    {
        this.prefab = prefab;
        this.size = size;
        this.rotation = rotation;
        this.BuildCost = buildCost;
    }
}