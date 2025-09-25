using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    Button upgradeButton;
    public Upgrade upgrade;
    
    UpgradeController upgradeController;

    private void OnEnable()
    {
        Upgrade[] upgrades = GameManager.Instance.GetUpgrades();
        if (upgrades.ToList().Contains(upgrade))
        {
            DisableButton();
        }

        if (upgradeButton == null)
        {
            upgradeButton = GetComponentInChildren<Button>();
            upgradeButton.onClick.AddListener(UpgradeButtonClicked);
        }
    }

    private void OnValidate()
    {
        if(upgrade != null)
            gameObject.name = "Upgrade_" + upgrade.name;
        else
            gameObject.name = "Upgrade_null";
    }

    void UpgradeButtonClicked()
    {
        upgradeController.SelectUpgradesHint(upgrade, this);
        
        Upgrade[] upgrades = GameManager.Instance.GetUpgrades();
        if (upgrades.ToList().Contains(upgrade))
        {
            DisableButton();
        }
    }
    
    public void SetUpgradeController(UpgradeController upgradeController)
    {
        this.upgradeController = upgradeController;
    }

    public void DisableButton()
    {
        upgradeButton.interactable = false;
    }
}
