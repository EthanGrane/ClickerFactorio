using UnityEngine;
using System;

public interface IBuilding
{
    
    public void Initialize(CellObject cellObject);
    public void Tick();
}
