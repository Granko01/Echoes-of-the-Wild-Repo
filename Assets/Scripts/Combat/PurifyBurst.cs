using System.Collections;
using UnityEngine;

// Attach to the Player. Executes the boss-finishing Purify Burst when the active boss
// is in Phase 3 (BossHealth.PurifyBurstAvailable). Triggered by PlayerController.OnPurifyBurst.
[RequireComponent(typeof(PlayerController))]
public class PurifyBurst : MonoBehaviour
{
    [SerializeField] private float  _burstRadius    = 8f;
    [SerializeField] private float  _healAmount     = 1;   // hearts healed on success
    [SerializeField] private AudioClip _burstClip;
    [SerializeField] private GameObject _burstVFXPrefab;

    private PlayerHealth _health;

    private void Awake() => _health = GetComponent<PlayerHealth>();

    public void TryActivate()
    {
        // Find closest boss in range with Purify Burst available
        var hits = Physics2D.OverlapCircleAll(transform.position, _burstRadius);
        foreach (var col in hits)
        {
            if (!col.TryGetComponent<BossHealth>(out var bh)) continue;
            if (!bh.PurifyBurstAvailable) continue;

            StartCoroutine(ExecutePurify(bh));
            return;
        }
    }

    private IEnumerator ExecutePurify(BossHealth boss)
    {
        AudioManager.Instance?.PlaySFX(_burstClip);

        if (_burstVFXPrefab != null)
        {
            var vfx = Instantiate(_burstVFXPrefab, transform.position, Quaternion.identity);
            Destroy(vfx, 3f);
        }

        GameEvents.RaisePurifyBurstActivated();

        // Brief dramatic pause
        yield return new WaitForSeconds(0.5f);

        boss.ApplyPurifyBurst();

        // Heal player
        _health?.Heal((int)_healAmount);
    }
}
