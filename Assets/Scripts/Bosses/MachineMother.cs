using System.Collections;
using UnityEngine;

// Chapter 2 — Forest Realm — Boss 2: Machine Mother MK-01 (MAMA)
// Phase 1: Robot punches + shield drones + laser scans.
// Phase 2: Arena transforms, mechanical arms attack from sides.
// Phase 3: Awareness moments — voice lines, player freezes emotionally.
public class MachineMother : ChapterBossController
{
    [Header("Phase 1 — Robot Combat")]
    [SerializeField] private float _punchRadius      = 1.8f;
    [SerializeField] private float _punchCooldown    = 1.5f;
    [SerializeField] private float _laserWidth       = 0.5f;
    [SerializeField] private float _laserRange       = 10f;
    [SerializeField] private float _laserWarning     = 1f;

    [Header("Phase 2 — Mechanical Arms")]
    [SerializeField] private float _armRadius        = 2f;
    [SerializeField] private float _armAttackInterval = 2f;

    [Header("Phase 3 — Awareness")]
    [SerializeField] private float _awarenessChance  = 0.3f;
    [SerializeField] private float _awarenessFreeze  = 2f;
    [SerializeField] private AudioClip _awarenessClip;   // "Punch..." / "Run..."

    [Header("Audio")]
    [SerializeField] private AudioClip _bootupClip;
    [SerializeField] private AudioClip _glitchClip;
    [SerializeField] private AudioClip _laserClip;
    [SerializeField] private AudioClip _punchClip;
    [SerializeField] private AudioClip _shutdownClip;

    private int  _currentPhase = 1;
    private bool _armsActive;
    private Rigidbody2D _rb;

    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody2D>();
    }

    protected override void OnActivated()
    {
        // MAMA glitches — voice breaks — "Protect child... Protect..."
        AudioManager.Instance?.PlaySFX(_glitchClip);
        StartCoroutine(BossLoop());
    }

    protected override void OnPhaseChanged(int phase)
    {
        _currentPhase = phase;
        if (phase == 2) StartCoroutine(Phase2ArmLoop());
    }

    protected override void OnDefeated()
    {
        AudioManager.Instance?.PlaySFX(_shutdownClip);
        if (_rb != null) _rb.linearVelocity = Vector2.zero;
        // "Did I keep you safe?" — Battery: 0% — Eyes fade. (Voice line clip assigned in inspector.)
    }

    // ── Main Loop ─────────────────────────────────────────────────────────────

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
        }
    }

    // ── Phase 1 ───────────────────────────────────────────────────────────────

    private IEnumerator Phase1Tick()
    {
        int attack = Random.Range(0, 3);
        switch (attack)
        {
            case 0: yield return StartCoroutine(RobotPunch());   break;
            case 1: yield return StartCoroutine(LaserScan());    break;
            case 2: yield return new WaitForSeconds(1f);         break; // shield drone (no prefab: skip)
        }
    }

    private IEnumerator RobotPunch()
    {
        if (_player == null) yield break;
        AudioManager.Instance?.PlaySFX(_punchClip);

        // Telegraph: move toward player
        float elapsed = 0f;
        while (elapsed < _punchCooldown * 0.5f && _player != null)
        {
            Vector2 dir = (_player.position - transform.position).normalized;
            if (_rb != null) _rb.linearVelocity = dir * 4f;
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (_rb != null) _rb.linearVelocity = Vector2.zero;

        DamagePlayer(_punchRadius, 1f);
        yield return new WaitForSeconds(_punchCooldown * 0.5f);
    }

    private IEnumerator LaserScan()
    {
        if (_player == null) yield break;
        AudioManager.Instance?.PlaySFX(_laserClip);

        // Warning telegraph
        yield return new WaitForSeconds(_laserWarning);

        // Fire laser in player direction — detect player overlap
        Vector2 dir      = (_player.position - transform.position).normalized;
        var hits = Physics2D.CircleCastAll(transform.position, _laserWidth, dir, _laserRange);
        foreach (var hit in hits)
        {
            if (hit.collider.TryGetComponent<PlayerHealth>(out var ph))
                ph.TakeDamage(1);
        }

        yield return new WaitForSeconds(0.5f);
    }

    // ── Phase 2 ───────────────────────────────────────────────────────────────

    private IEnumerator Phase2Tick()
    {
        yield return StartCoroutine(Phase1Tick());
    }

    private IEnumerator Phase2ArmLoop()
    {
        _armsActive = true;
        while (!_defeated && _currentPhase >= 2)
        {
            yield return new WaitForSeconds(_armAttackInterval);
            if (_player == null) continue;

            // Arms from sides of arena — position relative to player
            Vector2 leftArm  = (Vector2)_player.position + Vector2.left  * 3f;
            Vector2 rightArm = (Vector2)_player.position + Vector2.right * 3f;

            DamageAtPoint(leftArm,  _armRadius);
            DamageAtPoint(rightArm, _armRadius);
        }
    }

    // ── Phase 3 ───────────────────────────────────────────────────────────────

    private IEnumerator Phase3Tick()
    {
        if (Random.value < _awarenessChance)
        {
            // MAMA becomes aware — says "Punch..." or "Run..."
            AudioManager.Instance?.PlaySFX(_awarenessClip);
            if (_rb != null) _rb.linearVelocity = Vector2.zero;
            yield return new WaitForSeconds(_awarenessFreeze);
        }
        else
        {
            yield return StartCoroutine(Phase1Tick());
        }
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    private void DamagePlayer(float radius, float damage)
    {
        if (_player == null) return;
        if (Vector2.Distance(transform.position, _player.position) <= radius)
            _player.GetComponent<PlayerHealth>()?.TakeDamage((int)damage);
    }

    private void DamageAtPoint(Vector2 point, float radius)
    {
        if (_player == null) return;
        if (Vector2.Distance(point, _player.position) <= radius)
            _player.GetComponent<PlayerHealth>()?.TakeDamage(1);
    }
}
