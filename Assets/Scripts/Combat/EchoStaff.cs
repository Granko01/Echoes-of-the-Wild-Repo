using System.Collections;
using UnityEngine;

// Chapter 1 weapon — Echo Staff
// Base  : Echo Blast (projectile)
// Lv2   : Sound Pulse (AoE wave that reveals hidden things + hits enemies)
// Lv3   : Crystal Bounce (ricochet projectile)
// MAX   : Echo Clone (fake sound decoy at target position)
public class EchoStaff : WeaponBase
{
    [Header("Echo Blast — Base")]
    [SerializeField] private GameObject _blastPrefab;
    [SerializeField] private float      _blastSpeed  = 10f;

    [Header("Sound Pulse — Lv2")]
    [SerializeField] private float _pulseRadius    = 4f;
    [SerializeField] private float _pulseDuration  = 0.4f;

    [Header("Crystal Bounce — Lv3")]
    [SerializeField] private GameObject _bouncePrefab;
    [SerializeField] private int        _bounceCount = 3;

    [Header("Echo Clone — MAX")]
    [SerializeField] private GameObject _clonePrefab;
    [SerializeField] private float      _cloneRange    = 6f;
    [SerializeField] private float      _cloneLifetime = 5f;

    [Header("Audio")]
    [SerializeField] private AudioClip _blastClip;
    [SerializeField] private AudioClip _pulseClip;
    [SerializeField] private AudioClip _cloneClip;

    private PlayerController _pc;

    private void Start() => _pc = GetComponentInParent<PlayerController>();

    protected override void Attack(Vector2 direction)
    {
        if (Level >= 2)
            FireCrystalBounce(direction);
        else
            FireEchoBlast(direction);
    }

    protected override void WeaponSkill(Vector2 direction)
    {
        switch (Level)
        {
            case 1: StartCoroutine(SoundPulseRoutine()); break;
            case 2: FireCrystalBounce(direction);        break;
            case 3: DropEchoClone(direction);            break;
        }
    }

    // ── Echo Blast ────────────────────────────────────────────────────────────

    private void FireEchoBlast(Vector2 direction)
    {
        AudioManager.Instance?.PlaySFX(_blastClip);

        if (_blastPrefab == null)
        {
            DealAoeDamage((Vector2)transform.position + direction * 0.7f, 0.4f);
            return;
        }

        var go  = Instantiate(_blastPrefab, transform.position, Quaternion.identity);
        var rb  = go.GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = direction.normalized * _blastSpeed;

        var proj = go.AddComponent<EchoBlastProjectile>();
        proj.Init(_hitLayer, _attackDamage);
        Destroy(go, 3f);
    }

    // ── Sound Pulse ───────────────────────────────────────────────────────────

    private IEnumerator SoundPulseRoutine()
    {
        AudioManager.Instance?.PlaySFX(_pulseClip);
        PulseWaveEffect.Spawn(transform.position, _pulseRadius, System.Array.Empty<Vector3>());

        float elapsed = 0f;
        while (elapsed < _pulseDuration)
        {
            DealAoeDamage(transform.position, _pulseRadius);
            SoundDetector.Instance?.RegisterSource(
                new SoundSource(transform.position, 3f, isPlayer: false));
            elapsed += 0.1f;
            yield return new WaitForSeconds(0.1f);
        }
    }

    // ── Crystal Bounce ────────────────────────────────────────────────────────

    private void FireCrystalBounce(Vector2 direction)
    {
        AudioManager.Instance?.PlaySFX(_blastClip);

        if (_bouncePrefab == null) { FireEchoBlast(direction); return; }

        var go   = Instantiate(_bouncePrefab, transform.position, Quaternion.identity);
        var proj = go.GetComponent<CrystalBounceProjectile>();
        if (proj == null) proj = go.AddComponent<CrystalBounceProjectile>();
        proj.Init(direction.normalized * _blastSpeed, _bounceCount, _hitLayer, _attackDamage);
    }

    // ── Echo Clone ────────────────────────────────────────────────────────────

    private void DropEchoClone(Vector2 facingDirection)
    {
        AudioManager.Instance?.PlaySFX(_cloneClip);

        Vector2 dir = facingDirection.magnitude > 0.01f ? facingDirection.normalized : Vector2.right;
        Vector2 pos = (Vector2)transform.position + dir * _cloneRange;

        if (_clonePrefab == null)
        {
            SoundDetector.Instance?.RegisterSource(new SoundSource(pos, _cloneLifetime, isPlayer: false));
            return;
        }

        var go    = Instantiate(_clonePrefab, (Vector3)pos, Quaternion.identity);
        var clone = go.GetComponent<EchoClone>();
        if (clone == null) clone = go.AddComponent<EchoClone>();
        clone.Init(_cloneLifetime);
    }
}
