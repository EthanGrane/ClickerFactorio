using System;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(PlayerHarvest))]
[RequireComponent(typeof(PlayerBuilding))]
public class PlayerController : MonoBehaviour
{
    public PlayerMode playerMode = PlayerMode.harvest;
    
    PlayerBuilding playerBuilding;
    PlayerHarvest playerHarvest;

    void Awake()
    {
        playerBuilding = gameObject.GetComponent<PlayerBuilding>();
        playerHarvest = gameObject.GetComponent<PlayerHarvest>();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
            ChangePlayerMode(playerMode == PlayerMode.harvest ? PlayerMode.building : PlayerMode.harvest);
        
        switch (playerMode)
        {
            case PlayerMode.harvest:
                playerHarvest.HandleHarvest();
                break;
            case PlayerMode.building:
                playerBuilding.HandleBuilding();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    void ChangePlayerMode(PlayerMode newMode)
    {
        switch (newMode)
        {
            case PlayerMode.harvest:
                playerBuilding.StopBuilding();
                playerBuilding.HideAllBuildingGhost();
                playerBuilding.HideGrid();
                break;
            case PlayerMode.building:
                playerBuilding.ShowGrid();
                break;
        }        
        
        playerMode = newMode;
    }
}

public enum PlayerMode
{
    harvest,
    building,
}