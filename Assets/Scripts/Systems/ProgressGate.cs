using System.Collections;
using UnityEngine;

// A solid barrier that slides upward and deactivates when its assigned BiomeArea is healed.
// Leave _triggerBiome null to open on ANY biome heal.
public class ProgressGate : MonoBehaviour
{
    [SerializeField] private BiomeArea _triggerBiome;
    [SerializeField] private float     _openDuration = 1.2f;

    private bool _open;

    private void OnEnable()  => GameEvents.OnHealComplete += OnHealComplete;
    private void OnDisable() => GameEvents.OnHealComplete -= OnHealComplete;

    private void OnHealComplete(BiomeArea area)
    {
        if (_open) return;
        if (_triggerBiome != null && area != _triggerBiome) return;
        _open = true;
        StartCoroutine(OpenRoutine());
    }

    private IEnumerator OpenRoutine()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        Vector3 start   = transform.position;
        Vector3 end     = start + Vector3.up * 9f;
        float   elapsed = 0f;

        while (elapsed < _openDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, Mathf.SmoothStep(0f, 1f, elapsed / _openDuration));
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
