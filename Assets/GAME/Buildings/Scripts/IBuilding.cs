using UnityEngine;

public interface IBuilding
{
    public void Initialize(CellObject cellObject);
    public void Tick();
}
