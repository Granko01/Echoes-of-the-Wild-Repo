#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

// Run AFTER "EotW/Setup Game Scene".
// Builds the full play-through level: 4 sections + boss (~10-15 min).
//
// Ground top surface = y -3.0.  Player standing centre ≈ y -2.5.
// Single-jump height from ground ≈ 2.4 u   →  player can reach y ≈ -0.1
// Double-jump height from ground ≈ 4.8 u   →  player can reach y ≈  2.3
//
// Platform helper stores y at the *centre* of the sprite.
// A platform with h=0.3 has its top at y + 0.15.
public static class LevelBuilder
{
    [MenuItem("EotW/Build Level")]
    static void BuildLevel()
    {
        var ground = GameObject.Find("Ground");
        if (ground == null)
        {
            Debug.LogError("[LevelBuilder] Run 'EotW/Setup Game Scene' first.");
            return;
        }

        // Extend the ground plane to cover the whole level (x -10 → 125)
        ground.transform.position   = new Vector3(57.5f, -3.5f, 0f);
        ground.transform.localScale = new Vector3(270f, 1f, 1f);

        // Remove the placeholder deer + biome from scene setup
        Wipe("BabyDeer");
        Wipe("BiomeArea");

        var root = GetOrCreate("Level");

        Section1_TutorialForest(root);     // x  0 –  22
        Section2_HighlandPassage(root);    // x 24 –  52
        Section3_RiverCrossing(root);      // x 54 –  80
        Section4_BossArena(root);          // x 82 – 116

        // Reset player to spawn
        var player = GameObject.Find("Punch");
        if (player != null) player.transform.position = new Vector3(0f, 0f, 0f);

        EditorUtility.SetDirty(root);
        Debug.Log("[EotW] Level built — press Play to test. " +
                  "Total leaves: 26 from pickups + 30 from heals = 56.");
    }

    // ── SECTION 1 — Tutorial Forest (x 0 → 22) ──────────────────────────
    // Two agitated deer. Teaches pulsewave + match-3 basics.
    static void Section1_TutorialForest(GameObject root)
    {
        var s = Child(root, "S1_TutorialForest");

        // Gentle platform staircase
        Platform(s,  5f, -2.35f,  4f, 0.3f);   // +0.8 step from ground
        Platform(s, 10f, -1.85f,  3.5f, 0.3f); // +1.3
        Platform(s, 15f, -1.35f,  4f,   0.3f); // +1.8 — 1 leaf reward up here
        Platform(s, 19f, -2.05f,  3f,   0.3f); // step back down

        // Leaves: 5 ground-level + 1 elevated reward
        Leaf(s,  2f, -2.5f);
        Leaf(s,  6f, -2.0f);   // on low platform
        Leaf(s, 11f, -1.5f);   // on mid platform
        Leaf(s, 16f, -1.0f);   // on high platform — rewards the jump
        Leaf(s, 20f, -2.5f);

        // Entities
        var deer1 = Entity(s, "Deer_1", 8f,  EntityType.Deer,
                           EntityState.Agitated, new Color(0.72f, 0.45f, 0.20f));
        var deer2 = Entity(s, "Deer_2", 17f, EntityType.Deer,
                           EntityState.Distressed, new Color(0.65f, 0.40f, 0.18f));

        var b1 = Biome(s, "Biome_1", 11f, 0f, 22f, 8f,
                       new[] { deer1, deer2 });

        Gate(s, "Gate_1", 22.3f, b1);
    }

