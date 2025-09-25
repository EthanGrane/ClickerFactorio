using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class UpgradeController : MonoBehaviour
{
    public Material backgroundMaterial;
    public Volume upgradeVolume;
    public AudioMixerSnapshot mainSnapshot;
    public AudioMixerSnapshot upgradeSnapshot;
    [Space]
    public Transform UpgradesLayoutParent;
    public RectTransform UpgradesHint;
    public UpgradeHintController upgradeHintController;
    [Header("Sound")]
    public AudioClip selectSound;
    public AudioClip upgradeSound;

    private Upgrade selectedUpgrade = null;
    private UpgradeButton[] upgradeButton;
    
    float iUpgradeHintYPos = 0;
    Tweener backgroundTweener = null;
    bool isShowing = false;
    float timeScale = 1;
    float volumeWeight = 0;
    float sensitivity = 50;
    
    Transform[] upgradesTransforms;
    Vector3[] upgradesInitialPositions;

    private void Start()
    {
        iUpgradeHintYPos = UpgradesHint.position.y;

        upgradeButton = UpgradesLayoutParent.GetComponentsInChildren<UpgradeButton>();
        for (int i = 0; i < upgradeButton.Length; i++)
            upgradeButton[i].SetUpgradeController(this);
        
        // Transforms
        upgradesTransforms = new Transform[UpgradesLayoutParent.childCount];
        for(int i = 0; i < UpgradesLayoutParent.childCount; i++)
        {
            upgradesTransforms[i] = UpgradesLayoutParent.GetChild(i);
        }        
        // Positions
        upgradesInitialPositions = new Vector3[upgradesTransforms.Length];
        for (int i = 0; i < upgradesTransforms.Length; i++)
            upgradesInitialPositions[i] = upgradesTransforms[i].position;

        HideAllMenu();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
            ToggleUpgradeUI();
        
        backgroundMaterial.SetFloat("_UnscaledTime",Time.unscaledTime);
        Time.timeScale = timeScale;
        upgradeVolume.weight = volumeWeight;
        HandleDragToMove();
    }

    /*
     * Upgrade Scene Movement
     */
    void HandleDragToMove()
    {
        if (Input.GetMouseButton(0))
        {
            for (int i = 0; i < upgradesTransforms.Length; i++)
                upgradesTransforms[i].position += new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0) * sensitivity;
        }
    }

    /*
     * Interactions
     */
    public void SelectUpgradesHint(Upgrade upgrade, UpgradeButton upgradeButton)
    {
        if (selectedUpgrade != null && selectedUpgrade == upgrade)
        {
            GameManager.Instance.AddUpgrade(upgrade);
            upgradeButton.DisableButton();
            selectedUpgrade = null;
            AudioManager.Instance.PlayOneShot2D(upgradeSound).Play();
        }
        else
            AudioManager.Instance.PlayOneShot2D(selectSound).Play();;
        
        selectedUpgrade = upgrade;
        UpgradesHint.gameObject.SetActive(true);
        upgradeHintController.SetupUpgradeHint(upgrade);
    }

    
    /*
     * Show/Hide Menus
     */
    public void ToggleUpgradeUI()
    {
        if(backgroundTweener != null)return;
        
        isShowing = !isShowing;

        if (isShowing)
            ShowUpgradeUI();
        else
            HideUpgradeUI();
    }
    
    [ContextMenu("Hide All!")]
    void HideAllMenu()
    {
        isShowing = false;
        
        backgroundMaterial.SetFloat("_Disolve",0);
        volumeWeight = 0;
        upgradeVolume.weight = 0;

        UpgradesHint.gameObject.SetActive(false);
        
        HideUpgradesElementsWithoutAnimation();
    }

    public void ShowUpgradeUI()
    {
        for (int i = 0; i < upgradesTransforms.Length; i++)
            upgradesTransforms[i].position = upgradesInitialPositions[i];
        
        backgroundTweener = backgroundMaterial
            .DOFloat(1f, "_Disolve", 1.25f)
            .SetEase(Ease.Linear)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                backgroundTweener = null;
                ShowUpgradesElements();
                
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            });

        DOTween.To(() => timeScale, x => timeScale = x, 0f, 1f)
            .SetEase(Ease.OutCirc)
            .SetUpdate(true);
        
        DOTween.To(() => volumeWeight, x => volumeWeight = x, 1f, .5f)
            .SetEase(Ease.InCirc)
            .SetUpdate(true);
        
        upgradeSnapshot.TransitionTo(.25f);
    }
    
    public void HideUpgradeHint()
    {
        UpgradesHint.gameObject.SetActive(false);
    }

    public void HideUpgradeUI()
    {
        HideUpgradesElements();
        
        UpgradesHint.gameObject.SetActive(false);

        backgroundTweener = backgroundMaterial
            .DOFloat(0f, "_Disolve", 1f)
            .SetEase(Ease.Linear)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                backgroundTweener = null;
                
                DOTween.To(() => timeScale, x => timeScale = x, 1f, .5f)
                    .SetEase(Ease.InOutCirc)
                    .SetUpdate(true);
            });
        DOTween.To(() => volumeWeight, x => volumeWeight = x, 0f, 1f)
            .SetEase(Ease.InOutCirc)
            .SetUpdate(true);
        
        mainSnapshot.TransitionTo(1f);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void ShowUpgradesElements()
    {
        for (int i = 0; i < UpgradesLayoutParent.childCount; i++)
        {
            Transform upgrade = UpgradesLayoutParent.GetChild(i);
            upgrade.DOScale(Vector3.one, .25f).SetUpdate(true);
        }
    }
    
    void HideUpgradesElements()
    {
        for (int i = 0; i < UpgradesLayoutParent.childCount; i++)
        {
            Transform upgrade = UpgradesLayoutParent.GetChild(i);
            upgrade.DOScale(Vector3.zero, .1f).SetUpdate(true);
        }
    }

    [ContextMenu("Show Upgrades Elements")]
    void ShowUpgradesElementsWithoutAnimation()
    {
        for (int i = 0; i < UpgradesLayoutParent.childCount; i++)
            UpgradesLayoutParent.GetChild(i).localScale = Vector3.one;
    }
    
    [ContextMenu("Hide Upgrades Elements")]
    void HideUpgradesElementsWithoutAnimation()
    {
        for (int i = 0; i < UpgradesLayoutParent.childCount; i++)
            UpgradesLayoutParent.GetChild(i).localScale = Vector3.zero;
    }

}
