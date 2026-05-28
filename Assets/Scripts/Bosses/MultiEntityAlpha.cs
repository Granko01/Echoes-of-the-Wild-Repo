using System.Collections;
using UnityEngine;

// Chapter 2 — Forest Realm — Boss 1: Multi-Entity Alpha
// A giant creature formed of roots: Wolf head + Tiger head + Bird wings + Gorilla arms.
// Cycles through sub-phases. Nunchaku air combos interrupt Bird; fast combos stagger Wolf.
public class MultiEntityAlpha : ChapterBossController
{
    public enum SubPhase { Wolf, Tiger, Bird, Gorilla }

    [Header("Sub-Phase Cycling")]
    [SerializeField] private float _subPhaseDuration   = 12f;
    [SerializeField] private float _subPhaseTransition = 1f;

    [Header("Wolf — Dash Attacks")]
    [SerializeField] private float _wolfDashSpeed      = 8f;
    [SerializeField] private float _wolfDashCooldown   = 2f;

    [Header("Tiger — Heavy Aggression")]
    [SerializeField] private float _tigerPounceRadius  = 2f;
    [SerializeField] private float _tigerSwipeRadius   = 3f;

    [Header("Bird — Aerial Attacks")]
    [SerializeField] private float _birdAltitude       = 5f;
    [SerializeField] private float _birdDiveSpeed      = 12f;
    [SerializeField] private int   _aerialComboToInterrupt = 10;

    [Header("Gorilla — Smash")]
    [SerializeField] private float _gorillaSlamRadius  = 4f;
    [SerializeField] private int   _wolfComboToStagger = 5;

    [Header("Audio")]
    [SerializeField] private AudioClip _roarClip;
    [SerializeField] private AudioClip _transitionClip;
    [SerializeField] private AudioClip _defeatClip;

    private SubPhase _subPhase = SubPhase.Wolf;
    private int      _currentPhase = 1;
    private Rigidbody2D _rb;

    protected override void Awake()
    {
        base.Awake();
        _rb = GetComponent<Rigidbody2D>();
        // Listen for combo changes — interrupt Bird at aerial combo threshold
        GameEvents.OnComboChanged += HandleComboChanged;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameEvents.OnComboChanged -= HandleComboChanged;
    }

    protected override void OnActivated()
    {
        AudioManager.Instance?.PlaySFX(_roarClip);
        StartCoroutine(SubPhaseLoop());
        StartCoroutine(AttackLoop());
    }

    protected override void OnPhaseChanged(int phase)
    {
        _currentPhase = phase;
    }

    protected override void OnDefeated()
    {
        AudioManager.Instance?.PlaySFX(_defeatClip);
        if (_rb != null) _rb.linearVelocity = Vector2.zero;
        // Splits apart — four animals awaken. Forest regrows. (Visual handled by scene animator.)
    }

    // ── Sub-Phase Cycling ─────────────────────────────────────────────────────

    private IEnumerator SubPhaseLoop()
    {
        SubPhase[] cycle = { SubPhase.Wolf, SubPhase.Tiger, SubPhase.Bird, SubPhase.Gorilla };
        int idx = 0;
        while (!_defeated)
        {
            _subPhase = cycle[idx % cycle.Length];
            AudioManager.Instance?.PlaySFX(_transitionClip);
            yield return new WaitForSeconds(_subPhaseDuration);
            idx++;
        }
    }

    // ── Attack Loop ───────────────────────────────────────────────────────────

    private IEnumerator AttackLoop()
    {
        while (!_defeated)
        {
            switch (_subPhase)
            {
                case SubPhase.Wolf:    yield return StartCoroutine(WolfAttack());    break;
                case SubPhase.Tiger:   yield return StartCoroutine(TigerAttack());   break;
                case SubPhase.Bird:    yield return StartCoroutine(BirdAttack());    break;
                case SubPhase.Gorilla: yield return StartCoroutine(GorillaAttack()); break;
            }
            float cooldown = _currentPhase == 1 ? 2f : (_currentPhase == 2 ? 1.4f : 1f);
            yield return new WaitForSeconds(cooldown);
        }
    }

    private IEnumerator WolfAttack()
    {
        // Dash at player
        if (_player == null) yield break;
        Vector2 dir = (_player.position - transform.position).normalized;
        if (_rb != null) _rb.linearVelocity = dir * _wolfDashSpeed;
        yield return new WaitForSeconds(0.4f);
        if (_rb != null) _rb.linearVelocity = Vector2.zero;

        DamagePlayer(1.2f, 1f);
    }

    private IEnumerator TigerAttack()
    {
        // Pounce + swipe combo
        if (_player != null)
        {
            Vector2 dir = (_player.position - transform.position).normalized;
            if (_rb != null) _rb.linearVelocity = new Vector2(dir.x * 6f, 5f);
        }
        yield return new WaitForSeconds(0.5f);
        DamagePlayer(_tigerPounceRadius, 1f);
        yield return new WaitForSeconds(0.2f);
        DamagePlayer(_tigerSwipeRadius, 1f);
        if (_rb != null) _rb.linearVelocity = Vector2.zero;
    }

    private IEnumerator BirdAttack()
    {
        // Rise then dive
        if (_rb != null) _rb.linearVelocity = new Vector2(0f, _birdAltitude);
        yield return new WaitForSeconds(0.6f);
        if (_rb != null) _rb.gravityScale = 0f;

        // Hover briefly
        yield return new WaitForSeconds(0.8f);

        // Dive toward player
        if (_player != null && _rb != null)
        {
            Vector2 dir = (_player.position - transform.position).normalized;
            _rb.gravityScale    = 1f;
            _rb.linearVelocity  = dir * _birdDiveSpeed;
        }
        yield return new WaitForSeconds(0.4f);
        DamagePlayer(1.5f, 1f);
        if (_rb != null) _rb.linearVelocity = Vector2.zero;
    }

    private IEnumerator GorillaAttack()
    {
        // Ground slam
        if (_rb != null) _rb.linearVelocity = new Vector2(0f, -15f);
        yield return new WaitForSeconds(0.3f);
        DamagePlayer(_gorillaSlamRadius, 2f);
        yield return new WaitForSeconds(0.3f);
    }

    // ── Interrupt / Stagger ───────────────────────────────────────────────────

    private void HandleComboChanged(int combo)
    {
        if (_defeated) return;

        // Bird interrupted by aerial combos
        if (_subPhase == SubPhase.Bird && combo >= _aerialComboToInterrupt)
        {
            StopCoroutine(nameof(BirdAttack));
            if (_rb != null) _rb.linearVelocity = Vector2.zero;
        }

        // Wolf staggered by fast combos
        if (_subPhase == SubPhase.Wolf && combo >= _wolfComboToStagger)
        {
            if (_rb != null) _rb.linearVelocity = Vector2.zero;
        }
    }

    // ── Utilities ─────────────────────────────────────────────────────────────

    private void DamagePlayer(float radius, float damage)
    {
        if (_player == null) return;
        if (Vector2.Distance(transform.position, _player.position) <= radius)
            _player.GetComponent<PlayerHealth>()?.TakeDamage((int)damage);
    }
}
