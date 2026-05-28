using System.Collections;
using UnityEngine;

// Attach alongside EntityController + EntityStateMachine to turn any entity into a chapter mini-boss.
// Each MiniBossType gets its own phase-scaled AI loop. The emotional-state machine is still the
// health model — Stable = defeated.
[RequireComponent(typeof(EntityController), typeof(EntityStateMachine))]
public class MiniBossController : MonoBehaviour
{
    [Header("Type")]
    [SerializeField] private MiniBossType _type;

    [Header("Activation")]
    [SerializeField] private float _activationRange = 12f;

    [Header("Cave Maw — Sound Detection")]
    [Tooltip("Player horizontal speed above this value counts as audible noise.")]
    [SerializeField] private float _noiseThreshold = 0.5f;

    [Header("Cave Maw / Baby Deer — Defeat Audio")]
    [SerializeField] private AudioClip _defeatClip;

    [Header("Ice Hollow Gorilla — Crystal Shield")]
    [Tooltip("Child trigger Collider2D + SuppressionField that activates during immunity window.")]
    [SerializeField] private SuppressionField _crystalShieldField;
    [SerializeField] private float            _immuneDuration     = 3f;
    [SerializeField] private float            _vulnerableDuration = 2f;

    [Header("Ice Hollow Gorilla — Ground Slam")]
    [SerializeField] private SuppressionField _slamFieldLeft;
    [SerializeField] private SuppressionField _slamFieldRight;
    [SerializeField] private float            _slamInterval = 6f;
    [SerializeField] private float            _slamDuration = 2f;

    private EntityController   _entity;
    private EntityStateMachine _sm;
    private Transform          _player;
    private Rigidbody2D        _playerRb;
    private Transform          _panicTarget;  // reusable waypoint for Baby Deer erratic dashes
    private bool               _active;
    private bool               _defeated;

    private void Awake()
    {
        _entity      = GetComponent<EntityController>();
        _sm          = GetComponent<EntityStateMachine>();
        _panicTarget = new GameObject($"{name}_PanicTarget").transform;
        _panicTarget.position = transform.position;
    }

    private void OnEnable()  => GameEvents.OnStateChange += HandleStateChange;
    private void OnDisable() => GameEvents.OnStateChange -= HandleStateChange;

    private void OnDestroy()
    {
        if (_panicTarget != null) Destroy(_panicTarget.gameObject);
    }

    private void Update()
    {
        if (_active || _defeated) return;

        if (_player == null)
        {
            var pc = Object.FindFirstObjectByType<PlayerController>();
            if (pc != null)
            {
                _player   = pc.transform;
                _playerRb = pc.GetComponent<Rigidbody2D>();
            }
        }

        if (_player != null &&
            Vector2.Distance(transform.position, _player.position) < _activationRange)
            Activate();
    }

    private void Activate()
    {
        _active = true;
        GameEvents.RaiseMiniBossActivated(_type);

        switch (_type)
        {
            case MiniBossType.CaveMaw:          StartCoroutine(CaveMawLoop());        break;
            case MiniBossType.BabyDeer:         StartCoroutine(BabyDeerLoop());       break;
            case MiniBossType.IceHollowGorilla: StartCoroutine(IceHollowGorillaLoop()); break;
        }
    }

    private void HandleStateChange(EntityController entity, EntityState state)
    {
        if (entity != _entity || _defeated) return;
        if (state == EntityState.Stable) OnDefeated();
    }

    // ── CAVE MAW ────────────────────────────────────────────────────────────────

