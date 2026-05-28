using System.Collections;
using UnityEngine;

// Spawns a quick scale-up + fade-out flash at a world position.
// No prefab needed — creates its own sprite at runtime.
// Call AttackVFX.Spawn(pos) from any weapon hit method.
public class AttackVFX : MonoBehaviour
{
    public static void Spawn(Vector2 pos, Color color, float size = 0.5f, float duration = 0.18f)
    {
        var go = new GameObject("AttackVFX");
        go.transform.position = pos;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = MakeSprite();
        sr.color        = color;
        sr.sortingOrder = 20;
        var vfx = go.AddComponent<AttackVFX>();
        vfx.StartCoroutine(vfx.PlayRoutine(sr, size, duration));
    }

    // Convenience overloads
    public static void SpawnHit(Vector2 pos)
        => Spawn(pos, new Color(1f, 0.9f, 0.3f, 1f), 0.5f, 0.18f);

    public static void SpawnHeavyHit(Vector2 pos)
    {
        Spawn(pos, new Color(1f, 0.5f, 0.1f, 1f), 0.9f, 0.22f);
        CameraShake.Instance?.Shake(0.18f, 0.18f);
    }

    public static void SpawnPulse(Vector2 pos, float radius)
        => Spawn(pos, new Color(0.4f, 0.85f, 1f, 0.6f), radius * 2f, 0.30f);

    private IEnumerator PlayRoutine(SpriteRenderer sr, float targetSize, float duration)
    {
        float elapsed    = 0f;
        Color startColor = sr.color;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float scale = Mathf.Lerp(0.05f, targetSize, t);
            transform.localScale = new Vector3(scale, scale, 1f);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(1f, 0f, t));
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

    private static Sprite MakeSprite()
    {
        var tex = new Texture2D(2, 2);
        tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 2, 2), Vector2.one * 0.5f, 2f);
    }
}
