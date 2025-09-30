using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] int playerMoney = 0;
    
    public List<Upgrade> upgrades;
    
    public Action<int> OnPlayerMoneyChanged;
    public Action<Upgrade> OnPlayerBoughtUpgrade;
    
    public List<ResourceItem> allResources = new List<ResourceItem>();
    public List<ResourceItem> unlockedResources = new List<ResourceItem>();
    
    public List<BuildingObject> allBuildings = new List<BuildingObject>();
    public List<BuildingObject> unlockedBuildings = new List<BuildingObject>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void RemoveMoney(float amount)
    {
        amount = Mathf.Abs(amount);
        SetPlayerMoney(playerMoney - (int)(amount));
    }

    // Player Money
    public void AddMoney(float amount, bool useUpgrades = true)
    {
        float finalAmount = Mathf.Abs(amount);
        if(useUpgrades)
            finalAmount = GetUpgradesBonus(amount);
        SetPlayerMoney(playerMoney + (int)(finalAmount));
    }
    public int GetPlayerMoney() { return playerMoney; }

    public void SetPlayerMoney(int amount)
    {
        playerMoney = amount; 
        OnPlayerMoneyChanged?.Invoke(playerMoney);
    }

    public float GetUpgradesBonus(float amount)
    {
        float finalAmount = amount;
        float clickBonus = 0f;
        float clickMultiplier = 1f;
        
        for (int i = 0; i < upgrades.Count; i++)
        {
            if (upgrades[i] is UpgradeBonus bonus)
            {
                switch (bonus.upgradeType)
                {
                    case UpgradeType.MoneyMultiplier:
                        clickMultiplier += bonus.value;
                        break;
                    case UpgradeType.DrillMultiplier:
                        break;
                    case UpgradeType.DrillValue:
                        break;
                }
            }
        }
        
        return (finalAmount + clickBonus) * clickMultiplier;
    }

    public int GetClickDamage()
    {
        int baseDamage = 1;
        int flatBonus = 0;
        int multiplier = 1;

        for (int i = 0; i < upgrades.Count; i++)
        {
            if (upgrades[i] is UpgradeBonus bonus)
            {
                switch (bonus.upgradeType)
                {
                    case UpgradeType.ClickDamage:
                        flatBonus += (int)bonus.value;
                        break;
                    case UpgradeType.ClickDamageMultiplier:
                        multiplier *= (int)bonus.value;
                        break;
                }
            }
        }

        return (baseDamage + flatBonus) * multiplier;
    }


    public Upgrade[] GetUpgrades()
    {
        return upgrades.ToArray();
    }
    
    public void AddUpgrade(Upgrade upgrade)
    {
        if(!upgrades.Contains(upgrade))
        {
            upgrades.Add(upgrade);

            if (upgrade is UpgradeUnlock)
                HandleUpgradeUnlock(upgrade as UpgradeUnlock);
            
            OnPlayerBoughtUpgrade?.Invoke(upgrade);
        }    
    }

    void HandleUpgradeUnlock(UpgradeUnlock upgradeUnlock)
    {
        if(unlockedBuildings.Contains(upgradeUnlock.buildingObject) == false)
            unlockedBuildings.Add(upgradeUnlock.buildingObject);
    }
}
