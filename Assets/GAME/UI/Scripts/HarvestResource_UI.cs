using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HarvestResource_UI : MonoBehaviour
{
    PlayerHarvest playerHarvest;

    public GameObject harvestResourceUIPanel;
    
    public Image fillCircleImage;
    public TextMeshProUGUI resourceNameText;
    public TextMeshProUGUI resourceHealthText;

    CancellationTokenSource cts;

    private void Awake()
    {
        playerHarvest = FindFirstObjectByType<PlayerHarvest>();
        playerHarvest.OnHarvestResourceMaterial += UpdateResourceInfo;
    }

    private void Start()
    {
        harvestResourceUIPanel.SetActive(false);
    }

    void UpdateResourceInfo(ResourceMaterial resourceMaterial)
    {
        resourceNameText.text = resourceMaterial.resourceName;
        resourceHealthText.text = resourceMaterial.currentResourceHealth.ToString();

        fillCircleImage.fillAmount = resourceMaterial.currentResourceHealth > 0
            ? Mathf.Lerp(0, 1, (float)resourceMaterial.currentResourceHealth / resourceMaterial.resourceHealth)
            : 0;

        if(resourceMaterial.currentResourceHealth > 0)
            harvestResourceUIPanel.SetActive(true);
        else
            harvestResourceUIPanel.SetActive(false);

        cts?.Cancel();
        cts = new CancellationTokenSource();

        _ = HideUI(cts.Token);
    }

    private async Task HideUI(CancellationToken token)
    {
        try
        {
            await Task.Delay(1000, token);
            harvestResourceUIPanel.SetActive(false);
        }
        catch (TaskCanceledException)
        {
        }
    }
}