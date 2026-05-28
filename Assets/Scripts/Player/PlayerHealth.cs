using System.Collections;
using UnityEngine;

// Attach to the Player GameObject. Tracks hearts.
// EntityController and BossHealth both call TakeDamage on the player.
[RequireComponent(typeof(PlayerController))]
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int   _maxHearts         = 3;
    [SerializeField] private float _invincibleSeconds  = 0.6f;

    public int  MaxHearts     => _maxHearts;
    public int  CurrentHearts { get; private set; }
    public bool IsInvincible  { get; private set; }

    private void Awake() => CurrentHearts = _maxHearts;

    public void TakeDamage(int amount = 1)
    {
        if (IsInvincible || CurrentHearts <= 0) return;

        CurrentHearts = Mathf.Max(0, CurrentHearts - amount);
        GameEvents.RaisePlayerDamaged(CurrentHearts, _maxHearts);

        if (CurrentHearts == 0)
        {
            GameEvents.RaisePlayerDied();
            return;
        }

        StartCoroutine(InvincibilityFrames());
    }

    public void Heal(int amount = 1)
    {
        CurrentHearts = Mathf.Min(_maxHearts, CurrentHearts + amount);
        GameEvents.RaisePlayerDamaged(CurrentHearts, _maxHearts);
    }

    // Called by PlayerController dodge routine to manually control iframes
    public void SetInvincible(bool value)
    {
        IsInvincible = value;
    }

    private IEnumerator InvincibilityFrames()
    {
        IsInvincible = true;
        yield return new WaitForSeconds(_invincibleSeconds);
        IsInvincible = false;
    }
}
