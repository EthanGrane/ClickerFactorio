using System;
using UnityEngine;

[System.Serializable]
public class BuildingObject
{
    public GameObject prefab;      // el objeto que se instanciará
    public int size = 1; // tamaño en celdas
    public int rotation = 0;
    public int BuildValue;

    public BuildingObject(GameObject prefab, int size,int rotation, int BuildValue)
    {
        this.prefab = prefab;
        this.size = size;
        this.rotation = rotation;
        this.BuildValue = BuildValue;
    }
}