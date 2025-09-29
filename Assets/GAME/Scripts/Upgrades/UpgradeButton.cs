using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour
{
    public Button upgradeButton;
    public Upgrade upgrade;
    public UpgradeButton nextUpgradeButton;
    
    UpgradeController upgradeController;
    ButtonEvents buttonEvents;

    private void Awake()
    {
        if (upgradeButton == null)
            upgradeButton = GetComponentInChildren<Button>();
    }

    private void Start()
    {
        // Ocultamos todos los upgrades al inicio
        gameObject.SetActive(false);

        // Si es el primero de la cadena, lo mostramos y lo desbloqueamos
        if (IsFirstUpgrade())
        {
            gameObject.SetActive(true);
            upgradeButton.interactable = true;
        }
        else
        {
            upgradeButton.interactable = false;
        }

        // Eventos hover
        if (buttonEvents == null)
        {
            buttonEvents = GetComponentInChildren<ButtonEvents>();
            buttonEvents.onPointerEnter += (PointerEventData e) =>
            {
                upgradeController.SelectUpgradesHint(upgrade);
                upgradeController.ShowUpgradeHint();
            };

            buttonEvents.onPointerExit += (PointerEventData e) =>
            {
                upgradeController.HideUpgradeHint();
            };
        }

        // Evento click
        upgradeButton.onClick.AddListener(UpgradeButtonClicked);
    }
    
    void UpgradeButtonClicked()
    {
        if (upgradeController.BuyUpgrade(this))
        {

            // Se desactiva este botón
            DisableButton();

            // Se activa y muestra el siguiente en la cadena
            if (nextUpgradeButton != null)
                nextUpgradeButton.ShowAndEnable();
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

    public void EnableButton()
    {
        upgradeButton.interactable = true;
    }

    public void ShowAndEnable()
    {
        gameObject.SetActive(true);
        EnableButton();
    }

    bool IsFirstUpgrade()
    {
        // Si no tiene un "anterior", lo consideramos el primero
        return transform.GetSiblingIndex() == 0;
    }
}
