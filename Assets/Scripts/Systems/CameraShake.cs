using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Shake(float duration = 0.15f, float strength = 0.12f)
        => StartCoroutine(ShakeRoutine(duration, strength));

    private IEnumerator ShakeRoutine(float duration, float strength)
    {
        Vector3 origin  = transform.localPosition;
        float   elapsed = 0f;
        while (elapsed < duration)
        {
            float pct = 1f - elapsed / duration;
            transform.localPosition = origin + (Vector3)(Random.insideUnitCircle * strength * pct);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = origin;
    }
}
