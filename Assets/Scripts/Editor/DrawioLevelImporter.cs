#if UNITY_EDITOR
using System.Text.RegularExpressions;
using System.Xml;
using UnityEditor;
using UnityEngine;

// Imports a draw.io SVG (or native .drawio) level-design file into Unity.
// Each diagram page becomes a child GameObject under "ImportedLevels".
//
// Menu: EotW ▸ Import Draw.io Level
//
// Element classification (by draw.io style):
//   umlActor           → PlayerSpawn marker
//   deer_1             → Enemy (EntityController + EntityStateMachine)
//   acute_triangle     → Spike hazard
//   rhombus (#A20025)  → CourageFragment collectible
//   rounded=1 #008a00  → Floating platform
//   #001F00            → Hidden area / cave
//   #60a917 / #60A917  → Solid platform / wall
//   jumpInArrow, text  → Skipped (visual-only in the diagram)
//
// Coordinate mapping:
//   draw.io Y goes DOWN; Unity Y goes UP.
//   Scale and GroundRefY can be tuned below.
public static class DrawioLevelImporter
{
    // ── Tuning constants ────────────────────────────────────────────────────
    // 1 draw.io unit × Scale = 1 Unity unit.
    // At 0.02: a 120-wide floating platform → 2.4 Unity units.
    const float Scale = 0.02f;

    // draw.io Y where the ground surface sits (top edge of the first ground rect).
    const float DrawioGroundY = 350f;

    // Unity Y of the ground surface (matches LevelBuilder / SceneSetup).
    const float UnityGroundY = -3.0f;

    // ── Entry point ─────────────────────────────────────────────────────────

