using System.Collections;
using UnityEngine;

// Chapter 4 weapon B — Soul Hook
// Base  : Pull (pulls player toward target or pulls small enemies toward player)
// Lv2   : Enemy grab (lock an enemy in place briefly)
// Lv3   : Swing chains (grapple to a surface and swing)
// MAX   : Multi-hook traversal (chain multiple grapple points)
public class SoulHook : WeaponBase
{
    [Header("Pull — Base")]
    [SerializeField] private float _hookRange     = 8f;
    [SerializeField] private float _pullSpeed     = 15f;
    [SerializeField] private LayerMask _grappleLayer;

    [Header("Enemy Grab — Lv2")]
    [SerializeField] private float _grabDuration = 2f;

    [Header("Swing — Lv3")]
    [SerializeField] private float _swingForce = 18f;

    [Header("Audio")]
    [SerializeField] private AudioClip _hookClip;
    [SerializeField] private AudioClip _pullClip;

    private Rigidbody2D _playerRb;
    private bool        _isSwinging;
    private Vector2     _swingAnchor;

    private void Start() => _playerRb = GetComponentInParent<Rigidbody2D>();

    protected override void Attack(Vector2 direction)
    {
        AudioManager.Instance?.PlaySFX(_hookClip);

        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, _hookRange, _grappleLayer | _hitLayer);
        if (!hit) return;

        if (hit.collider.TryGetComponent<BossHealth>(out var bh))
        {
            bh.TakeDamage(_attackDamage);
            if (Level >= 1) StartCoroutine(GrabRoutine(hit.collider));
        }
        else if (Level >= 2)
        {
            // Swing toward surface
            _swingAnchor = hit.point;
            StartCoroutine(SwingRoutine());
        }
        else
        {
            // Pull player toward grapple point
            StartCoroutine(PullRoutine(hit.point));
        }
    }

    protected override void WeaponSkill(Vector2 direction)
    {
        if (Level >= 3) StartCoroutine(MultiHookRoutine(direction));
        else             Attack(direction);
    }

    private IEnumerator PullRoutine(Vector2 target)
    {
        if (_playerRb == null) yield break;
        AudioManager.Instance?.PlaySFX(_pullClip);
        float elapsed = 0f;
        while (elapsed < 0.5f && Vector2.Distance(transform.position, target) > 0.5f)
        {
            Vector2 dir = ((Vector2)target - (Vector2)transform.position).normalized;
            _playerRb.linearVelocity = dir * _pullSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator GrabRoutine(Collider2D target)
    {
        var rb = target.GetComponent<Rigidbody2D>();
        float saved = rb != null ? rb.gravityScale : 0f;
        if (rb != null) { rb.linearVelocity = Vector2.zero; rb.gravityScale = 0f; }
        yield return new WaitForSeconds(_grabDuration);
        if (rb != null) rb.gravityScale = saved;
    }

    private IEnumerator SwingRoutine()
    {
        if (_playerRb == null) yield break;
        _isSwinging = true;
        float elapsed = 0f;
        while (elapsed < 0.6f && _isSwinging)
        {
            Vector2 toAnchor = _swingAnchor - (Vector2)transform.position;
            Vector2 perp     = new Vector2(-toAnchor.y, toAnchor.x).normalized;
            _playerRb.AddForce(perp * _swingForce);
            elapsed += Time.deltaTime;
            yield return null;
        }
        _isSwinging = false;
    }

    private IEnumerator MultiHookRoutine(Vector2 direction)
    {
        // Chain 3 grapples in sequence
        for (int i = 0; i < 3; i++)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, _hookRange, _grappleLayer);
            if (!hit) break;
            yield return StartCoroutine(PullRoutine(hit.point));
            yield return new WaitForSeconds(0.1f);
        }
    }
}
