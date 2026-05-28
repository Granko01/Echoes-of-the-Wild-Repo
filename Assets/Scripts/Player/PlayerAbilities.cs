using System.Collections;
using UnityEngine;

// Handles suppression state only. Weapon-based abilities are in WeaponHolder / PurifyBurst.
// SuppressionField triggers still call SetSuppressed(). WeaponBase checks IsSuppressed.
public class PlayerAbilities : MonoBehaviour
{
    private bool _isSuppressed;

    public bool IsSuppressed => _isSuppressed;

    // Called by SuppressionField trigger
    public void SetSuppressed(bool suppressed)
    {
        _isSuppressed = suppressed;
    }

    // Resonance Persistence: brief ability window inside suppression (Act 4 unlock)
    public void UseResonancePersistence()
    {
        if (!_isSuppressed) return;
        StartCoroutine(TemporaryUnsuppress(2f));
    }

    private IEnumerator TemporaryUnsuppress(float duration)
    {
        _isSuppressed = false;
        yield return new WaitForSeconds(duration);
        _isSuppressed = true;
    }
}
