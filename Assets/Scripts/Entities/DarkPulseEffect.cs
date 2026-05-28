using System.Collections;
using UnityEngine;

// Boss counterpart to PulseWaveEffect. Spawns an expanding red ring at the boss
// position — no prefabs or assets needed, sprite is generated in code.
public class DarkPulseEffect : MonoBehaviour
{
    private static Sprite _ringSprite;

    public static void Spawn(Vector3 origin, float radius)
    {
        var go = new GameObject("DarkPulseRing");
        go.transform.position = origin;
        go.AddComponent<DarkPulseEffect>().Play(radius);
    }

    private void Play(float radius)
    {
        var sr        = gameObject.AddComponent<SpriteRenderer>();
        sr.sprite     = GetRingSprite();
        sr.color      = new Color(1f, 0.15f, 0.1f, 1f);   // red
        sr.sortingOrder = 5;
        StartCoroutine(Animate(sr, radius));
    }

    private IEnumerator Animate(SpriteRenderer sr, float radius)
    {
        float duration = 0.55f, t = 0f;
        while (t < duration)
        {
            float p     = t / duration;
            float scale = radius * 2f * p;
            transform.localScale = new Vector3(scale, scale, 1f);
            sr.color = new Color(1f, 0.15f, 0.1f, 1f - p);
            t += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

    private static Sprite GetRingSprite()
    {
        if (_ringSprite != null) return _ringSprite;
        const int size = 128, thickness = 10;
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
}
