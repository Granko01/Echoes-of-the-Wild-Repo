using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Chapter 1 — Cave Realm — Boss 2: Distressed Mother Beast
// Phase 1: Claw combo, slam, leap, corruption wave.
// Phase 2 (≤65%): Memory Flowers spawn around arena; Echo pulse triggers recognition pauses.
// Phase 3 (≤25%): Attacks interrupted mid-animation; reaches toward player; brief recognition.
public class DistressedMotherBeast : ChapterBossController
{
    [Header("Phase 1 — Aggression")]
    [SerializeField] private float _clawRadius      = 1.5f;
    [SerializeField] private float _clawDamage      = 1f;
    [SerializeField] private float _slamRadius      = 3f;
    [SerializeField] private float _leapDistance    = 5f;
    [SerializeField] private float _corruptWaveRadius = 5f;
    [SerializeField] private float _attackInterval  = 2f;

    [Header("Phase 2 — Memory Flowers")]
    [SerializeField] private GameObject _memoryFlowerPrefab;
    [SerializeField] private int        _flowerCount      = 5;
    [SerializeField] private float      _flowerSpawnRadius = 6f;
    [SerializeField] private float      _recognitionPauseDuration = 2f;

    [Header("Phase 3 — Recognition")]
    [SerializeField] private float _phase3AttackInterruptChance = 0.4f;
    [SerializeField] private float _reachDuration = 1.5f;

    [Header("Audio")]
    [SerializeField] private AudioClip _battleStartClip;
    [SerializeField] private AudioClip _recognitionClip;
    [SerializeField] private AudioClip _defeatClip;

    private int   _currentPhase = 1;
    private bool  _pausedForRecognition;
    private Rigidbody2D _rb;

    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody2D>();
        GameEvents.OnPulseHit += HandlePulseHit;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameEvents.OnPulseHit -= HandlePulseHit;
    }

    protected override void OnActivated()
    {
        AudioManager.Instance?.PlaySFX(_battleStartClip);
        StartCoroutine(BossLoop());
    }

    protected override void OnPhaseChanged(int phase)
    {
        _currentPhase = phase;
        if (phase == 2) SpawnMemoryFlowers();
    }

    protected override void OnDefeated()
    {
        AudioManager.Instance?.PlaySFX(_defeatClip);
        if (_rb != null) _rb.linearVelocity = Vector2.zero;
        // Kneels and touches Punch's head — visual handled by animator trigger
    }

    // ── Main Loop ─────────────────────────────────────────────────────────────

    private IEnumerator BossLoop()
    {
        while (!_defeated)
        {
            if (_pausedForRecognition) { yield return null; continue; }

            switch (_currentPhase)
            {
                case 1: yield return StartCoroutine(Phase1Attack()); break;
                case 2: yield return StartCoroutine(Phase2Attack()); break;
                case 3: yield return StartCoroutine(Phase3Attack()); break;
            }

            yield return new WaitForSeconds(_attackInterval);
        }
    }

    // ── Phase 1 ───────────────────────────────────────────────────────────────

    private IEnumerator Phase1Attack()
    {
        int attack = Random.Range(0, 4);
        switch (attack)
        {
            case 0: yield return StartCoroutine(ClawCombo());       break;
            case 1: yield return StartCoroutine(Slam());            break;
            case 2: yield return StartCoroutine(Leap());            break;
            case 3: yield return StartCoroutine(CorruptionWave());  break;
        }
    }

    private IEnumerator ClawCombo()
    {
        for (int i = 0; i < 3; i++)
        {
            DamagePlayer(_clawRadius, _clawDamage);
            yield return new WaitForSeconds(0.3f);
        }
    }

    private IEnumerator Slam()
    {
        if (_player != null)
            FaceTarget(_player.position);
        DamagePlayer(_slamRadius, _clawDamage);
        yield return new WaitForSeconds(0.5f);
    }

    private IEnumerator Leap()
    {
        if (_player == null) yield break;
        Vector2 dir = (_player.position - transform.position).normalized;
        if (_rb != null)
            _rb.linearVelocity = new Vector2(dir.x * _leapDistance, 8f);
        yield return new WaitForSeconds(0.6f);
        DamagePlayer(_clawRadius + 0.5f, _clawDamage);
    }

    private IEnumerator CorruptionWave()
    {
        DamagePlayer(_corruptWaveRadius, _clawDamage);
        yield return new WaitForSeconds(0.4f);
    }

    // ── Phase 2 ───────────────────────────────────────────────────────────────

    private IEnumerator Phase2Attack()
    {
        // Same attacks but flowers can be triggered by player touching them
        yield return StartCoroutine(Phase1Attack());
    }

    private void SpawnMemoryFlowers()
    {
        if (_memoryFlowerPrefab == null) return;
        for (int i = 0; i < _flowerCount; i++)
        {
            Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle.normalized * _flowerSpawnRadius;
            Instantiate(_memoryFlowerPrefab, pos, Quaternion.identity);
        }
    }

    // Pulse hit during Phase 2 → recognition pause
    private void HandlePulseHit(EntityController ec)
    {
        if (ec.gameObject != gameObject || _currentPhase < 2) return;
        if (!_pausedForRecognition) StartCoroutine(RecognitionPause());
    }

    private IEnumerator RecognitionPause()
    {
        _pausedForRecognition = true;
        AudioManager.Instance?.PlaySFX(_recognitionClip);
        if (_rb != null) _rb.linearVelocity = Vector2.zero;
        yield return new WaitForSeconds(_recognitionPauseDuration);
        _pausedForRecognition = false;
    }

    // ── Phase 3 ───────────────────────────────────────────────────────────────

    private IEnumerator Phase3Attack()
    {
        // Chance to interrupt attack mid-swing and reach toward player instead
        if (Random.value < _phase3AttackInterruptChance)
        {
            yield return StartCoroutine(ReachTowardPlayer());
        }
        else
        {
            // Weakened version of Phase 1
            yield return StartCoroutine(ClawCombo());
        }
    }

    private IEnumerator ReachTowardPlayer()
    {
        if (_player == null) yield break;
        AudioManager.Instance?.PlaySFX(_recognitionClip);
        if (_rb != null) _rb.linearVelocity = Vector2.zero;

        // Animate reaching — face player and hold
        FaceTarget(_player.position);
        yield return new WaitForSeconds(_reachDuration);
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    private void DamagePlayer(float radius, float damage)
    {
        if (_player == null) return;
        if (Vector2.Distance(transform.position, _player.position) <= radius)
            _player.GetComponent<PlayerHealth>()?.TakeDamage((int)damage);
    }

    private void FaceTarget(Vector3 target)
    {
        float dir = target.x > transform.position.x ? 1f : -1f;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * dir,
                                           transform.localScale.y, transform.localScale.z);
    }
}
