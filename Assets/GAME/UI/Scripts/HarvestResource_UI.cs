using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // <--- Asegúrate de tener DOTween

public class HarvestResource_UI : MonoBehaviour
{
    PlayerHarvest playerHarvest;

    public GameObject harvestResourceUIPanel;

    public Image fillCircleImage;
    public TextMeshProUGUI resourceNameText;
    public TextMeshProUGUI resourceHealthText;

    bool isHidden = false;
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
        if (isHidden)
        {
            fillCircleImage.fillAmount = (float)resourceMaterial.currentResourceHealth / resourceMaterial.resourceHealth;
        }

        isHidden = false;

        resourceNameText.text = resourceMaterial.resourceName;
        resourceHealthText.text = resourceMaterial.currentResourceHealth.ToString();

        float targetFill = resourceMaterial.currentResourceHealth > 0
            ? (float)resourceMaterial.currentResourceHealth / resourceMaterial.resourceHealth
            : 0;

        // Animar fillAmount con DOTween
        DOTween.Kill(fillCircleImage); // Cancela cualquier tween anterior
        fillCircleImage.DOFillAmount(targetFill, 0.1f).SetEase(Ease.OutQuad); // 0.5s de animación

        // Mostrar/ocultar panel
        harvestResourceUIPanel.SetActive(resourceMaterial.currentResourceHealth > 0);

        // Cancelar cualquier hide pendiente
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
            isHidden = true;
        }
        catch (TaskCanceledException)
        {
        }
    }
}