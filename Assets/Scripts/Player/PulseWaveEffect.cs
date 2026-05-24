using System.Collections;
using UnityEngine;

public class PulseWaveEffect : MonoBehaviour
{
    private static Sprite _ringSprite;
    private static Sprite _orbSprite;

    // Spawns an expanding ring + one traveling orb per hit entity
    public static void Spawn(Vector3 origin, float radius, Vector3[] hitTargets)
    {
        var ringGo = new GameObject("PulseRing");
        ringGo.transform.position = origin;
        ringGo.AddComponent<PulseWaveEffect>().PlayRing(radius);

        foreach (var target in hitTargets)
        {
            var orbGo = new GameObject("PulseOrb");
            orbGo.transform.position = origin;
            orbGo.AddComponent<PulseWaveEffect>().PlayOrb(target);
        }
    }

    // ── Ring ─────────────────────────────────────────────────────────────────

    private void PlayRing(float radius)
    {
        var sr        = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite     = GetRingSprite();
        sr.color      = new Color(0.35f, 0.88f, 1f, 1f);
        sr.sortingOrder = 5;
        StartCoroutine(AnimateRing(sr, radius));
    }

    private IEnumerator AnimateRing(SpriteRenderer sr, float radius)
    {
        float duration = 0.45f, t = 0f;
        while (t < duration)
        {
            float p     = t / duration;
            float scale = radius * 2f * p;
            transform.localScale = new Vector3(scale, scale, 1f);
            sr.color = new Color(0.35f, 0.88f, 1f, 1f - p);
            t += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

    // ── Orb ──────────────────────────────────────────────────────────────────

    private void PlayOrb(Vector3 target)
    {
        var sr        = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite     = GetOrbSprite();
        sr.color      = new Color(0.5f, 0.95f, 1f, 1f);
        sr.sortingOrder = 6;
        transform.localScale = Vector3.one * 0.35f;
        StartCoroutine(AnimateOrb(sr, target));
    }

    private IEnumerator AnimateOrb(SpriteRenderer sr, Vector3 target)
    {
        Vector3 start = transform.position;
        float duration = 0.18f, t = 0f;
        while (t < duration)
        {
            float p = t / duration;
            transform.position = Vector3.Lerp(start, target, p);
            // slight shrink and fade as it arrives
            float scale = Mathf.Lerp(0.35f, 0.1f, p);
            transform.localScale = Vector3.one * scale;
            sr.color = new Color(0.5f, 0.95f, 1f, 1f - p * 0.6f);
            t += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

    // ── Sprite factories (cached after first call) ────────────────────────────

    private static Sprite GetRingSprite()
    {
        if (_ringSprite != null) return _ringSprite;
        const int size = 128, thickness = 8;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float c = size * 0.5f, outerR = c - 1f, innerR = outerR - thickness;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(c, c));
                float a = Mathf.Min(Mathf.Clamp01(outerR - d + 1f), Mathf.Clamp01(d - innerR + 1f));
                tex.SetPixel(x, y, new Color(1, 1, 1, a));
            }
        tex.Apply();
        _ringSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return _ringSprite;
    }

    private static Sprite GetOrbSprite()
    {
        if (_orbSprite != null) return _orbSprite;
        const int size = 32;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        float c = size * 0.5f, r = c - 1f;
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(c, c));
                tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(r - d + 1f)));
            }
        tex.Apply();
        _orbSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        return _orbSprite;
    }
}
