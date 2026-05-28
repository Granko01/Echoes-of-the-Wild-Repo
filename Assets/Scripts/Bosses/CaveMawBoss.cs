using System.Collections;
using UnityEngine;

// Chapter 1 — Cave Realm — Boss 1: Cave Maw
// No eyes. Tracks SOUND only.
// Phase 1: Investigates nearest SoundSource; charges if player noise detected.
// Phase 2 (≤65%): Enrages — fast charges, destroys pillars, smaller arena.
// Phase 3 (≤25%): Confused, slows down, moments of peace. Purify Burst available.
public class CaveMawBoss : ChapterBossController
{
    [Header("Sound Detection")]
    [SerializeField] private float _hearingRange     = 12f;
    [SerializeField] private float _noiseThreshold   = 0.8f;
    [SerializeField] private float _investigateSpeed = 2.5f;

    [Header("Phase 1 — Hunt")]
    [SerializeField] private float _chargeWindup     = 1.2f;
    [SerializeField] private float _chargeCooldown   = 3f;

    [Header("Phase 2 — Enrage")]
    [SerializeField] private float _phase2ChargeCooldown = 1.8f;
    [SerializeField] private AudioClip _enrageClip;
    [SerializeField] private AudioClip _rockFallClip;

    [Header("Phase 3 — Confused")]
    [SerializeField] private float _confusedSpeed = 1.2f;
    [SerializeField] private float _peaceChance   = 0.35f;

    [Header("Audio")]
    [SerializeField] private AudioClip _roarClip;
    [SerializeField] private AudioClip _sniffClip;
    [SerializeField] private AudioClip _defeatClip;

    private Rigidbody2D _rb;
    private int         _currentPhase = 1;
    private float       _chargeTimer;

    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody2D>();
    }

    protected override void Update()
    {
        base.Update();

        // Register player noise as a sound source while CaveMaw is active
        if (!_active || _defeated || _player == null) return;
        float noise = _playerRb != null ? _playerRb.linearVelocity.magnitude : 0f;
        if (noise > _noiseThreshold)
        {
            SoundDetector.Instance?.RegisterSource(
                new SoundSource(_player.position, 0.5f, isPlayer: true));
        }
    }

    protected override void OnActivated()
    {
        AudioManager.Instance?.PlaySFX(_roarClip);
        StartCoroutine(BossLoop());
    }

    protected override void OnPhaseChanged(int phase)
    {
        _currentPhase = phase;
        if (phase == 2) AudioManager.Instance?.PlaySFX(_enrageClip);
    }

    protected override void OnDefeated()
    {
        AudioManager.Instance?.PlaySFX(_defeatClip);
        if (_rb != null) _rb.linearVelocity = Vector2.zero;
        // "Still searching..." whisper — assign voice clip in Inspector
    }

    // ── Main AI Loop ──────────────────────────────────────────────────────────

    private IEnumerator BossLoop()
    {
        while (!_defeated)
        {
            switch (_currentPhase)
            {
                case 1: yield return StartCoroutine(Phase1Tick()); break;
                case 2: yield return StartCoroutine(Phase2Tick()); break;
                case 3: yield return StartCoroutine(Phase3Tick()); break;
            }
            yield return null;
        }
    }

    // ── Phase 1: Silent Hunt ──────────────────────────────────────────────────

    private IEnumerator Phase1Tick()
    {
        AudioManager.Instance?.PlaySFX(_sniffClip);

        var source = SoundDetector.Instance?.GetNearestSource(transform.position, _hearingRange);

        if (source != null)
        {
            yield return StartCoroutine(MoveToward(source.Position, _investigateSpeed, 0.8f));

            if (source.IsPlayer && _chargeTimer <= 0f)
            {
                yield return new WaitForSeconds(_chargeWindup);
                if (!_defeated && _player != null)
                    _entity.TriggerCharge(_player);
                _chargeTimer = _chargeCooldown;
            }
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        _chargeTimer = Mathf.Max(0f, _chargeTimer - 0.5f);
    }

    // ── Phase 2: Enrage ───────────────────────────────────────────────────────

    private IEnumerator Phase2Tick()
    {
        if (_player != null)
        {
            AudioManager.Instance?.PlaySFX(_rockFallClip);
            _entity.TriggerCharge(_player);
            yield return new WaitForSeconds(_phase2ChargeCooldown);
        }
        else
        {
            yield return new WaitForSeconds(0.3f);
        }
    }

    // ── Phase 3: Confused ─────────────────────────────────────────────────────

    private IEnumerator Phase3Tick()
    {
        if (Random.value < _peaceChance)
        {
            if (_rb != null) _rb.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(1.5f);
        }
        else
        {
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            if (_rb != null) _rb.linearVelocity = randomDir * _confusedSpeed;
            yield return new WaitForSeconds(0.8f);
            if (_rb != null) _rb.linearVelocity = Vector2.zero;
        }

        yield return new WaitForSeconds(0.3f);
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    private IEnumerator MoveToward(Vector2 target, float speed, float stopDist)
    {
        float timeout = 3f;
        while (!_defeated && timeout > 0f &&
               Vector2.Distance(transform.position, target) > stopDist)
        {
            Vector2 dir = (target - (Vector2)transform.position).normalized;
            if (_rb != null) _rb.linearVelocity = dir * speed;
            timeout -= Time.deltaTime;
            yield return null;
        }
        if (_rb != null) _rb.linearVelocity = Vector2.zero;
    }
}
