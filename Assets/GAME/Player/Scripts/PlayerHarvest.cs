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

    public void HandleHarvest()
    {
        if (Input.GetMouseButtonDown(0))
        {

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, harvestDistance))
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

        if (Input.GetMouseButtonUp(0))
        {
            if (lastResourceMaterial)
            {
                lastResourceMaterial.BounceObject();

                lastResourceMaterial = null;
            }
        }

    }
}