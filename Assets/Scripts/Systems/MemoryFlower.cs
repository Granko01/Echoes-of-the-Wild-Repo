using System.Collections;
using UnityEngine;

// Chapter 1 — Distressed Mother Beast encounter.
// Memory Flowers grow during Phase 2. Player touching one weakens the boss briefly
// and triggers a recognition moment (boss pauses, looks confused).
[RequireComponent(typeof(Collider2D))]
public class MemoryFlower : MonoBehaviour
{
    [SerializeField] private float _stunDuration  = 2f;
    [SerializeField] private float _damageAmount  = 5f;
    [SerializeField] private AudioClip _touchClip;

    private BossHealth _boss;
    private bool       _used;

    private void Start()
    {
        GetComponent<Collider2D>().isTrigger = true;
        // Find the chapter boss in scene
        _boss = FindFirstObjectByType<BossHealth>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_used || !other.CompareTag("Player")) return;
        _used = true;
        StartCoroutine(FlowerEffect());
    }

    private IEnumerator FlowerEffect()
    {
        AudioManager.Instance?.PlaySFX(_touchClip);

        if (_boss != null)
        {
            _boss.TakeDamage(_damageAmount);
            _boss.NotifyMemoryFlowerTouch();
        }

        // Fade out the flower visually
        var sr = GetComponent<SpriteRenderer>();
        float t = 0f;
        Color start = sr != null ? sr.color : Color.white;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            if (sr != null) sr.color = Color.Lerp(start, Color.clear, t);
            yield return null;
        }

        Destroy(gameObject);
    }
}
