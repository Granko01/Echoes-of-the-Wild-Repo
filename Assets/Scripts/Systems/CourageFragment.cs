using System.Collections;
using UnityEngine;

// Boss reward collectible. Player walks over it to collect.
// Adds Echo Fragments to WeaponUpgradeSystem.
[RequireComponent(typeof(Collider2D))]
public class CourageFragment : MonoBehaviour
{
    [SerializeField] private int       _fragmentValue = 5;
    [SerializeField] private AudioClip _collectClip;

    private bool _collected;

    private void Start() => GetComponent<Collider2D>().isTrigger = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_collected || !other.CompareTag("Player")) return;
        _collected = true;
        StartCoroutine(CollectRoutine());
    }

    private IEnumerator CollectRoutine()
    {
        AudioManager.Instance?.PlaySFX(_collectClip);
        WeaponUpgradeSystem.Instance?.AddFragments(_fragmentValue);

        // Simple bounce-up + fade collect animation
        float t = 0f;
        Vector3 start = transform.position;
        var sr = GetComponent<SpriteRenderer>();
        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            transform.position = start + Vector3.up * (t * 1.5f);
            if (sr != null) sr.color = new Color(1f, 1f, 1f, 1f - t);
            yield return null;
        }

        Destroy(gameObject);
    }
}