    [MenuItem("EotW/Import Draw.io Level")]
    static void Import()
    {
        var path = EditorUtility.OpenFilePanel(
            "Select Draw.io Level File", Application.dataPath, "svg,drawio");
        if (string.IsNullOrEmpty(path)) return;

        string raw = System.IO.File.ReadAllText(path);
        XmlDocument mxDoc;

        if (raw.TrimStart().StartsWith("<svg") || raw.TrimStart().StartsWith("<?xml"))
        {
            var contentMatch = Regex.Match(raw, @"content=""(.*?)""",
                RegexOptions.Singleline);
            if (!contentMatch.Success)
            {
                // Try loading as native .drawio XML (no SVG wrapper)
                mxDoc = new XmlDocument();
                mxDoc.LoadXml(raw);
                if (mxDoc.DocumentElement.Name != "mxfile")
                {
                    Debug.LogError("[LevelImport] Not a valid draw.io file.");
                    return;
                }
            }
            else
            {
                string content = contentMatch.Groups[1].Value;
                content = DecodeEntities(content);
                mxDoc = new XmlDocument();
                mxDoc.LoadXml(content);
            }
        }
        else
        {
            mxDoc = new XmlDocument();
            mxDoc.LoadXml(raw);
        }

        var diagrams = mxDoc.DocumentElement.SelectNodes("diagram");
        if (diagrams == null || diagrams.Count == 0)
        {
            Debug.LogError("[LevelImport] No <diagram> elements found.");
            return;
        }

        var existing = GameObject.Find("ImportedLevels");
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog("Re-import?",
                    "Delete existing ImportedLevels and reimport?", "Yes", "Cancel"))
                return;
            Undo.DestroyObjectImmediate(existing);
        }

        var root = new GameObject("ImportedLevels");
        int totalElements = 0;

        foreach (XmlNode diagram in diagrams)
        {
            string levelName = diagram.Attributes?["name"]?.Value ?? $"Level_{totalElements}";
            var levelGo = new GameObject(levelName);
            levelGo.transform.SetParent(root.transform, false);

            var model = diagram.SelectSingleNode("mxGraphModel");
            if (model == null) continue;

            var cells = model.SelectNodes(".//mxCell[@vertex='1']");
            int count = 0;
            foreach (XmlNode cell in cells)
            {
                if (ProcessCell(cell, levelGo))
                    count++;
            }

            totalElements += count;
            Debug.Log($"[LevelImport] '{levelName}': {count} elements created.");
        }

        Undo.RegisterCreatedObjectUndo(root, "Import Draw.io Levels");
        EditorUtility.SetDirty(root);
        Selection.activeGameObject = root;

        Debug.Log($"[LevelImport] Done — {diagrams.Count} page(s), " +
                  $"{totalElements} total elements.  Scale = {Scale}");
    }

    // ── Cell processor ──────────────────────────────────────────────────────

    static bool ProcessCell(XmlNode cell, GameObject parent)
    {
        string style = cell.Attributes?["style"]?.Value ?? "";
        string value = cell.Attributes?["value"]?.Value ?? "";

        var geom = cell.SelectSingleNode("mxGeometry");
        if (geom == null) return false;

        float dx = FloatAttr(geom, "x");
        float dy = FloatAttr(geom, "y");
        float dw = FloatAttr(geom, "width");
        float dh = FloatAttr(geom, "height");

        if (dw < 1f && dh < 1f) return false;
        if (style.Contains("locked=1")) return false;
        if (dw > 30000f || dh > 5000f) return false;
        if (style.Contains("text;")) return false;
        if (style.Contains("jumpInArrow")) return false;
        if (style.Contains("singleArrow")) return false;

        Vector4 u = ToUnity(dx, dy, dw, dh);
        float rot = StyleFloat(style, "rotation");

        if (style.Contains("umlActor"))         { MakePlayerSpawn(parent, u);       return true; }
        if (style.Contains("deer_1"))            { MakeEnemy(parent, u);             return true; }
        if (style.Contains("acute_triangle"))    { MakeSpike(parent, u, rot);        return true; }
        if (style.Contains("rhombus"))           { MakeFragment(parent, u);          return true; }
        if (Has(style, "rounded=1") && HasColor(style, "008a00"))
                                                 { MakeFloatingPlatform(parent, u);  return true; }
        if (HasColor(style, "001F00"))            { MakeHiddenArea(parent, u);        return true; }
        if (HasColor(style, "60a917") || HasColor(style, "60A917") || HasColor(style, "008a00"))
                                                 { MakePlatform(parent, u);          return true; }

        return false;
    }

    // ── Coordinate conversion ───────────────────────────────────────────────
    // Returns (centerX, centerY, width, height) in Unity world space.

    static Vector4 ToUnity(float dx, float dy, float dw, float dh)
    {
        float cx = (dx + dw * 0.5f) * Scale;
        float cy = -((dy + dh * 0.5f) - DrawioGroundY) * Scale + UnityGroundY;
        return new Vector4(cx, cy, dw * Scale, dh * Scale);
    }

    // ── Element factories ───────────────────────────────────────────────────

    static void MakePlatform(GameObject parent, Vector4 u)
    {
        var go = Create(parent, "Platform", u.x, u.y, u.z, u.w);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = Sprite();
        sr.color        = new Color(0.25f, 0.45f, 0.15f);
        sr.sortingOrder = -1;
        go.AddComponent<BoxCollider2D>().size = Vector2.one;
    }

    static void MakeHiddenArea(GameObject parent, Vector4 u)
    {
        var go = Create(parent, "HiddenArea", u.x, u.y, u.z, u.w);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = Sprite();
        sr.color        = new Color(0.12f, 0.25f, 0.06f);
        sr.sortingOrder = -2;
        go.AddComponent<BoxCollider2D>().size = Vector2.one;
    }

    static void MakeFloatingPlatform(GameObject parent, Vector4 u)
    {
        float h = Mathf.Max(u.w, 0.3f);
        var go = Create(parent, "FloatingPlatform", u.x, u.y, u.z, h);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = Sprite();
        sr.color        = new Color(0.35f, 0.55f, 0.20f);
        sr.sortingOrder = -1;
        go.AddComponent<BoxCollider2D>().size = Vector2.one;
    }

    static void MakeSpike(GameObject parent, Vector4 u, float rotation)
    {
        var go = Create(parent, "Spike", u.x, u.y, u.z, u.w);
        if (rotation != 0f)
            go.transform.rotation = Quaternion.Euler(0f, 0f, -rotation);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = Sprite();
        sr.color        = new Color(0.90f, 0.08f, 0.00f);
        sr.sortingOrder = 0;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size      = Vector2.one;
        go.AddComponent<Spike>();
    }

    static void MakeEnemy(GameObject parent, Vector4 u)
    {
        var go = new GameObject("Enemy");
        go.transform.SetParent(parent.transform, false);
        go.transform.position   = new Vector3(u.x, u.y, 0f);
        go.transform.localScale = new Vector3(0.8f, 1f, 1f);
        go.layer = LayerMask.NameToLayer("Default");

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = Sprite();
        sr.color        = new Color(0.72f, 0.45f, 0.20f);
        sr.sortingOrder = 1;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale   = 3f;
        rb.freezeRotation = true;
        rb.interpolation  = RigidbodyInterpolation2D.Interpolate;

        go.AddComponent<CapsuleCollider2D>().size = new Vector2(0.8f, 1f);

        var sm   = go.AddComponent<EntityStateMachine>();
        var smSo = new SerializedObject(sm);
        smSo.FindProperty("_startState").enumValueIndex = (int)EntityState.Agitated;
        smSo.ApplyModifiedProperties();

        var ec   = go.AddComponent<EntityController>();
        var ecSo = new SerializedObject(ec);
        ecSo.FindProperty("_entityType").enumValueIndex = (int)EntityType.Deer;
        ecSo.ApplyModifiedProperties();
    }

    static void MakeFragment(GameObject parent, Vector4 u)
    {
        var go = new GameObject("Fragment");
        go.transform.SetParent(parent.transform, false);
        go.transform.position   = new Vector3(u.x, u.y, 0f);
        go.transform.localScale = Vector3.one * 0.5f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = Sprite();
        sr.color        = new Color(0.90f, 0.20f, 0.40f);
        sr.sortingOrder = 2;

        go.AddComponent<CircleCollider2D>().isTrigger = true;
        go.AddComponent<CourageFragment>();
    }

    static void MakePlayerSpawn(GameObject parent, Vector4 u)
    {
        var go = new GameObject("PlayerSpawn");
        go.transform.SetParent(parent.transform, false);
        go.transform.position = new Vector3(u.x, u.y, 0f);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    static GameObject Create(GameObject parent, string name,
                              float x, float y, float w, float h)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.transform.position   = new Vector3(x, y, 0f);
        go.transform.localScale = new Vector3(w, h, 1f);
        go.layer = LayerMask.NameToLayer("Default");
        return go;
    }

    static bool Has(string style, string token) =>
        style.Contains(token);

    static bool HasColor(string style, string hex) =>
        style.IndexOf(hex, System.StringComparison.OrdinalIgnoreCase) >= 0;

    static float FloatAttr(XmlNode n, string attr)
    {
        var v = n.Attributes?[attr]?.Value;
        if (v == null) return 0f;
        float.TryParse(v, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float f);
        return f;
    }

    static float StyleFloat(string style, string key)
    {
        var m = Regex.Match(style, key + @"=(-?[\d.]+)");
        if (!m.Success) return 0f;
        float.TryParse(m.Groups[1].Value, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out float f);
        return f;
    }

    static string DecodeEntities(string s)
    {
        // Single pass only — inner value= attributes use &amp;lt; which
        // decodes to &lt; (valid XML entity).  A second pass would break
        // them into raw '<' inside attribute values.
        // &amp; MUST be last so &amp;lt; → &lt; (not <).
        return s.Replace("&#10;", "\n")
                .Replace("&#13;", "\r")
                .Replace("&lt;", "<")
                .Replace("&gt;", ">")
                .Replace("&quot;", "\"")
                .Replace("&apos;", "'")
                .Replace("&amp;", "&");
    }

    static Sprite Sprite()
    {
        var tex = new Texture2D(2, 2);
        tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
        tex.Apply();
        return UnityEngine.Sprite.Create(
            tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
    }
}
#endif