    // ── SECTION 2 — Highland Passage (x 24 → 52) ────────────────────────
    // Elephant needs match-3 immediately.  Wall-climb cliff + vine swing.
    static void Section2_HighlandPassage(GameObject root)
    {
        var s = Child(root, "S2_HighlandPassage");

        // Raised shelf entry
        Platform(s, 27f, -2.5f, 7f, 0.5f,  new Color(0.30f, 0.40f, 0.18f));

        // Climbing staircase toward cliff
        Platform(s, 32f, -1.85f, 3f, 0.3f);
        Platform(s, 36f, -1.25f, 3f, 0.3f);  // mid jump
        Platform(s, 40f, -0.75f, 3f, 0.3f);  // needs double-jump or wall-climb

        // Tall cliff wall (wall-climb zone)
        MakeClimbZone(s, "Cliff_WallClimb", 33.3f, 0.5f, 0.8f, 7f, ClimbZone.ZoneType.WallClimb);

        // Vine bridge across a gap
        MakeClimbZone(s, "Vine_Bridge", 43f, -0.3f, 1.5f, 3.5f, ClimbZone.ZoneType.VineSwing);

        // Far side descent
        Platform(s, 46f, -1.25f, 3.5f, 0.3f);
        Platform(s, 50f, -2.05f, 3.5f, 0.3f);

        // Leaves: 7 total
        Leaf(s, 25f, -2.5f);
        Leaf(s, 28f, -1.8f);
        Leaf(s, 33f, -1.5f);
        Leaf(s, 37f, -0.9f);
        Leaf(s, 41f, -0.3f);   // reward for reaching the top
        Leaf(s, 47f, -0.9f);
        Leaf(s, 51f, -1.7f);

        // Entities
        var elephant = Entity(s, "Elephant_1", 37f, EntityType.Elephant,
                              EntityState.Overwhelmed, new Color(0.60f, 0.60f, 0.65f));
        var deer3    = Entity(s, "Deer_3", 47f, EntityType.Deer,
                              EntityState.Agitated, new Color(0.68f, 0.42f, 0.18f));

        var b2 = Biome(s, "Biome_2", 38f, 0f, 28f, 9f,
                       new[] { elephant, deer3 });

        Gate(s, "Gate_2", 52.3f, b2);
    }

    // ── SECTION 3 — River Crossing (x 54 → 80) ──────────────────────────
    // Moving platforms over a lowered river.  Fish + Bird pair.
    static void Section3_RiverCrossing(GameObject root)
    {
        var s = Child(root, "S3_RiverCrossing");

        // Stepping stones
        Platform(s, 56f, -1.85f, 2.5f, 0.3f);
        Platform(s, 60f, -1.35f, 2.5f, 0.3f);
        Platform(s, 64f, -1.85f, 2.5f, 0.3f);

        // Moving platforms over the river gap (x 65 – 73)
        MovingPlatform(s, "MovePlat_H1", 67f, -1.85f, 2.5f, 2.0f, false);
        MovingPlatform(s, "MovePlat_V1", 71f, -1.35f, 2.5f, 1.2f, true);

        // Vine ropes over the river
        MakeClimbZone(s, "Vine_River_A", 57f, -0.5f, 1.2f, 4f,   ClimbZone.ZoneType.VineSwing);
        MakeClimbZone(s, "Vine_River_B", 62f, -0.5f, 1.2f, 4f,   ClimbZone.ZoneType.VineSwing);

        // Far bank
        Platform(s, 75f, -2.05f, 4f, 0.3f);
        Platform(s, 78f, -1.35f, 3f, 0.3f);

        // Leaves: 8 total
        Leaf(s, 55f, -2.5f);
        Leaf(s, 57f, -1.5f);
        Leaf(s, 61f, -1.0f);
        Leaf(s, 65f, -1.5f);
        Leaf(s, 68f, -1.5f);  // on/near moving platform
        Leaf(s, 72f, -1.0f);
        Leaf(s, 76f, -1.7f);
        Leaf(s, 79f, -1.0f);

        // Entities
        var fish = Entity(s, "Fish_1", 60f, EntityType.Fish,
                          EntityState.Agitated, new Color(0.20f, 0.50f, 0.90f));
        var bird = Entity(s, "Bird_1", 74f, EntityType.Bird,
                          EntityState.Agitated, new Color(0.85f, 0.65f, 0.20f));

        var b3 = Biome(s, "Biome_3", 67f, 0f, 26f, 9f,
                       new[] { fish, bird });

        Gate(s, "Gate_3", 80.3f, b3);
    }

