using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

public class UpgradeController : MonoBehaviour
{
    private static readonly int UnscaledTime = Shader.PropertyToID("_UnscaledTime");
    private static readonly int Disolve = Shader.PropertyToID("_Disolve");
    public Material backgroundMaterial;
    public Volume upgradeVolume;
    public AudioMixerSnapshot mainSnapshot;
    public AudioMixerSnapshot upgradeSnapshot;
    [Space]
    public Transform upgradesLayoutParent;
    public RectTransform upgradesHint;
    public UpgradeHintController upgradeHintController;
    [Header("Sound")]
    public AudioClip selectSound;
    public AudioClip upgradeSound;

    private Upgrade _selectedUpgrade = null;
    private UpgradeButton[] _upgradeButtons;
    
    float _iUpgradeHintYPos = 0;
    Tweener _backgroundTweener = null;
    bool _isShowing = false;
    float _timeScale = 1;
    float _volumeWeight = 0;
    readonly float _sensitivity = 50;
    
    Transform[] _upgradesTransforms;
    Vector3[] _upgradesInitialPositions;

    private void Start()
    {
        _iUpgradeHintYPos = upgradesHint.position.y;

        _upgradeButtons = upgradesLayoutParent.GetComponentsInChildren<UpgradeButton>(true);
        for (int i = 0; i < _upgradeButtons.Length; i++)
            _upgradeButtons[i].SetUpgradeController(this);
        
        // Transforms
        _upgradesTransforms = new Transform[upgradesLayoutParent.childCount];
        for(int i = 0; i < upgradesLayoutParent.childCount; i++)
        {
            _upgradesTransforms[i] = upgradesLayoutParent.GetChild(i);
        }        
        // Positions
        _upgradesInitialPositions = new Vector3[_upgradesTransforms.Length];
        for (int i = 0; i < _upgradesTransforms.Length; i++)
            _upgradesInitialPositions[i] = _upgradesTransforms[i].position;

        HideAllMenu();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
            ToggleUpgradeUI();
        
        backgroundMaterial.SetFloat(UnscaledTime,Time.unscaledTime);
        Time.timeScale = _timeScale;
        upgradeVolume.weight = _volumeWeight;
        HandleDragToMove();
    }

    /*
     * Upgrade Scene Movement
     */
    void HandleDragToMove()
    {
        if (Input.GetMouseButton(0))
        {
            for (int i = 0; i < _upgradesTransforms.Length; i++)
                _upgradesTransforms[i].position += new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0) * _sensitivity;
        }
    }

    /*
     * Interactions
     */
    public void BuyUpgrade(UpgradeButton upgradeButton)
    {
        Debug.Log("Buying upgrade");
        if(_selectedUpgrade == null)
            return;
        Debug.Log("_selectedUpgrade");

        upgradeButton.DisableButton();

        GameManager.Instance.AddUpgrade(_selectedUpgrade);
        AudioManager.Instance.PlayOneShot2D(upgradeSound).Play();
        _selectedUpgrade = null;
        
        upgradeButton.transform.DOShakeScale(1f, Vector3.one * 0.25f).SetUpdate(true);
    }
    
    public void SelectUpgradesHint(Upgrade upgrade)
    {
        _selectedUpgrade = upgrade;
        upgradeHintController.SetupUpgradeHint(upgrade);
    }

    
    /*
     * Show/Hide Menus
     */
    public void ToggleUpgradeUI()
    {
        if(_backgroundTweener != null)return;
        
        _isShowing = !_isShowing;

        if (_isShowing)
            ShowUpgradeUI();
        else
            HideUpgradeUI();
    }
    
    [ContextMenu("Hide All!")]
    void HideAllMenu()
    {
        _isShowing = false;
        
        backgroundMaterial.SetFloat(Disolve,0);
        _volumeWeight = 0;
        upgradeVolume.weight = 0;

        upgradesHint.gameObject.SetActive(false);
        
        HideUpgradesElementsWithoutAnimation();
    }
    
    [ContextMenu("Show All!")]
    void ShowAllMenu()
    {
        _isShowing = true;
        
        backgroundMaterial.SetFloat(Disolve,1);
        _volumeWeight = 1;
        upgradeVolume.weight = _volumeWeight;

        upgradesHint.gameObject.SetActive(true);
        
        ShowUpgradesElementsWithoutAnimation();
    }

    public void ShowUpgradeUI()
    {
        for (int i = 0; i < _upgradesTransforms.Length; i++)
            _upgradesTransforms[i].position = _upgradesInitialPositions[i];
        
        _backgroundTweener = backgroundMaterial
            .DOFloat(1f, Disolve, 1.25f)
            .SetEase(Ease.Linear)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _backgroundTweener = null;
                ShowUpgradesElements();
                
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            });

        DOTween.To(() => _timeScale, x => _timeScale = x, 0f, 1f)
            .SetEase(Ease.OutCirc)
            .SetUpdate(true);
        
        DOTween.To(() => _volumeWeight, x => _volumeWeight = x, 1f, .5f)
            .SetEase(Ease.InCirc)
            .SetUpdate(true);
        
        upgradeSnapshot.TransitionTo(.25f);
    }
    
    public void ShowUpgradeHint()
    {
        upgradesHint.gameObject.SetActive(true);
    }
    
    public void HideUpgradeHint()
    {
        upgradesHint.gameObject.SetActive(false);
    }

    public void HideUpgradeUI()
    {
        HideUpgradesElements();
        
        upgradesHint.gameObject.SetActive(false);

        _backgroundTweener = backgroundMaterial
            .DOFloat(0f, "_Disolve", 1f)
            .SetEase(Ease.Linear)
            .SetUpdate(true)
            .OnComplete(() =>
            {
                _backgroundTweener = null;
                
                DOTween.To(() => _timeScale, x => _timeScale = x, 1f, .5f)
                    .SetEase(Ease.InOutCirc)
                    .SetUpdate(true);
            });
        DOTween.To(() => _volumeWeight, x => _volumeWeight = x, 0f, 1f)
            .SetEase(Ease.InOutCirc)
            .SetUpdate(true);
        
        mainSnapshot.TransitionTo(1f);
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void ShowUpgradesElements()
    {
        for (int i = 0; i < upgradesLayoutParent.childCount; i++)
        {
            Transform upgrade = upgradesLayoutParent.GetChild(i);
            upgrade.DOScale(Vector3.one, .25f).SetUpdate(true);
        }
    }
    
    void HideUpgradesElements()
    {
        for (int i = 0; i < upgradesLayoutParent.childCount; i++)
        {
            Transform upgrade = upgradesLayoutParent.GetChild(i);
            upgrade.DOScale(Vector3.zero, .1f).SetUpdate(true);
        }
    }
    
    void ShowUpgradesElementsWithoutAnimation()
    {
        for (int i = 0; i < upgradesLayoutParent.childCount; i++)
            upgradesLayoutParent.GetChild(i).localScale = Vector3.one;
    }
    
    void HideUpgradesElementsWithoutAnimation()
    {
        for (int i = 0; i < upgradesLayoutParent.childCount; i++)
            upgradesLayoutParent.GetChild(i).localScale = Vector3.zero;
    }

}
