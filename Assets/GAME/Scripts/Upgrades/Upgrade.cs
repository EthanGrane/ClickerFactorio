using System;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Upgrade", menuName = "GAME/New Upgrade")]
public class Upgrade : ScriptableObject
{
    public string upgradeName;
    public string upgradeDescription;
    public UpgradeType upgradeType;
    public float value;
    [Space(20)]
    public int upgradeCost;
}

public enum UpgradeType
{
    ClickMultiplier,
    ClickValue,
    DrillMultiplier,
    DrillValue,
}