    // ── SECTION 4 — Boss Arena (x 82 → 116) ─────────────────────────────
    // Dramatic approach corridor → suppression field → Corrupted Ancient.
    static void Section4_BossArena(GameObject root)
    {
        var s = Child(root, "S4_BossArena");

        // Approach corridor — dramatic narrowing path
        Platform(s, 85f, -2.05f, 5f, 0.3f);
        Platform(s, 90f, -1.35f, 4f, 0.3f);

        // Arena floor (darker stone colour)
        Platform(s, 101f, -3.05f, 28f, 0.6f,
                 new Color(0.18f, 0.12f, 0.08f));   // arena stone floor

        // Combat maneuvering platforms inside arena
        Platform(s,  94f, -1.55f, 3f, 0.3f);
        Platform(s,  99f, -1.05f, 3f, 0.3f);
        Platform(s, 105f, -1.55f, 3f, 0.3f);
        Platform(s, 110f, -1.05f, 3f, 0.3f);

        // Approach leaves
        Leaf(s,  83f, -2.5f);
        Leaf(s,  86f, -1.7f);
        Leaf(s,  91f, -1.0f);
        Leaf(s,  95f, -1.2f);
        Leaf(s, 100f, -0.7f);
        Leaf(s, 106f, -2.5f);

        // Boss entity (Turbulent Elephant, 2× scale)
        var boss = Entity(s, "BossElephant", 103f,
                          EntityType.Elephant, EntityState.Turbulent,
                          new Color(0.28f, 0.12f, 0.32f), scale: 2.0f);

        // Boss suppression field (starts inactive; BossController toggles it)
        var suppressGo = new GameObject("BossSuppression");
        suppressGo.transform.SetParent(s.transform, false);
        suppressGo.transform.position = new Vector3(101f, 0f, 0f);
        var sfCol = suppressGo.AddComponent<BoxCollider2D>();
        sfCol.isTrigger = true;
        sfCol.size      = new Vector2(30f, 12f);
        var sf = suppressGo.AddComponent<SuppressionField>();
        suppressGo.SetActive(false);

        // BossController — wire suppression field
        var bossCtrl = boss.gameObject.AddComponent<BossController>();
        var bcSo     = new SerializedObject(bossCtrl);
        bcSo.FindProperty("_suppressionField").objectReferenceValue = sf;
        bcSo.ApplyModifiedProperties();

        // Boss biome (needed for Gate_Final + HealComplete rewards)
        var bossEC = boss.GetComponent<EntityController>();
        Biome(s, "Biome_Boss", 101f, 0f, 30f, 12f, new[] { bossEC });
    }

    // ── Entity factory ────────────────────────────────────────────────────

    static EntityController Entity(GameObject parent, string name, float x,
                                   EntityType type, EntityState state,
                                   Color color, float scale = 0.8f)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.transform.position   = new Vector3(x, 0f, 0f);
        go.transform.localScale = new Vector3(scale, scale * 1.25f, 1f);
        go.layer = LayerMask.NameToLayer("Default");

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = WhiteSprite();
        sr.color        = color;
        sr.sortingOrder = 1;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale   = 3f;
        rb.freezeRotation = true;
        rb.interpolation  = RigidbodyInterpolation2D.Interpolate;

        go.AddComponent<CapsuleCollider2D>().size = new Vector2(0.8f, 1f);

        var sm   = go.AddComponent<EntityStateMachine>();
        var smSo = new SerializedObject(sm);
        smSo.FindProperty("_startState").enumValueIndex = (int)state;
        smSo.ApplyModifiedProperties();

        var ec   = go.AddComponent<EntityController>();
        var ecSo = new SerializedObject(ec);
        ecSo.FindProperty("_entityType").enumValueIndex = (int)type;
        ecSo.ApplyModifiedProperties();

