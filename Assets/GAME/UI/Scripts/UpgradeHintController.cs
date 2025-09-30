using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpgradeHintController : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI upgradeText;
    public TextMeshProUGUI costText;

    private Dictionary<UpgradeType, string> upgradeMap = new Dictionary<UpgradeType, string>
    {
        { UpgradeType.ClickDamage, "+? Click Damage" },
        { UpgradeType.ClickDamageMultiplier, "+? Click Damage Multiplier" },
        { UpgradeType.MoneyMultiplier, "+? Money Multiplier" },
    };

    public void SetupUpgradeHint(Upgrade upgrade)
    {
        titleText.text = upgrade.upgradeName;
        descriptionText.text = $"{upgrade.upgradeDescription}";
        costText.text = $"Cost: {upgrade.upgradeCost}$";

        if (upgrade is UpgradeBonus bonus)
        {
            upgradeText.text = $"Upgrade Bonus\n\n{upgradeMap[bonus.upgradeType].Replace("?",bonus.value.ToString())}";
        }
        else if (upgrade is UpgradeUnlock unlock)
        {
            upgradeText.text = $"Unlock\n\n{unlock.upgradeName}\n{unlock.upgradeDescription}";
        }
    }
}
