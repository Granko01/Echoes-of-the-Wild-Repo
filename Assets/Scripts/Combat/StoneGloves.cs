using System.Collections;
using UnityEngine;

// Chapter 3 weapon — Stone Gloves
// Base  : Heavy punch (short-range, high damage)
// Lv2   : Stone Slam (ground shockwave)
// Lv3   : Armor Break (shatters enemy defense — removes IceHollowGorilla crystal shield)
// MAX   : Avalanche Impact (massive punch creating ice destruction)
public class StoneGloves : WeaponBase
{
    [Header("Heavy Punch — Base")]
    [SerializeField] private float _punchRange  = 1.2f;

    [Header("Stone Slam — Lv2")]
    [SerializeField] private float _slamRadius     = 3f;
    [SerializeField] private float _slamDuration   = 0.3f;

    [Header("Avalanche Impact — MAX")]
    [SerializeField] private float _avalancheRadius   = 5f;
    [SerializeField] private float _avalancheDuration = 0.8f;
    [SerializeField] private float _avalancheDamage   = 40f;

    [Header("Audio")]
    [SerializeField] private AudioClip _punchClip;
    [SerializeField] private AudioClip _slamClip;
    [SerializeField] private AudioClip _avalancheClip;

    private PlayerController _pc;
    private void Start() => _pc = GetComponentInParent<PlayerController>();

    protected override void Attack(Vector2 direction)
    {
        AudioManager.Instance?.PlaySFX(_punchClip);
        Vector2 center = (Vector2)transform.position + direction.normalized * _punchRange;
        DealAoeDamage(center, 0.8f);
        // Drain cold on hit
        ColdMeter.Instance?.Drain(0.3f);
    }

    protected override void WeaponSkill(Vector2 direction)
    {
        switch (Level)
        {
            case 1: StartCoroutine(StoneSlamRoutine()); break;
            case 2: StartCoroutine(ArmorBreakRoutine(direction)); break;
            case 3: StartCoroutine(AvalancheRoutine()); break;
        }
    }

    private IEnumerator StoneSlamRoutine()
    {
        AudioManager.Instance?.PlaySFX(_slamClip);
        // Create a ground shockwave in both directions
        float elapsed = 0f;
        while (elapsed < _slamDuration)
        {
            DealAoeDamage(transform.position, _slamRadius);
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
        ColdMeter.Instance?.Drain(0.5f);
    }

    private IEnumerator ArmorBreakRoutine(Vector2 direction)
    {
        AudioManager.Instance?.PlaySFX(_punchClip);
        // Same as slam but also tries to break crystal shields on nearby bosses
        var hits = Physics2D.OverlapCircleAll(transform.position, _slamRadius, _hitLayer);
        foreach (var col in hits)
        {
            if (col.TryGetComponent<BossHealth>(out var bh))
            {
                bh.TakeDamage(_attackDamage * 2f);
                bh.NotifyArmorBreak();
            }
        }
        yield return null;
    }

    private IEnumerator AvalancheRoutine()
    {
        AudioManager.Instance?.PlaySFX(_avalancheClip);
        float elapsed = 0f;
        while (elapsed < _avalancheDuration)
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, _avalancheRadius, _hitLayer);
            foreach (var col in hits)
            {
                if (col.TryGetComponent<BossHealth>(out var bh))
                    bh.TakeDamage(_avalancheDamage * Time.deltaTime);
                if (col.TryGetComponent<EntityController>(out var ec))
                    ec.OnPulseHit();
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        ColdMeter.Instance?.Drain(1f);
    }
}
