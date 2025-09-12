using UnityEngine;
using System;

public interface IBuilding
{
    
    public void Initialize(CellObject cellObject);
    
    public CellObject GetCellObject();
    public void PlanTick();
    public void ActionTick();
}
