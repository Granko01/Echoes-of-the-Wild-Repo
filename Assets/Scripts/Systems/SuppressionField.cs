using UnityEngine;

// Attach to a trigger collider to create an Act 4 suppression zone.
// Disables player abilities inside. Resonance Persistence (post-Act 4 unlock)
// allows brief ability windows even inside the field.
[RequireComponent(typeof(Collider2D))]
public class SuppressionField : MonoBehaviour
{
    [SerializeField] private bool _allowResonancePersistence;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<PlayerAbilities>(out var abilities)) return;
        if (!_allowResonancePersistence)
            abilities.SetSuppressed(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerAbilities>(out var abilities))
            abilities.SetSuppressed(false);
    }

    // Called after Act 4 Hug scene to allow brief ability use inside fields
    public void UnlockResonancePersistence() => _allowResonancePersistence = true;
}
