using UnityEngine;

// Tracks the player's hit-chain combo count. Resets if no hit within the window.
// SpiralNunchaku (and any weapon) calls RegisterHit() on a successful strike.
// PlayerHealth calls ResetCombo() when player takes damage.
public class ComboMeter : MonoBehaviour
{
    public static ComboMeter Instance { get; private set; }

    [SerializeField] private float _comboWindow      = 1.5f;  // seconds between hits before reset
    [SerializeField] private int   _speedBonusThreshold = 5;
    [SerializeField] private int   _aerialBonusThreshold = 10;
    [SerializeField] private float _speedMultiplier  = 1.2f;

    public int   Count         { get; private set; }
    public bool  SpeedBonusActive  => Count >= _speedBonusThreshold;
    public bool  AerialBonusActive => Count >= _aerialBonusThreshold;

    private float           _resetTimer;
    private PlayerController _pc;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        _pc = FindFirstObjectByType<PlayerController>();
        GameEvents.OnPlayerDamaged += OnPlayerDamaged;
    }

    private void OnDestroy() => GameEvents.OnPlayerDamaged -= OnPlayerDamaged;

    private void Update()
    {
        if (Count <= 0) return;
        _resetTimer -= Time.deltaTime;
        if (_resetTimer <= 0f) ResetCombo();
    }

    public void RegisterHit()
    {
        Count++;
        _resetTimer = _comboWindow;
        GameEvents.RaiseComboChanged(Count);

        if (_pc != null)
            _pc.SetComboSpeedBonus(SpeedBonusActive ? _speedMultiplier : 1f);
    }

    public void ResetCombo()
    {
        if (Count == 0) return;
        Count = 0;
        _resetTimer = 0f;
        GameEvents.RaiseComboChanged(0);
        if (_pc != null) _pc.SetComboSpeedBonus(1f);
    }

    private void OnPlayerDamaged(int current, int max) => ResetCombo();
}
