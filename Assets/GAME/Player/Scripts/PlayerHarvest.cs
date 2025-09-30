using System;
using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;

public class PlayerHarvest : MonoBehaviour
{
    PlayerInventory inventory;
    
    public float harvestDistance = 5;
    public LayerMask ResourceLayerMask;

    Dictionary<Upgrade, Sprite> mouseUpgradesTexturesDictionary = new Dictionary<Upgrade, Sprite>();

    public Upgrade[] MouseUpgrades;
    public Sprite[] mouseSprites;
    public SpriteRenderer mouseRenderer;
    
    ResourceMaterial lastResourceMaterial = null;
    
    public Action<ResourceMaterial> OnHarvestResourceMaterial;
    
    [Header("Autoclicker Settings")]
    public bool enableAutoClick = false;
    public float autoClickRate = 3f; // clicks por segundo
    private float autoClickTimer = 0f;

    private void Start()
    {
        mouseUpgradesTexturesDictionary = new Dictionary<Upgrade, Sprite>();

        for (int i = 0; i < MouseUpgrades.Length && i < mouseSprites.Length; i++)
        {
            mouseUpgradesTexturesDictionary[MouseUpgrades[i]] = mouseSprites[i];
        }

        GameManager.Instance.OnPlayerBoughtUpgrade += upgrade =>
        {
            if (mouseUpgradesTexturesDictionary.TryGetValue(upgrade, out Sprite sprite))
            {
                mouseRenderer.sprite = sprite;
            }
            else
            {
                Debug.LogWarning($"No sprite found for upgrade {upgrade}");
            }
        };
    }

    void Update()
    {
        HandleAutoClick();
    }
    
    public void HandleHarvest()
    {
        
        if (Input.GetMouseButtonDown(0))
        {
            enableAutoClick = true;
            HarvestDown();
        }

        if (Input.GetMouseButtonUp(0))
        {
            enableAutoClick = false;
            HarvestUp();
        }
    }

    void HarvestDown()
    {
        if (Physics.Raycast(Camera.main.transform.position,Camera.main.transform.forward, out RaycastHit hit, harvestDistance))
        {
            if (hit.transform.GetComponent<ResourceMaterial>())
            {
                int harvestDamage = GameManager.Instance.GetClickDamage();
                hit.transform.GetComponent<ResourceMaterial>().HarvestMaterial(Mathf.FloorToInt(harvestDamage));
                lastResourceMaterial = hit.transform.GetComponent<ResourceMaterial>();
                    
                OnHarvestResourceMaterial?.Invoke(lastResourceMaterial);
            }
        }
    }

    void HarvestUp()
    {
        if (lastResourceMaterial)
        {
            lastResourceMaterial.BounceObject();

            lastResourceMaterial = null;
        }
    }
    
    void HandleAutoClick()
    {
        if (!enableAutoClick)
        {
            autoClickTimer = 0f;
            return;
        }
        autoClickTimer += Time.deltaTime;

        if (autoClickTimer >= 1f / autoClickRate)
        {
            autoClickTimer = 0f;
            HarvestDown();
        }
    }

}