        return ec;
    }

    // ── Level object factories ─────────────────────────────────────────────

    static void Platform(GameObject parent, float x, float y,
                         float w, float h, Color? col = null)
    {
        var go = new GameObject("Platform");
        go.transform.SetParent(parent.transform, false);
        go.transform.position   = new Vector3(x, y, 0f);
        go.transform.localScale = new Vector3(w, h, 1f);
        go.layer = LayerMask.NameToLayer("Default");

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = WhiteSprite();
        sr.color        = col ?? new Color(0.25f, 0.45f, 0.15f);
        sr.sortingOrder = -1;

        go.AddComponent<BoxCollider2D>().size = Vector2.one;
    }

    static void MovingPlatform(GameObject parent, string name,
                               float x, float y, float w,
                               float distance, bool vertical)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.transform.position   = new Vector3(x, y, 0f);
        go.transform.localScale = new Vector3(w, 0.3f, 1f);
        go.layer = LayerMask.NameToLayer("Default");

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = WhiteSprite();
        sr.color        = new Color(0.35f, 0.55f, 0.20f);
        sr.sortingOrder = -1;

        go.AddComponent<BoxCollider2D>().size = Vector2.one;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.isKinematic    = true;
        rb.interpolation  = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;

        var pm   = go.AddComponent<PlatformMover>();
        var pmSo = new SerializedObject(pm);
        pmSo.FindProperty("_distance").floatValue = distance;
        pmSo.FindProperty("_vertical").boolValue  = vertical;
        pmSo.ApplyModifiedProperties();
    }

    static void Leaf(GameObject parent, float x, float y)
    {
        var go = new GameObject("Leaf");
        go.transform.SetParent(parent.transform, false);
        go.transform.position   = new Vector3(x, y, 0f);
        go.transform.localScale = Vector3.one * 0.35f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = WhiteSprite();
        sr.color        = new Color(0.28f, 0.88f, 0.18f);
        sr.sortingOrder = 2;

        go.AddComponent<CircleCollider2D>().isTrigger = true;
        go.AddComponent<Leaf>();
    }

    static BiomeArea Biome(GameObject parent, string name,
                           float cx, float cy, float w, float h,
                           EntityController[] entities)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.transform.position = new Vector3(cx, cy, 0f);

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size      = new Vector2(w, h);

        go.AddComponent<AudioSource>();
        var ba   = go.AddComponent<BiomeArea>();
        var baSo = new SerializedObject(ba);
        var ep   = baSo.FindProperty("_requiredEntities");
        ep.arraySize = entities.Length;
        for (int i = 0; i < entities.Length; i++)
            ep.GetArrayElementAtIndex(i).objectReferenceValue = entities[i];
        baSo.ApplyModifiedProperties();

        return ba;
    }

    static void Gate(GameObject parent, string name, float x, BiomeArea biome)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.transform.position   = new Vector3(x, -0.5f, 0f);
        go.transform.localScale = new Vector3(0.5f, 7f, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = WhiteSprite();
        sr.color        = new Color(0.55f, 0.22f, 0.08f, 0.92f);
        sr.sortingOrder = 3;

        go.AddComponent<BoxCollider2D>().size = Vector2.one;

        var pg   = go.AddComponent<ProgressGate>();
        var pgSo = new SerializedObject(pg);
        pgSo.FindProperty("_triggerBiome").objectReferenceValue = biome;
        pgSo.ApplyModifiedProperties();
    }

    static void MakeClimbZone(GameObject parent, string name,
                              float x, float y, float w, float h,
                              ClimbZone.ZoneType zoneType)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        go.transform.position = new Vector3(x, y, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite       = WhiteSprite();
        sr.color        = zoneType == ClimbZone.ZoneType.VineSwing
                          ? new Color(0.30f, 0.60f, 0.10f, 0.55f)
                          : new Color(0.45f, 0.35f, 0.10f, 0.55f);
        sr.sortingOrder = -2;

        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size      = new Vector2(w, h);

        var cz   = go.AddComponent<ClimbZone>();
        var czSo = new SerializedObject(cz);
        czSo.FindProperty("_type").enumValueIndex = (int)zoneType;
        czSo.ApplyModifiedProperties();
    }

    // ── Utilities ─────────────────────────────────────────────────────────

    static void Wipe(string name)
    {
        var go = GameObject.Find(name);
        if (go != null) Object.DestroyImmediate(go);
    }

    static GameObject GetOrCreate(string name)
    {
        var go = GameObject.Find(name);
        return go != null ? go : new GameObject(name);
    }

    static GameObject Child(GameObject parent, string name)
    {
        var t = parent.transform.Find(name);
        if (t != null) return t.gameObject;
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    static Sprite WhiteSprite()
    {
        var tex = new Texture2D(2, 2);
        tex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
    }
}
#endif
