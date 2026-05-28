using UnityEngine;

// Simple forward-flying projectile for EchoStaff base attack.
// Deals damage on first hit then destroys itself.
[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class EchoBlastProjectile : MonoBehaviour
{
    private LayerMask _hitLayer;
    private float     _damage;

    public void Init(LayerMask hitLayer, float damage)
    {
        _hitLayer = hitLayer;
        _damage   = damage;
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Skip if not in hit layer and not environment
        if (other.gameObject == gameObject) return;

        if (other.TryGetComponent<BossHealth>(out var bh)) bh.TakeDamage(_damage);
        if (other.TryGetComponent<EntityController>(out var ec)) ec.OnPulseHit();

        // Register impact as sound for CaveMaw
        SoundDetector.Instance?.RegisterSource(
            new SoundSource(transform.position, 2f, isPlayer: false));

        Destroy(gameObject);
    }
}
