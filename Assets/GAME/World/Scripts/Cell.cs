using UnityEngine;

[System.Serializable]
public class Cell
{
    public Vector2Int position;
    public CellObject cellObject;
    public bool isOccupied => cellObject != null;

    public Cell(int x, int y)
    {
        position = new Vector2Int(x, y);
    }
}