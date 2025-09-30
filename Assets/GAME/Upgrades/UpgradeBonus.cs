using UnityEngine;

[CreateAssetMenu(fileName = "New Upgrade Bonus", menuName = "GAME/Upgrade Bonus")]
public class UpgradeBonus : Upgrade
{
    public UpgradeType upgradeType;
    public float value;
}