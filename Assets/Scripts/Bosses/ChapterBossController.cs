using UnityEngine;

// Abstract base for all chapter bosses. Subclasses implement per-phase AI loops
// and the defeat cutscene. BossHealth drives phase transitions via GameEvents.
[RequireComponent(typeof(BossHealth), typeof(EntityController), typeof(EntityStateMachine))]
public abstract class ChapterBossController : MonoBehaviour
{
    [Header("Activation")]
    [SerializeField] protected float _activationRange = 14f;

    public string BossId => gameObject.name;

    protected BossHealth         _bossHealth;
    protected EntityController   _entity;
    protected EntityStateMachine _sm;
    protected Transform          _player;
    protected Rigidbody2D        _playerRb;
    protected bool               _active;
    protected bool               _defeated;

    protected virtual void Awake()
    {
        _bossHealth = GetComponent<BossHealth>();
        _entity     = GetComponent<EntityController>();
        _sm         = GetComponent<EntityStateMachine>();
    }

    protected virtual void OnEnable()
    {
        GameEvents.OnBossPhaseChanged += HandlePhaseChanged;
        GameEvents.OnBossDefeated     += HandleDefeated;
        GameEvents.OnStateChange      += HandleStateChange;
    }

    protected virtual void OnDisable()
    {
        GameEvents.OnBossPhaseChanged -= HandlePhaseChanged;
        GameEvents.OnBossDefeated     -= HandleDefeated;
        GameEvents.OnStateChange      -= HandleStateChange;
    }

    protected virtual void Update()
    {
        if (_active || _defeated) return;

        if (_player == null)
        {
            var pc = FindFirstObjectByType<PlayerController>();
            if (pc != null) { _player = pc.transform; _playerRb = pc.GetComponent<Rigidbody2D>(); }
        }

        if (_player != null &&
            Vector2.Distance(transform.position, _player.position) < _activationRange)
            Activate();
    }

    protected virtual void Activate()
    {
        _active = true;
        GameEvents.RaiseMiniBossActivated(MiniBossType.CaveMaw);  // reuse HUD activation
        OnActivated();
    }

    // ── Subclass hooks ────────────────────────────────────────────────────────

    protected abstract void OnActivated();
    protected abstract void OnPhaseChanged(int phase);
    protected abstract void OnDefeated();

    // ── Event handlers ────────────────────────────────────────────────────────

    private void HandlePhaseChanged(int phase)
    {
        if (_defeated) return;
        OnPhaseChanged(phase);
    }

    private void HandleDefeated(string bossId)
    {
        if (_defeated || bossId != BossId) return;
        _defeated = true;
        StopAllCoroutines();
        OnDefeated();
    }

    // Match-3 can stabilise the entity directly without going through BossHealth —
    // treat that as a defeat so the AI loop stops.
    private void HandleStateChange(EntityController entity, EntityState state)
    {
        if (_defeated || entity != _entity || state != EntityState.Stable) return;
        _defeated = true;
        StopAllCoroutines();
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = Vector2.zero;
        OnDefeated();
    }
}