    private IEnumerator CaveMawLoop()
    {
        while (!_defeated)
        {
            if (_player == null) { yield return null; continue; }

            float detectionRadius = _sm.Current switch
            {
                EntityState.Turbulent  => 10f,
                EntityState.Agitated   =>  8f,
                EntityState.Distressed =>  6f,
                _                      =>  4f
            };

            float dist  = Vector2.Distance(transform.position, _player.position);
            float noise = _playerRb != null ? Mathf.Abs(_playerRb.linearVelocity.x) : 0f;

            if (dist < detectionRadius && noise > _noiseThreshold)
            {
                // 1s telegraph gives player a stealth window before the charge
                yield return new WaitForSeconds(1f);
                if (_defeated) break;

                _entity.TriggerCharge(_player);

                float cooldown = _sm.Current == EntityState.Turbulent ? 3f : 5f;
                yield return new WaitForSeconds(cooldown);
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    // ── BABY DEER ───────────────────────────────────────────────────────────────

    private IEnumerator BabyDeerLoop()
    {
        // Charge immediately on activation
        PanicCharge();

        while (!_defeated)
        {
            float interval = _sm.Current switch
            {
                EntityState.Turbulent  => 1.5f,
                EntityState.Agitated   => 2.0f,
                EntityState.Distressed => 2.8f,
                _                      => 4.0f
            };

            yield return new WaitForSeconds(interval);
            if (_defeated) break;
            PanicCharge();
        }
    }

    private void PanicCharge()
    {
        Vector2 randomOffset = Random.insideUnitCircle.normalized * 3f;
        _panicTarget.position = (Vector2)transform.position + randomOffset;
        _entity.TriggerCharge(_panicTarget);
    }

    // ── ICE HOLLOW GORILLA ──────────────────────────────────────────────────────

    private IEnumerator IceHollowGorillaLoop()
    {
        StartCoroutine(CrystalShieldLoop());
        StartCoroutine(GroundSlamLoop());
        yield break;
    }

    private IEnumerator CrystalShieldLoop()
    {
        SpriteRenderer sprite = GetComponentInChildren<SpriteRenderer>();

        while (!_defeated)
        {
            // Immunity window — shorter as the gorilla calms down
            float t           = 1f - (int)_sm.Current / 5f;
            float immuneTime  = Mathf.Lerp(_immuneDuration, _immuneDuration * 0.5f, t);

            if (_crystalShieldField != null) _crystalShieldField.gameObject.SetActive(true);
            if (sprite != null) sprite.color = Color.white;  // crystal-white visual cue

            yield return new WaitForSeconds(immuneTime);
            if (_defeated) break;

            if (_crystalShieldField != null) _crystalShieldField.gameObject.SetActive(false);
            // Sprite color reverts on the next entity state transition via EntityStateMachine.ApplyVisuals

            yield return new WaitForSeconds(_vulnerableDuration);
        }

        if (_crystalShieldField != null) _crystalShieldField.gameObject.SetActive(false);
    }

    private IEnumerator GroundSlamLoop()
    {
        while (!_defeated)
        {
            float interval = _sm.Current switch
            {
                EntityState.Turbulent => _slamInterval * 0.6f,
                EntityState.Agitated  => _slamInterval * 0.8f,
                _                     => _slamInterval
            };

            yield return new WaitForSeconds(interval);
            if (_defeated) break;

            StartCoroutine(DoSlam());
        }
    }

    private IEnumerator DoSlam()
    {
        if (_slamFieldLeft  != null) _slamFieldLeft.gameObject.SetActive(true);
        if (_slamFieldRight != null) _slamFieldRight.gameObject.SetActive(true);
        yield return new WaitForSeconds(_slamDuration);
        if (_slamFieldLeft  != null) _slamFieldLeft.gameObject.SetActive(false);
        if (_slamFieldRight != null) _slamFieldRight.gameObject.SetActive(false);
    }

    // ── DEFEAT ──────────────────────────────────────────────────────────────────

    private void OnDefeated()
    {
        _defeated = true;
        StopAllCoroutines();

        if (_crystalShieldField != null) _crystalShieldField.gameObject.SetActive(false);
        if (_slamFieldLeft  != null) _slamFieldLeft.gameObject.SetActive(false);
        if (_slamFieldRight != null) _slamFieldRight.gameObject.SetActive(false);

        GameEvents.RaiseMiniBossDefeated(_type);

        switch (_type)
        {
            case MiniBossType.CaveMaw:
                // GDD: "Still searching…" whisper after defeat
                AudioManager.Instance?.PlaySFX(_defeatClip);
                break;
            case MiniBossType.BabyDeer:
                // GDD: Baby Deer becomes a companion helper
                GetComponent<DeerCompanion>()?.ActivateCompanionMode();
                break;
            case MiniBossType.IceHollowGorilla:
                // GDD: freed animals SFX
                AudioManager.Instance?.PlaySFX(_defeatClip);
                break;
        }
    }
}
