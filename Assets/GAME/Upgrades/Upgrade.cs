using UnityEngine;

public enum UpgradeType
{
    MoneyMultiplier,
    ClickDamage,
    ClickDamageMultiplier,
    DrillMultiplier,
    DrillValue,
}

// Clase base abstracta
public abstract class Upgrade : ScriptableObject
{
    public string upgradeName;
    public string upgradeDescription;
    public int upgradeCost;
}