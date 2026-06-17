using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [Header("Currency")]
    [SerializeField] private TextMeshProUGUI _gemsText;
    [SerializeField] private TextMeshProUGUI _coinsText;
    [SerializeField] private TextMeshProUGUI _leavesText;

    [Header("Energy")]
    [SerializeField] private TextMeshProUGUI _energyText;       // e.g. "42/60"
    [SerializeField] private TextMeshProUGUI _energyTimerText;  // e.g. "4:32"

    [Header("Bond Indicators — one Image per EntityType")]
    [SerializeField] private Image _deerBondFill;
    [SerializeField] private Image _elephantBondFill;

    [Header("Ability State")]
    [SerializeField] private GameObject _suppressionOverlay;

    [Header("Player Hearts")]
    [SerializeField] private Image[] _heartImages;

    [Header("Mini-Boss HUD")]
    [SerializeField] private MiniBossHUD _miniBossHUD;

    [Header("Boss HP Bar (Chapter Bosses)")]
    [SerializeField] private Image           _bossHPFill;           // Image with fillMethod=Horizontal
    [SerializeField] private GameObject      _bossHPRoot;           // parent to show/hide
    [SerializeField] private Image           _bossPhaseFlash;       // overlay that flashes on phase change

    [Header("Combo Meter")]
    [SerializeField] private TextMeshProUGUI _comboText;
    [SerializeField] private GameObject      _comboRoot;

    [Header("Purify Burst Prompt")]
    [SerializeField] private GameObject      _purifyBurstPrompt;    // press V / RB prompt

    [Header("Weapon Display")]
    [SerializeField] private TextMeshProUGUI _weaponNameText;
    [SerializeField] private Image[]         _weaponLevelPips;      // 3 pip images (filled = unlocked)

    [Header("Echo Fragment Counter")]
    [SerializeField] private TextMeshProUGUI _fragmentText;

    [Header("Cold Meter (Chapter 3)")]
    [SerializeField] private Image           _coldMeterFill;        // Image with fillMethod=Horizontal
    [SerializeField] private GameObject      _coldMeterRoot;

    private EntityController _activeMiniB;

    private void OnEnable()
    {
        GameEvents.OnBondLevelUp          += HandleBondLevelUp;
        GameEvents.OnHealComplete         += HandleHealComplete;
        GameEvents.OnStateChange          += HandleStateChange;
        GameEvents.OnMiniBossActivated    += HandleMiniBossActivated;
        GameEvents.OnMiniBossDefeated     += HandleMiniBossDefeated;
        GameEvents.OnPlayerDamaged        += HandlePlayerDamaged;
        GameEvents.OnBossHPChanged        += HandleBossHPChanged;
        GameEvents.OnBossPhaseChanged     += HandleBossPhaseChanged;
        GameEvents.OnBossDefeated         += HandleBossDefeated;
        GameEvents.OnComboChanged         += HandleComboChanged;
        GameEvents.OnWeaponUpgraded       += HandleWeaponUpgraded;
        GameEvents.OnFragmentCollected    += HandleFragmentCollected;
        GameEvents.OnColdMeterChanged     += HandleColdMeterChanged;
        GameEvents.OnPurifyBurstActivated += HandlePurifyBurstActivated;
        GameEvents.OnGemsChanged          += HandleGemsChanged;
        GameEvents.OnCoinsChanged         += HandleCoinsChanged;
        GameEvents.OnLeavesChanged        += HandleLeavesChanged;
        GameEvents.OnEnergyChanged        += HandleEnergyChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnBondLevelUp          -= HandleBondLevelUp;
        GameEvents.OnHealComplete         -= HandleHealComplete;
        GameEvents.OnStateChange          -= HandleStateChange;
        GameEvents.OnMiniBossActivated    -= HandleMiniBossActivated;
        GameEvents.OnMiniBossDefeated     -= HandleMiniBossDefeated;
        GameEvents.OnPlayerDamaged        -= HandlePlayerDamaged;
        GameEvents.OnBossHPChanged        -= HandleBossHPChanged;
        GameEvents.OnBossPhaseChanged     -= HandleBossPhaseChanged;
        GameEvents.OnBossDefeated         -= HandleBossDefeated;
        GameEvents.OnComboChanged         -= HandleComboChanged;
        GameEvents.OnWeaponUpgraded       -= HandleWeaponUpgraded;
        GameEvents.OnFragmentCollected    -= HandleFragmentCollected;
        GameEvents.OnColdMeterChanged     -= HandleColdMeterChanged;
        GameEvents.OnPurifyBurstActivated -= HandlePurifyBurstActivated;
        GameEvents.OnGemsChanged          -= HandleGemsChanged;
        GameEvents.OnCoinsChanged         -= HandleCoinsChanged;
        GameEvents.OnLeavesChanged        -= HandleLeavesChanged;
        GameEvents.OnEnergyChanged        -= HandleEnergyChanged;
    }

    public void SetSuppressionOverlay(bool active)
    {
        if (_suppressionOverlay != null) _suppressionOverlay.SetActive(active);
    }

    // ── Existing handlers ─────────────────────────────────────────────────────

    private void HandleBondLevelUp(EntityType type, int level)
    {
        float fill = level / 3f;
        switch (type)
        {
            case EntityType.Deer:     if (_deerBondFill != null)     _deerBondFill.fillAmount     = fill; break;
            case EntityType.Elephant: if (_elephantBondFill != null) _elephantBondFill.fillAmount = fill; break;
        }
    }

    private void HandleHealComplete(BiomeArea area) => CurrencyManager.Instance?.AddLeaves(10);

    private void HandleStateChange(EntityController entity, EntityState state)
    {
        if (_miniBossHUD != null && entity == _activeMiniB)
            _miniBossHUD.OnStateChanged(state);
    }

    private void HandleMiniBossActivated(MiniBossType type)
    {
        var controller = Object.FindFirstObjectByType<MiniBossController>();
        _activeMiniB = controller != null ? controller.GetComponent<EntityController>() : null;
        _miniBossHUD?.OnMiniBossActivated(type);
    }

    private void HandleMiniBossDefeated(MiniBossType type)
    {
        _activeMiniB = null;
        _miniBossHUD?.OnMiniBossDefeated();
    }

    private void HandlePlayerDamaged(int current, int max)
    {
        if (_heartImages == null) return;
        for (int i = 0; i < _heartImages.Length; i++)
        {
            if (_heartImages[i] != null)
                _heartImages[i].enabled = i < current;
        }
    }

    // ── New combat handlers ───────────────────────────────────────────────────

    private void HandleBossHPChanged(float normalized)
    {
        if (_bossHPRoot != null) _bossHPRoot.SetActive(true);
        if (_bossHPFill != null) _bossHPFill.fillAmount = normalized;
    }

    private void HandleBossPhaseChanged(int phase)
    {
        // Flash phase-change overlay
        if (_bossPhaseFlash != null)
            StartCoroutine(FlashRoutine());

        // Show Purify Burst prompt on Phase 3
        if (_purifyBurstPrompt != null)
            _purifyBurstPrompt.SetActive(phase >= 3);
    }

    private void HandleBossDefeated(string bossId)
    {
        if (_bossHPRoot != null)     _bossHPRoot.SetActive(false);
        if (_purifyBurstPrompt != null) _purifyBurstPrompt.SetActive(false);
    }

    private void HandleComboChanged(int combo)
    {
        if (_comboRoot == null) return;
        _comboRoot.SetActive(combo > 0);
        if (_comboText != null)
            _comboText.text = combo > 0 ? $"x{combo}" : "";
    }

    private void HandleWeaponUpgraded(WeaponData data)
    {
        if (_weaponNameText != null)
            _weaponNameText.text = $"{data.displayName} Lv{data.currentLevel + 1}";

        if (_weaponLevelPips != null)
        {
            for (int i = 0; i < _weaponLevelPips.Length; i++)
            {
                if (_weaponLevelPips[i] != null)
                    _weaponLevelPips[i].enabled = i < data.currentLevel;
            }
        }
    }

    private void HandleFragmentCollected(int total)
    {
        if (_fragmentText != null)
            _fragmentText.text = $"Fragments: {total}";
    }

    private void HandleColdMeterChanged(float value)
    {
        if (_coldMeterRoot == null) return;
        _coldMeterRoot.SetActive(value > 0f);
        if (_coldMeterFill != null) _coldMeterFill.fillAmount = value;
    }

    private void HandlePurifyBurstActivated()
    {
        if (_purifyBurstPrompt != null) _purifyBurstPrompt.SetActive(false);
    }

    private void HandleGemsChanged(int amount)
    {
        if (_gemsText != null) _gemsText.text = $"{amount}";
    }

    private void HandleCoinsChanged(int amount)
    {
        if (_coinsText != null) _coinsText.text = $"{amount}";
    }

    private void HandleLeavesChanged(int amount)
    {
        if (_leavesText != null) _leavesText.text = $"x{amount}";
    }

    private void HandleEnergyChanged(int current, int max)
    {
        if (_energyText != null)
            _energyText.text = current >= max ? $"{max}/{max}" : $"{current}/{max}";

        if (_energyTimerText != null)
            _energyTimerText.gameObject.SetActive(current < max);
    }

    private void Update()
    {
        // Refresh energy countdown every frame when not full
        if (_energyTimerText == null || EnergyManager.Instance == null) return;
        if (EnergyManager.Instance.IsFull) return;

        //TimeSpan t = EnergyManager.Instance.TimeUntilNextEnergy();
        //_energyTimerText.text = $"{t.Minutes}:{t.Seconds:D2}";
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        if (_bossPhaseFlash == null) yield break;
        _bossPhaseFlash.gameObject.SetActive(true);
        float t = 0f;
        Color c = _bossPhaseFlash.color;
        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            _bossPhaseFlash.color = new Color(c.r, c.g, c.b, 1f - t);
            yield return null;
        }
        _bossPhaseFlash.gameObject.SetActive(false);
        _bossPhaseFlash.color = c;
    }
}
