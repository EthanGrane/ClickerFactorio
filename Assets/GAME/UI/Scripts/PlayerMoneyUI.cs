using UnityEngine;
using TMPro;
    
public class PlayerMoneyUI : MonoBehaviour
{
    public TextMeshProUGUI playerMoneyText;
    public TextMeshProUGUI playerMoneyCostText;
    
    PlayerBuilding playerBuilding;
    
    public void Start()
    {
        GameManager.Instance.OnPlayerMoneyChanged += (int amount) =>
        {
            playerMoneyText.text = amount.ToString();
        };

        playerBuilding = FindFirstObjectByType<PlayerBuilding>();
        // Events
        playerBuilding.OnStartBuilding += () =>
        {
            if(playerBuilding.currentBuilding)
                SetMoneyCost(playerBuilding.currentBuilding.BuildCost);
        };
        playerBuilding.OnStopBuilding += HideMoneyText;
        playerBuilding.onCurrentBuildingChanged += (BuildingObject obj) =>
        {
            if(obj)
                SetMoneyCost(obj.BuildCost);
        };
    }

    public void SetMoneyCost(int cost)
    {
        playerMoneyCostText.text = "-" + cost.ToString();
    }

    public void HideMoneyText()
    {
        playerMoneyCostText.text = "";
    }
}
