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
        upgradeText.text = $"Upgrade Bonus\n\n{upgradeMap[upgrade.upgradeType].Replace("?",upgrade.value.ToString())}";
        costText.text = $"Cost: {upgrade.upgradeCost}$";
    }
}
