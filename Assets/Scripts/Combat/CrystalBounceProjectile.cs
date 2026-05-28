using UnityEngine;

// EchoStaff Lv3 — ricochets off walls N times.
// Each bounce creates a SoundSource for CaveMaw.
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class CrystalBounceProjectile : MonoBehaviour
{
    private int       _bouncesLeft;
    private LayerMask _hitLayer;
    private float     _damage;
    private Vector2   _velocity;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
        if (_rb != null)
        {
            _rb.gravityScale    = 0f;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    public void Init(Vector2 velocity, int bounces, LayerMask hitLayer, float damage)
    {
        _velocity    = velocity;
        _bouncesLeft = bounces;
        _hitLayer    = hitLayer;
        _damage      = damage;
        if (_rb != null) _rb.linearVelocity = _velocity;
        Destroy(gameObject, 5f);
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        // Hit an enemy/boss
        if (col.collider.TryGetComponent<BossHealth>(out var bh)) bh.TakeDamage(_damage);
        if (col.collider.TryGetComponent<EntityController>(out var ec)) ec.OnPulseHit();

        SoundDetector.Instance?.RegisterSource(
            new SoundSource(transform.position, 2.5f, isPlayer: false));

        if (_bouncesLeft <= 0) { Destroy(gameObject); return; }

        // Reflect velocity off collision normal
        _bouncesLeft--;
        _velocity = Vector2.Reflect(_velocity, col.contacts[0].normal);
        if (_rb != null) _rb.linearVelocity = _velocity;
    }
}
