using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] int playerMoney = 0;

    // Drill Money
    // Click Money
    public float moneyBonus = 0f;
    public float moneyMultiplier = 1f;
    
    // Damage
    public float playerHarvestDamage = 1f;
    public float playerHarvestDamageMultiplier = 1f;
    
    public List<Upgrade> upgrades;
    
    public Action<int> onPlayerMoneyChanged;
    
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
        onPlayerMoneyChanged?.Invoke(playerMoney);
    }

    public float GetUpgradesBonus(float amount)
    {
        float finalAmount = amount;
        float clickBonus = 0f;
        float clickMultiplier = 1f;
        
        for (int i = 0; i < upgrades.Count; i++)
        {
            switch (upgrades[i].upgradeType)
            {
                case UpgradeType.ClickMultiplier:
                    clickMultiplier += upgrades[i].value;
                    break;
                case UpgradeType.ClickValue:
                    clickBonus += upgrades[i].value;
                    break;
                
                case UpgradeType.DrillMultiplier:
                    break;
                case UpgradeType.DrillValue:
                    break;
            }
        }
        
        return (finalAmount + clickBonus) * clickMultiplier;
    }

    public Upgrade[] GetUpgrades()
    {
        return upgrades.ToArray();
    }
    
    public void AddUpgrade(Upgrade upgrade)
    {
        if(!upgrades.Contains(upgrade))
            upgrades.Add(upgrade);
    }
}
