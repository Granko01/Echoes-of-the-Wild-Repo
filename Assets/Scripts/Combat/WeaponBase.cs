using UnityEngine;

// Abstract base for all weapons. Attach concrete subclass to a child GameObject of the Player.
// WeaponHolder finds this via GetComponentInChildren and routes inputs here.
public abstract class WeaponBase : MonoBehaviour
{
    [SerializeField] protected WeaponData _data;
    [SerializeField] protected float      _attackCooldown  = 0.4f;
    [SerializeField] protected float      _skillCooldown   = 3f;
    [SerializeField] protected float      _attackDamage    = 10f;  // HP damage to BossHealth
    [SerializeField] protected LayerMask  _hitLayer;

    protected float _attackTimer;
    protected float _skillTimer;

    public WeaponData Data  => _data;
    public int        Level => _data != null ? _data.currentLevel : 0;

    // Called by WeaponHolder when this weapon becomes the active one
    public virtual void OnEquip()   { gameObject.SetActive(true);  }
    public virtual void OnUnequip() { gameObject.SetActive(false); }

    protected virtual void Update()
    {
        if (_attackTimer > 0) _attackTimer -= Time.deltaTime;
        if (_skillTimer  > 0) _skillTimer  -= Time.deltaTime;
    }

    // Direction = player facing direction (±1 on x, or full vector for aimed weapons)
    public void TryAttack(Vector2 direction)
    {
        if (_attackTimer > 0) return;
        _attackTimer = _attackCooldown;
        Attack(direction);
        GameEvents.RaiseWeaponAttack(this, direction);
    }

    public void TryWeaponSkill(Vector2 direction)
    {
        if (_skillTimer > 0 || Level < 1) return;
        _skillTimer = _skillCooldown;
        WeaponSkill(direction);
        GameEvents.RaiseWeaponSkillUsed(this);
    }

    protected abstract void Attack(Vector2 direction);
    protected abstract void WeaponSkill(Vector2 direction);

    // Shared: deal damage to any BossHealth in radius
    protected void DealAoeDamage(Vector2 center, float radius)
    {
        AttackVFX.SpawnPulse(center, radius);
        CameraShake.Instance?.Shake(0.12f, 0.10f);
        var hits = Physics2D.OverlapCircleAll(center, radius, _hitLayer);
        foreach (var col in hits)
        {
            if (col.TryGetComponent<BossHealth>(out var bh)) bh.TakeDamage(_attackDamage);
            if (col.TryGetComponent<EntityController>(out var ec)) ec.OnPulseHit();
        }
    }

    // Shared: deal damage to a single target hit by a projectile/hitbox
    protected void DealDirectDamage(Collider2D col)
    {
        AttackVFX.SpawnHit(col.transform.position);
        CameraShake.Instance?.Shake(0.10f, 0.08f);
        if (col.TryGetComponent<BossHealth>(out var bh)) bh.TakeDamage(_attackDamage);
        if (col.TryGetComponent<EntityController>(out var ec)) ec.OnPulseHit();
    }

    // Shared: light flash at attack origin (no target required — call from melee Attack())
    protected void SpawnAttackFlash(Vector2 pos)
        => AttackVFX.SpawnHit(pos);
}
