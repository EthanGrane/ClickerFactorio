using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] int playerMoney = 0;
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

    // Player Money
    public void AddMoney(int amount) { SetPlayerMoney(playerMoney + amount); }
    public int GetPlayerMoney() { return playerMoney; }

    public void SetPlayerMoney(int amount)
    {
        playerMoney = amount; 
        onPlayerMoneyChanged?.Invoke(playerMoney);
    }
    
}
