#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

public static class SceneSetup
{
    [MenuItem("EotW/Setup Game Scene")]
    static void Setup()
    {
        // ── Event System (required for all UI button input) ──────────────────
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            var esGo = new GameObject("EventSystem");
            esGo.AddComponent<EventSystem>();
            esGo.AddComponent<InputSystemUIInputModule>();
        }

        // ── Managers ────────────────────────────────────────────────────────
        Make<GameManager>  ("GameManager");
        Make<AudioManager> ("AudioManager");
        Make<Match3Manager>("Match3Manager");
        Make<BondSystem>   ("BondSystem");

        // ── Camera ──────────────────────────────────────────────────────────
        var camGo = GameObject.FindFirstObjectByType<Camera>()?.gameObject ?? new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = Ensure<Camera>(camGo);
        cam.orthographic     = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor  = new Color(0.08f, 0.12f, 0.08f);
        cam.clearFlags       = CameraClearFlags.SolidColor;

        // ── Depth layer helpers ──────────────────────────────────────────────
        static GameObject MakeDepthLayer(string name, float y, float scaleX, float scaleY,
                                         Color color, int order, float factorX, float factorY = 0f)
        {
            var go = GetOrCreate(name);
            go.transform.position   = new Vector3(0f, y, 0f);
            go.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            var sr = Ensure<SpriteRenderer>(go);
            sr.sprite       = DefaultSprite();
            sr.color        = color;
            sr.sortingOrder = order;
            var px   = Ensure<ParallaxLayer>(go);
            var pxSo = new SerializedObject(px);
            pxSo.FindProperty("_parallaxFactorX").floatValue = factorX;
            pxSo.FindProperty("_parallaxFactorY").floatValue = factorY;
            pxSo.ApplyModifiedProperties();
            return go;
        }

        static GameObject MakeTreeGroup(string name, float factorX, int sortOrder,
                                        Color trunkColor, Color canopyColor,
                                        float[] xs, float[] trunkYs, float[] canopyXs, float treeScale)
        {
            var root = GetOrCreate(name);
            root.transform.position = Vector3.zero;
            for (int i = 0; i < xs.Length; i++)
            {
                var treeRoot = GetOrCreateChild(root, $"Tree_{i}");
                treeRoot.transform.localPosition = new Vector3(xs[i], -1.5f * treeScale, 0f);

                var trunk = GetOrCreateChild(treeRoot, "Trunk");
                trunk.transform.localPosition = Vector3.zero;
                trunk.transform.localScale    = new Vector3(0.3f * treeScale, trunkYs[i] * treeScale, 1f);
                var trSr = Ensure<SpriteRenderer>(trunk);
                trSr.sprite = DefaultSprite(); trSr.color = trunkColor; trSr.sortingOrder = sortOrder;

                var canopy = GetOrCreateChild(treeRoot, "Canopy");
                float canopyOffsetY = trunkYs[i] * treeScale * 0.5f + 0.6f * treeScale;
                canopy.transform.localPosition = new Vector3(0f, canopyOffsetY, 0f);
                canopy.transform.localScale    = new Vector3(canopyXs[i] * treeScale, canopyXs[i] * treeScale * 1.2f, 1f);
                var caSr = Ensure<SpriteRenderer>(canopy);
                caSr.sprite = DefaultSprite(); caSr.color = canopyColor; caSr.sortingOrder = sortOrder;
            }
            var px = Ensure<ParallaxLayer>(root);
            var pxSo = new SerializedObject(px);
            pxSo.FindProperty("_parallaxFactorX").floatValue = factorX;
            pxSo.FindProperty("_parallaxFactorY").floatValue = 0f;
            pxSo.ApplyModifiedProperties();
            return root;
        }

        // Camera BG colour matches the sky layer so there's no seam
        cam.backgroundColor = new Color(0.26f, 0.46f, 0.40f);

        // ── Depth stack (back → front) ────────────────────────────────────────
        // Sky — fully locked (no parallax), fills the camera backdrop
        MakeDepthLayer("Sky",        1f,  200f, 14f, new Color(0.26f, 0.46f, 0.40f), -20, 0.00f);
        // Far haze/horizon silhouette
        MakeDepthLayer("FarHills",  -0.5f, 200f,  5f, new Color(0.18f, 0.36f, 0.22f), -16, 0.05f);
        // Background forest mass
        MakeDepthLayer("Background", 0f,  200f, 12f, new Color(0.10f, 0.22f, 0.12f), -12, 0.12f);

        // Back trees — smaller, lighter, lots of them
        MakeTreeGroup("BackTrees", 0.22f, -8,
            new Color(0.42f, 0.30f, 0.14f), new Color(0.14f, 0.32f, 0.14f),
            new float[] { -10f, -7f, -4f, -1f, 2f, 5f, 8f, 11f },
            new float[] { 1.6f, 1.8f, 1.5f, 1.7f, 1.6f, 1.9f, 1.5f, 1.7f },
            new float[] { 1.1f, 1.3f, 1.0f, 1.2f, 1.1f, 1.4f, 1.0f, 1.2f },
            0.75f);

        // Mid trees — normal size, medium colour
        MakeTreeGroup("Trees", 0.38f, -4,
            new Color(0.36f, 0.22f, 0.09f), new Color(0.09f, 0.28f, 0.09f),
            new float[] { -8f, -5f, -2f, 2f, 5f, 8f, -3.5f },
            new float[] { 2.0f, 2.4f, 1.8f, 2.2f, 2.6f, 1.9f, 2.1f },
            new float[] { 1.4f, 1.6f, 1.2f, 1.5f, 1.7f, 1.3f, 1.5f },
            1.0f);

        // Front trees — larger, darker, fewer — partially overlap the player
        MakeTreeGroup("FrontTrees", 0.60f, 2,
            new Color(0.24f, 0.14f, 0.05f), new Color(0.04f, 0.18f, 0.05f),
            new float[] { -9f, -3f, 4f, 9f },
            new float[] { 2.6f, 2.8f, 2.5f, 2.7f },
            new float[] { 1.8f, 2.0f, 1.7f, 1.9f },
            1.3f);

        // Foreground vegetation strip — in front of everything, moves fast
        MakeDepthLayer("Foreground", -3.2f, 200f, 1.6f, new Color(0.03f, 0.10f, 0.03f), 6, 0.82f);

        // ── Ground ──────────────────────────────────────────────────────────
        var ground = GetOrCreate("Ground");
        ground.transform.position = new Vector3(0, -3.5f, 0);
        ground.transform.localScale = new Vector3(200, 1, 1);
        var gSr = Ensure<SpriteRenderer>(ground);
        gSr.sprite = DefaultSprite();
        gSr.color  = new Color(0.15f, 0.35f, 0.1f);
        gSr.sortingOrder = -1;
        var gCol = Ensure<BoxCollider2D>(ground);
        gCol.size = Vector2.one;
        ground.layer = LayerMask.NameToLayer("Default");

        // ── Player (Punch) ──────────────────────────────────────────────────
        var player = GetOrCreate("Punch");
        player.transform.position = new Vector3(0, 0, 0);
        player.layer = LayerMask.NameToLayer("Default");

        var pSr = Ensure<SpriteRenderer>(player);
        pSr.sprite = DefaultSprite();
        pSr.color  = new Color(1f, 0.85f, 0.4f);   // warm golden = Punch
        player.transform.localScale = new Vector3(0.8f, 1f, 1f);

        var rb = Ensure<Rigidbody2D>(player);
        rb.gravityScale   = 3f;
        rb.freezeRotation = true;
        rb.interpolation  = RigidbodyInterpolation2D.Interpolate;

        var col = Ensure<CapsuleCollider2D>(player);
        col.size = new Vector2(0.8f, 1f);

        Ensure<PlayerController>(player);
        Ensure<PlayerAbilities>(player);

        // PlayerInput — sends "OnMove", "OnJump" etc. to PlayerController on same GameObject
        var pi = Ensure<UnityEngine.InputSystem.PlayerInput>(player);
        var inputAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.InputSystem.InputActionAsset>(
            "Assets/InputSystem_Actions.inputactions");
        if (inputAsset != null)
        {
            pi.actions        = inputAsset;
            pi.defaultActionMap = "Player";
            pi.notificationBehavior = UnityEngine.InputSystem.PlayerNotifications.SendMessages;
        }

        var gcGo = GetOrCreateChild(player, "GroundCheck");
        gcGo.transform.localPosition = new Vector3(0, -0.55f, 0);

        var pcSo = new SerializedObject(player.GetComponent<PlayerController>());
        pcSo.FindProperty("_groundCheck").objectReferenceValue = gcGo.transform;
        // Ground layer mask — layer 0 (Default) = bit 1
        pcSo.FindProperty("_groundLayer").intValue = 1;
        pcSo.ApplyModifiedProperties();

        var paSo = new SerializedObject(player.GetComponent<PlayerAbilities>());
        paSo.FindProperty("_entityLayer").intValue = 1 << LayerMask.NameToLayer("Default");
        paSo.ApplyModifiedProperties();

        // Wire CameraFollow — must happen after player is created
        Ensure<CameraFollow>(camGo);
        var camFollowSo = new SerializedObject(camGo.GetComponent<CameraFollow>());
        camFollowSo.FindProperty("_target").objectReferenceValue = player.transform;
        camFollowSo.ApplyModifiedProperties();

        // ── HUD Canvas ──────────────────────────────────────────────────────
        var canvas = GetOrCreate("HUD");
        var cv = Ensure<Canvas>(canvas);
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        Ensure<CanvasScaler>(canvas);
        Ensure<GraphicRaycaster>(canvas);
        Ensure<HUDManager>(canvas);

        var lGo = GetOrCreateChild(canvas, "LeavesText");
        var rt = Ensure<RectTransform>(lGo);
        rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(20, -20);
        rt.sizeDelta = new Vector2(150, 40);
        var tmp = Ensure<TextMeshProUGUI>(lGo);
        tmp.text      = "Leaves: 0";
        tmp.fontSize  = 22;
        tmp.color     = Color.white;

        // Suppression overlay — full-screen red tint, starts hidden
        var overlayGo = GetOrCreateChild(canvas, "SuppressionOverlay");
        var overlayRt = Ensure<RectTransform>(overlayGo);
        overlayRt.anchorMin = Vector2.zero;
        overlayRt.anchorMax = Vector2.one;
        overlayRt.offsetMin = overlayRt.offsetMax = Vector2.zero;
        var overlayImg = Ensure<Image>(overlayGo);
        overlayImg.color = new Color(0.8f, 0.1f, 0.1f, 0.35f);
        overlayGo.SetActive(false);

        // Deer bond fill bar
        var deerBarBg = GetOrCreateChild(canvas, "DeerBondBg");
        var deerBgRt = Ensure<RectTransform>(deerBarBg);
        deerBgRt.anchorMin = deerBgRt.anchorMax = new Vector2(0, 1);
        deerBgRt.anchoredPosition = new Vector2(20, -70);
        deerBgRt.sizeDelta = new Vector2(100, 16);
        var deerBgImg = Ensure<Image>(deerBarBg);
        deerBgImg.color = new Color(0.2f, 0.2f, 0.2f);

        var deerFillGo = GetOrCreateChild(deerBarBg, "DeerBondFill");
        var deerFillRt = Ensure<RectTransform>(deerFillGo);
        deerFillRt.anchorMin = Vector2.zero;
        deerFillRt.anchorMax = Vector2.one;
        deerFillRt.offsetMin = deerFillRt.offsetMax = Vector2.zero;
        var deerFillImg = Ensure<Image>(deerFillGo);
        deerFillImg.color      = new Color(0.72f, 0.45f, 0.2f);
        deerFillImg.type       = Image.Type.Filled;
        deerFillImg.fillMethod = Image.FillMethod.Horizontal;
        deerFillImg.fillAmount = 0f;

        // Elephant bond fill bar
        var elephBarBg = GetOrCreateChild(canvas, "ElephantBondBg");
        var elephBgRt = Ensure<RectTransform>(elephBarBg);
        elephBgRt.anchorMin = elephBgRt.anchorMax = new Vector2(0, 1);
        elephBgRt.anchoredPosition = new Vector2(20, -95);
        elephBgRt.sizeDelta = new Vector2(100, 16);
        var elephBgImg = Ensure<Image>(elephBarBg);
        elephBgImg.color = new Color(0.2f, 0.2f, 0.2f);

        var elephFillGo = GetOrCreateChild(elephBarBg, "ElephantBondFill");
        var elephFillRt = Ensure<RectTransform>(elephFillGo);
        elephFillRt.anchorMin = Vector2.zero;
        elephFillRt.anchorMax = Vector2.one;
        elephFillRt.offsetMin = elephFillRt.offsetMax = Vector2.zero;
        var elephFillImg = Ensure<Image>(elephFillGo);
        elephFillImg.color      = new Color(0.6f, 0.6f, 0.6f);
        elephFillImg.type       = Image.Type.Filled;
        elephFillImg.fillMethod = Image.FillMethod.Horizontal;
        elephFillImg.fillAmount = 0f;

        var hudSo = new SerializedObject(canvas.GetComponent<HUDManager>());
        hudSo.FindProperty("_leavesText").objectReferenceValue        = tmp;
        hudSo.FindProperty("_suppressionOverlay").objectReferenceValue = overlayGo;
        hudSo.FindProperty("_deerBondFill").objectReferenceValue      = deerFillImg;
        hudSo.FindProperty("_elephantBondFill").objectReferenceValue  = elephFillImg;
        hudSo.ApplyModifiedProperties();

        // ── Baby Deer ───────────────────────────────────────────────────────
        var deer = GetOrCreate("BabyDeer");
        deer.transform.position   = new Vector3(4f, 0f, 0f);
        deer.transform.localScale = new Vector3(0.7f, 0.7f, 1f);
        deer.layer = LayerMask.NameToLayer("Default");

        var dSr = Ensure<SpriteRenderer>(deer);
        dSr.sprite = DefaultSprite();
        dSr.color  = new Color(0.72f, 0.45f, 0.2f);   // brown

        var dRb = Ensure<Rigidbody2D>(deer);
        dRb.gravityScale   = 3f;
        dRb.freezeRotation = true;
        Ensure<CapsuleCollider2D>(deer);

        Ensure<EntityStateMachine>(deer);
        Ensure<EntityController>(deer);

        var dSmSo = new SerializedObject(deer.GetComponent<EntityStateMachine>());
        dSmSo.FindProperty("_startState").enumValueIndex = (int)EntityState.Agitated;
        dSmSo.ApplyModifiedProperties();

        var dEcSo = new SerializedObject(deer.GetComponent<EntityController>());
        dEcSo.FindProperty("_entityType").enumValueIndex = (int)EntityType.Deer;
        dEcSo.ApplyModifiedProperties();

        // ── Biome Area (tracks the deer) ────────────────────────────────────
        var biome = GetOrCreate("BiomeArea");
        biome.transform.position = Vector3.zero;
        var biomeCol = Ensure<BoxCollider2D>(biome);
        biomeCol.isTrigger = true;
        biomeCol.size      = new Vector2(20f, 10f);
        var biomeArea = Ensure<BiomeArea>(biome);
        Ensure<AudioSource>(biome);
        var biomeSo = new SerializedObject(biomeArea);
        var entitiesProp = biomeSo.FindProperty("_requiredEntities");
        entitiesProp.arraySize = 1;
        entitiesProp.GetArrayElementAtIndex(0).objectReferenceValue = deer.GetComponent<EntityController>();
        biomeSo.ApplyModifiedProperties();

        // ── Match-3 Panel ───────────────────────────────────────────────────
        var m3Panel = GetOrCreateChild(canvas, "Match3Panel");
        var panelRt = Ensure<RectTransform>(m3Panel);
        panelRt.anchorMin       = new Vector2(0.5f, 0.5f);
        panelRt.anchorMax       = new Vector2(0.5f, 0.5f);
        panelRt.anchoredPosition = Vector2.zero;
        panelRt.sizeDelta       = new Vector2(600, 600);

        var panelImg = Ensure<Image>(m3Panel);
        panelImg.color = new Color(0.1f, 0.1f, 0.15f, 0.92f);

        Ensure<GridLayoutGroup>(m3Panel);
        Ensure<Match3UI>(m3Panel);
        m3Panel.SetActive(false);

        // Wire Match3Manager to the panel
        var mmSo = new SerializedObject(GameObject.Find("Match3Manager").GetComponent<Match3Manager>());
        mmSo.FindProperty("_gridUI").objectReferenceValue = m3Panel;
        mmSo.ApplyModifiedProperties();

        EditorUtility.SetDirty(player);
        Debug.Log("[EotW] Scene ready — press Play. Walk into the deer and press Z to pulse it.");
    }

    static Sprite DefaultSprite()
    {
        var tex = new Texture2D(2, 2);
        tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);
    }

    static GameObject Make<T>(string name) where T : Component
        => Ensure<T>(GetOrCreate(name)).gameObject;

    static GameObject GetOrCreate(string name)
    {
        var go = GameObject.Find(name);
        return go != null ? go : new GameObject(name);
    }

    static GameObject GetOrCreateChild(GameObject parent, string name)
    {
        var t = parent.transform.Find(name);
        if (t != null) return t.gameObject;
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        return go;
    }

    static T Ensure<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        if (c == null) c = go.AddComponent<T>();
        return c;
    }

    static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void MakeButtonLabel(GameObject btnGo, string text, int fontSize)
    {
        var go = GetOrCreateChild(btnGo, "Label");
        var rt = Ensure<RectTransform>(go);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var tmp = Ensure<TextMeshProUGUI>(go);
        tmp.text      = text;
        tmp.fontSize  = fontSize;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color     = Color.white;
    }

    // ── Main Menu Scene Builder ───────────────────────────────────────────

    [MenuItem("EotW/Setup Main Menu Scene")]
    static void SetupMainMenu()
    {
        string scenePath = "Assets/Scenes/MainMenu.unity";
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // ── Camera ──────────────────────────────────────────────────────────
        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = Ensure<Camera>(camGo);
        cam.orthographic     = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor  = new Color(0.04f, 0.08f, 0.06f);
        cam.clearFlags       = CameraClearFlags.SolidColor;

        // ── Parallax layers ─────────────────────────────────────────────────
        // BG — slowest drift, darkest green
        var bgLayer = new GameObject("ParallaxBG");
        bgLayer.transform.position = new Vector3(0f, 0f, 10f);
        var bgSr = Ensure<SpriteRenderer>(bgLayer);
        bgSr.sprite       = DefaultSprite();
        bgSr.color        = new Color(0.10f, 0.20f, 0.12f);
        bgSr.sortingOrder = -10;
        bgLayer.transform.localScale = new Vector3(40f, 12f, 1f);
        var bgPl = Ensure<ParallaxLayer>(bgLayer);
        var bgPlSo = new SerializedObject(bgPl);
        bgPlSo.FindProperty("_parallaxFactorX").floatValue    = 0.05f;
        bgPlSo.FindProperty("_autoScrollSpeed").vector2Value = new Vector2(-0.4f, 0f);
        bgPlSo.ApplyModifiedProperties();

        // Mid — medium drift
        var midLayer = new GameObject("ParallaxMid");
        midLayer.transform.position = new Vector3(0f, 0f, 5f);
        var midSr = Ensure<SpriteRenderer>(midLayer);
        midSr.sprite       = DefaultSprite();
        midSr.color        = new Color(0.06f, 0.22f, 0.08f);
        midSr.sortingOrder = -5;
        midLayer.transform.localScale = new Vector3(30f, 8f, 1f);
        var midPl = Ensure<ParallaxLayer>(midLayer);
        var midPlSo = new SerializedObject(midPl);
        midPlSo.FindProperty("_parallaxFactorX").floatValue    = 0.30f;
        midPlSo.FindProperty("_autoScrollSpeed").vector2Value = new Vector2(-0.8f, 0f);
        midPlSo.ApplyModifiedProperties();

        // FG — fastest drift, darkest
        var fgLayer = new GameObject("ParallaxFG");
        fgLayer.transform.position = new Vector3(0f, -2f, 2f);
        var fgSr = Ensure<SpriteRenderer>(fgLayer);
        fgSr.sprite       = DefaultSprite();
        fgSr.color        = new Color(0.03f, 0.12f, 0.04f);
        fgSr.sortingOrder = 0;
        fgLayer.transform.localScale = new Vector3(20f, 3f, 1f);
        var fgPl = Ensure<ParallaxLayer>(fgLayer);
        var fgPlSo = new SerializedObject(fgPl);
        fgPlSo.FindProperty("_parallaxFactorX").floatValue    = 0.60f;
        fgPlSo.FindProperty("_autoScrollSpeed").vector2Value = new Vector2(-1.4f, 0f);
        fgPlSo.ApplyModifiedProperties();

        // ── EventSystem ─────────────────────────────────────────────────────
        var esGo = new GameObject("EventSystem");
        esGo.AddComponent<EventSystem>();
        esGo.AddComponent<InputSystemUIInputModule>();

        // ── AudioManager ────────────────────────────────────────────────────
        if (Object.FindFirstObjectByType<AudioManager>() == null)
            Make<AudioManager>("AudioManager");

        // ── Canvas ──────────────────────────────────────────────────────────
        var canvasGo = new GameObject("MenuCanvas");
        var cv = Ensure<Canvas>(canvasGo);
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = Ensure<CanvasScaler>(canvasGo);
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight  = 0.5f;
        Ensure<GraphicRaycaster>(canvasGo);

        // ── Main Panel ──────────────────────────────────────────────────────
        var mainPanel = GetOrCreateChild(canvasGo, "MainPanel");
        StretchFull(Ensure<RectTransform>(mainPanel));
        Ensure<Image>(mainPanel).color = new Color(0f, 0f, 0f, 0f);

        // Title
        var titleGo = GetOrCreateChild(mainPanel, "TitleText");
        var titleRt = Ensure<RectTransform>(titleGo);
        titleRt.anchorMin        = new Vector2(0.5f, 0.75f);
        titleRt.anchorMax        = new Vector2(0.5f, 0.75f);
        titleRt.anchoredPosition = Vector2.zero;
        titleRt.sizeDelta        = new Vector2(800f, 120f);
        var titleTmp = Ensure<TextMeshProUGUI>(titleGo);
        titleTmp.text      = "ECHOES OF THE WILD";
        titleTmp.fontSize  = 64;
        titleTmp.alignment = TextAlignmentOptions.Center;
        titleTmp.color     = Color.white;

        // Buttons: Start, Settings, How To Play
        string[] mainBtnLabels = { "Start", "Settings", "How To Play" };
        var mainBtns = new Button[mainBtnLabels.Length];
        for (int i = 0; i < mainBtnLabels.Length; i++)
        {
            var btnGo = GetOrCreateChild(mainPanel, mainBtnLabels[i] + "Button");
            var btnRt = Ensure<RectTransform>(btnGo);
            btnRt.anchorMin        = new Vector2(0.5f, 0.5f);
            btnRt.anchorMax        = new Vector2(0.5f, 0.5f);
            btnRt.anchoredPosition = new Vector2(0f, 80f + -90f * i);
            btnRt.sizeDelta        = new Vector2(320f, 70f);
            Ensure<Image>(btnGo).color = new Color(0.15f, 0.35f, 0.15f);
            mainBtns[i] = Ensure<Button>(btnGo);

            var lblGo = GetOrCreateChild(btnGo, "Label");
            var lblRt = Ensure<RectTransform>(lblGo);
            lblRt.anchorMin = Vector2.zero;
            lblRt.anchorMax = Vector2.one;
            lblRt.offsetMin = lblRt.offsetMax = Vector2.zero;
            var lbl = Ensure<TextMeshProUGUI>(lblGo);
            lbl.text      = mainBtnLabels[i].ToUpper();
            lbl.fontSize  = 28;
            lbl.alignment = TextAlignmentOptions.Center;
            lbl.color     = Color.white;
        }

        // ── Settings Panel ──────────────────────────────────────────────────
        var settingsPanel = GetOrCreateChild(canvasGo, "SettingsPanel");
        StretchFull(Ensure<RectTransform>(settingsPanel));
        Ensure<Image>(settingsPanel).color = new Color(0f, 0f, 0f, 0.6f);

        // Music slider
        var musicSliderGo = GetOrCreateChild(settingsPanel, "MusicSlider");
        var musicSliderRt = Ensure<RectTransform>(musicSliderGo);
        musicSliderRt.anchorMin        = new Vector2(0.5f, 0.5f);
        musicSliderRt.anchorMax        = new Vector2(0.5f, 0.5f);
        musicSliderRt.anchoredPosition = new Vector2(80f, 120f);
        musicSliderRt.sizeDelta        = new Vector2(400f, 40f);
        var musicSlider = Ensure<Slider>(musicSliderGo);
        musicSlider.minValue = 0f; musicSlider.maxValue = 1f; musicSlider.value = 0.8f;

        var msLblGo = GetOrCreateChild(settingsPanel, "MusicLabel");
        var msLblRt = Ensure<RectTransform>(msLblGo);
        msLblRt.anchorMin        = new Vector2(0.5f, 0.5f);
        msLblRt.anchorMax        = new Vector2(0.5f, 0.5f);
        msLblRt.anchoredPosition = new Vector2(-220f, 120f);
        msLblRt.sizeDelta        = new Vector2(200f, 40f);
        var msLbl = Ensure<TextMeshProUGUI>(msLblGo);
        msLbl.text = "Music Volume"; msLbl.fontSize = 22; msLbl.color = Color.white;

        // SFX slider
        var sfxSliderGo = GetOrCreateChild(settingsPanel, "SFXSlider");
        var sfxSliderRt = Ensure<RectTransform>(sfxSliderGo);
        sfxSliderRt.anchorMin        = new Vector2(0.5f, 0.5f);
        sfxSliderRt.anchorMax        = new Vector2(0.5f, 0.5f);
        sfxSliderRt.anchoredPosition = new Vector2(80f, 40f);
        sfxSliderRt.sizeDelta        = new Vector2(400f, 40f);
        var sfxSlider = Ensure<Slider>(sfxSliderGo);
        sfxSlider.minValue = 0f; sfxSlider.maxValue = 1f; sfxSlider.value = 0.8f;

        var sfxLblGo = GetOrCreateChild(settingsPanel, "SFXLabel");
        var sfxLblRt = Ensure<RectTransform>(sfxLblGo);
        sfxLblRt.anchorMin        = new Vector2(0.5f, 0.5f);
        sfxLblRt.anchorMax        = new Vector2(0.5f, 0.5f);
        sfxLblRt.anchoredPosition = new Vector2(-220f, 40f);
        sfxLblRt.sizeDelta        = new Vector2(200f, 40f);
        var sfxLbl = Ensure<TextMeshProUGUI>(sfxLblGo);
        sfxLbl.text = "SFX Volume"; sfxLbl.fontSize = 22; sfxLbl.color = Color.white;

        // Quality dropdown
        var qualityGo = GetOrCreateChild(settingsPanel, "QualityDropdown");
        var qualityRt = Ensure<RectTransform>(qualityGo);
        qualityRt.anchorMin        = new Vector2(0.5f, 0.5f);
        qualityRt.anchorMax        = new Vector2(0.5f, 0.5f);
        qualityRt.anchoredPosition = new Vector2(80f, -60f);
        qualityRt.sizeDelta        = new Vector2(400f, 50f);
        var qualityDropdown = Ensure<TMP_Dropdown>(qualityGo);

        var qLblGo = GetOrCreateChild(settingsPanel, "QualityLabel");
        var qLblRt = Ensure<RectTransform>(qLblGo);
        qLblRt.anchorMin        = new Vector2(0.5f, 0.5f);
        qLblRt.anchorMax        = new Vector2(0.5f, 0.5f);
        qLblRt.anchoredPosition = new Vector2(-220f, -60f);
        qLblRt.sizeDelta        = new Vector2(200f, 50f);
        var qLbl = Ensure<TextMeshProUGUI>(qLblGo);
        qLbl.text = "Quality"; qLbl.fontSize = 22; qLbl.color = Color.white;

        // Settings Back button
        var settingsBackGo = GetOrCreateChild(settingsPanel, "BackButton");
        var settingsBackRt = Ensure<RectTransform>(settingsBackGo);
        settingsBackRt.anchorMin        = new Vector2(0.5f, 0.5f);
        settingsBackRt.anchorMax        = new Vector2(0.5f, 0.5f);
        settingsBackRt.anchoredPosition = new Vector2(0f, -180f);
        settingsBackRt.sizeDelta        = new Vector2(200f, 60f);
        Ensure<Image>(settingsBackGo).color = new Color(0.3f, 0.15f, 0.1f);
        var settingsBackBtn = Ensure<Button>(settingsBackGo);
        var sbLblGo = GetOrCreateChild(settingsBackGo, "Label");
        var sbLblRt = Ensure<RectTransform>(sbLblGo);
        sbLblRt.anchorMin = Vector2.zero; sbLblRt.anchorMax = Vector2.one;
        sbLblRt.offsetMin = sbLblRt.offsetMax = Vector2.zero;
        var sbLbl = Ensure<TextMeshProUGUI>(sbLblGo);
        sbLbl.text = "BACK"; sbLbl.fontSize = 24; sbLbl.alignment = TextAlignmentOptions.Center; sbLbl.color = Color.white;

        settingsPanel.SetActive(false);

        // ── How To Play Panel ────────────────────────────────────────────────
        var htpPanel = GetOrCreateChild(canvasGo, "HowToPlayPanel");
        StretchFull(Ensure<RectTransform>(htpPanel));
        Ensure<Image>(htpPanel).color = new Color(0f, 0f, 0f, 0.7f);

        // Slide container
        var slideContainerGo = GetOrCreateChild(htpPanel, "SlideContainer");
        var scRt = Ensure<RectTransform>(slideContainerGo);
        scRt.anchorMin = new Vector2(0.1f, 0.2f);
        scRt.anchorMax = new Vector2(0.9f, 0.85f);
        scRt.offsetMin = scRt.offsetMax = Vector2.zero;
        Ensure<Image>(slideContainerGo).color = new Color(0.1f, 0.1f, 0.12f, 0.85f);

        // 4 slides
        string[] slideTitles = { "Movement", "Abilities", "Match-3 Mechanics", "Bond System" };
        string[] slideDescs  =
        {
            "Use WASD / Arrow Keys to move.\nPress Space to jump.\nClimb vines and ladders to reach new areas.",
            "Z — Pulse Wave: calm nearby animals.\nX — Focus Calm: sustained calming aura.\nC — Emotional Burst: unlock Empty state.\nV — Spirit Assist: Elephant barrier pulse.",
            "When an animal is Overwhelmed, a Match-3 grid opens.\nMatch 3 tiles of the same type to reduce distress.\nGrid size grows as your bond deepens.",
            "Healing animals builds Bond (I–III).\nBond I Deer: stability gain bonus.\nBond I Elephant: expanded Spirit Assist radius.\nBond III unlocks the Spirit Realm act."
        };

        var slideGos = new GameObject[slideTitles.Length];
        for (int i = 0; i < slideTitles.Length; i++)
        {
            var slide   = GetOrCreateChild(slideContainerGo, $"Slide_{i}");
            var slideRt = Ensure<RectTransform>(slide);
            slideRt.anchorMin = Vector2.zero; slideRt.anchorMax = Vector2.one;
            slideRt.offsetMin = slideRt.offsetMax = Vector2.zero;
            Ensure<Image>(slide).color = Color.clear;

            var sTitleGo = GetOrCreateChild(slide, "Title");
            var sTitleRt = Ensure<RectTransform>(sTitleGo);
            sTitleRt.anchorMin        = new Vector2(0.5f, 0.82f);
            sTitleRt.anchorMax        = new Vector2(0.5f, 0.82f);
            sTitleRt.anchoredPosition = Vector2.zero;
            sTitleRt.sizeDelta        = new Vector2(700f, 60f);
            var sTitleTmp = Ensure<TextMeshProUGUI>(sTitleGo);
            sTitleTmp.text = slideTitles[i]; sTitleTmp.fontSize = 34;
            sTitleTmp.alignment = TextAlignmentOptions.Center; sTitleTmp.color = Color.white;

            var sDescGo = GetOrCreateChild(slide, "Description");
            var sDescRt = Ensure<RectTransform>(sDescGo);
            sDescRt.anchorMin = new Vector2(0.05f, 0.05f);
            sDescRt.anchorMax = new Vector2(0.95f, 0.75f);
            sDescRt.offsetMin = sDescRt.offsetMax = Vector2.zero;
            var sDescTmp = Ensure<TextMeshProUGUI>(sDescGo);
            sDescTmp.text = slideDescs[i]; sDescTmp.fontSize = 22;
            sDescTmp.alignment = TextAlignmentOptions.Center;
            sDescTmp.color = new Color(0.85f, 0.85f, 0.85f);
            sDescTmp.enableWordWrapping = true;

            slide.SetActive(i == 0);
            slideGos[i] = slide;
        }

        // Prev button
        var prevBtnGo = GetOrCreateChild(htpPanel, "PrevButton");
        var prevBtnRt = Ensure<RectTransform>(prevBtnGo);
        prevBtnRt.anchorMin        = new Vector2(0.05f, 0.05f);
        prevBtnRt.anchorMax        = new Vector2(0.05f, 0.05f);
        prevBtnRt.anchoredPosition = Vector2.zero;
        prevBtnRt.sizeDelta        = new Vector2(130f, 50f);
        Ensure<Image>(prevBtnGo).color = new Color(0.2f, 0.2f, 0.2f);
        var prevBtn = Ensure<Button>(prevBtnGo);
        var prevLblGo = GetOrCreateChild(prevBtnGo, "Label");
        var prevLblRt = Ensure<RectTransform>(prevLblGo);
        prevLblRt.anchorMin = Vector2.zero; prevLblRt.anchorMax = Vector2.one;
        prevLblRt.offsetMin = prevLblRt.offsetMax = Vector2.zero;
        var prevLbl = Ensure<TextMeshProUGUI>(prevLblGo);
        prevLbl.text = "< PREV"; prevLbl.fontSize = 20; prevLbl.alignment = TextAlignmentOptions.Center; prevLbl.color = Color.white;

        // Next button
        var nextBtnGo = GetOrCreateChild(htpPanel, "NextButton");
        var nextBtnRt = Ensure<RectTransform>(nextBtnGo);
        nextBtnRt.anchorMin        = new Vector2(0.95f, 0.05f);
        nextBtnRt.anchorMax        = new Vector2(0.95f, 0.05f);
        nextBtnRt.anchoredPosition = Vector2.zero;
        nextBtnRt.sizeDelta        = new Vector2(130f, 50f);
        Ensure<Image>(nextBtnGo).color = new Color(0.2f, 0.2f, 0.2f);
        var nextBtn = Ensure<Button>(nextBtnGo);
        var nextLblGo = GetOrCreateChild(nextBtnGo, "Label");
        var nextLblRt = Ensure<RectTransform>(nextLblGo);
        nextLblRt.anchorMin = Vector2.zero; nextLblRt.anchorMax = Vector2.one;
        nextLblRt.offsetMin = nextLblRt.offsetMax = Vector2.zero;
        var nextLbl = Ensure<TextMeshProUGUI>(nextLblGo);
        nextLbl.text = "NEXT >"; nextLbl.fontSize = 20; nextLbl.alignment = TextAlignmentOptions.Center; nextLbl.color = Color.white;

        // HTP Back button
        var htpBackGo = GetOrCreateChild(htpPanel, "BackButton");
        var htpBackRt = Ensure<RectTransform>(htpBackGo);
        htpBackRt.anchorMin        = new Vector2(0.5f, 0.05f);
        htpBackRt.anchorMax        = new Vector2(0.5f, 0.05f);
        htpBackRt.anchoredPosition = Vector2.zero;
        htpBackRt.sizeDelta        = new Vector2(140f, 50f);
        Ensure<Image>(htpBackGo).color = new Color(0.3f, 0.15f, 0.1f);
        var htpBackBtn = Ensure<Button>(htpBackGo);
        var htpBackLblGo = GetOrCreateChild(htpBackGo, "Label");
        var htpBackLblRt = Ensure<RectTransform>(htpBackLblGo);
        htpBackLblRt.anchorMin = Vector2.zero; htpBackLblRt.anchorMax = Vector2.one;
        htpBackLblRt.offsetMin = htpBackLblRt.offsetMax = Vector2.zero;
        var htpBackLbl = Ensure<TextMeshProUGUI>(htpBackLblGo);
        htpBackLbl.text = "BACK"; htpBackLbl.fontSize = 20; htpBackLbl.alignment = TextAlignmentOptions.Center; htpBackLbl.color = Color.white;

        htpPanel.SetActive(false);

        // ── Character Select Panel ──────────────────────────────────────────
        var charPanel = GetOrCreateChild(canvasGo, "CharacterPanel");
        StretchFull(Ensure<RectTransform>(charPanel));
        Ensure<Image>(charPanel).color = new Color(0f, 0f, 0f, 0.78f);

        // "SELECT YOUR CHARACTER" header
        var csTitleGo = GetOrCreateChild(charPanel, "Title");
        var csTitleRt = Ensure<RectTransform>(csTitleGo);
        csTitleRt.anchorMin = new Vector2(0.5f, 0.84f); csTitleRt.anchorMax = new Vector2(0.5f, 0.84f);
        csTitleRt.anchoredPosition = Vector2.zero; csTitleRt.sizeDelta = new Vector2(700f, 70f);
        var csTitleTmp = Ensure<TextMeshProUGUI>(csTitleGo);
        csTitleTmp.text = "SELECT YOUR CHARACTER";
        csTitleTmp.fontSize = 44; csTitleTmp.alignment = TextAlignmentOptions.Center; csTitleTmp.color = Color.white;

        // Portrait square (colour-filled; swapped at runtime when CharacterData has no sprite)
        var csPortraitGo = GetOrCreateChild(charPanel, "Portrait");
        var csPortraitRt = Ensure<RectTransform>(csPortraitGo);
        csPortraitRt.anchorMin = new Vector2(0.5f, 0.5f); csPortraitRt.anchorMax = new Vector2(0.5f, 0.5f);
        csPortraitRt.anchoredPosition = new Vector2(0f, 40f); csPortraitRt.sizeDelta = new Vector2(220f, 220f);
        var csPortraitImg = Ensure<Image>(csPortraitGo);
        csPortraitImg.color = new Color(1f, 0.85f, 0.4f);

        // Character name label
        var csNameGo = GetOrCreateChild(charPanel, "CharName");
        var csNameRt = Ensure<RectTransform>(csNameGo);
        csNameRt.anchorMin = new Vector2(0.5f, 0.5f); csNameRt.anchorMax = new Vector2(0.5f, 0.5f);
        csNameRt.anchoredPosition = new Vector2(0f, -100f); csNameRt.sizeDelta = new Vector2(500f, 55f);
        var csNameTmp = Ensure<TextMeshProUGUI>(csNameGo);
        csNameTmp.text = "CHARACTER"; csNameTmp.fontSize = 34;
        csNameTmp.alignment = TextAlignmentOptions.Center; csNameTmp.color = Color.white;

        // Description text
        var csDescGo = GetOrCreateChild(charPanel, "CharDesc");
        var csDescRt = Ensure<RectTransform>(csDescGo);
        csDescRt.anchorMin = new Vector2(0.22f, 0.20f); csDescRt.anchorMax = new Vector2(0.78f, 0.34f);
        csDescRt.offsetMin = csDescRt.offsetMax = Vector2.zero;
        var csDescTmp = Ensure<TextMeshProUGUI>(csDescGo);
        csDescTmp.text = "Description"; csDescTmp.fontSize = 20;
        csDescTmp.alignment = TextAlignmentOptions.Center;
        csDescTmp.color = new Color(0.82f, 0.82f, 0.82f); csDescTmp.enableWordWrapping = true;

        // ◀ Prev
        var csPrevGo = GetOrCreateChild(charPanel, "PrevButton");
        var csPrevRt = Ensure<RectTransform>(csPrevGo);
        csPrevRt.anchorMin = new Vector2(0.22f, 0.5f); csPrevRt.anchorMax = new Vector2(0.22f, 0.5f);
        csPrevRt.anchoredPosition = new Vector2(0f, 40f); csPrevRt.sizeDelta = new Vector2(80f, 80f);
        Ensure<Image>(csPrevGo).color = new Color(0.18f, 0.32f, 0.18f);
        var csPrevBtn = Ensure<Button>(csPrevGo);
        MakeButtonLabel(csPrevGo, "◀", 36);

        // ▶ Next
        var csNextGo = GetOrCreateChild(charPanel, "NextButton");
        var csNextRt = Ensure<RectTransform>(csNextGo);
        csNextRt.anchorMin = new Vector2(0.78f, 0.5f); csNextRt.anchorMax = new Vector2(0.78f, 0.5f);
        csNextRt.anchoredPosition = new Vector2(0f, 40f); csNextRt.sizeDelta = new Vector2(80f, 80f);
        Ensure<Image>(csNextGo).color = new Color(0.18f, 0.32f, 0.18f);
        var csNextBtn = Ensure<Button>(csNextGo);
        MakeButtonLabel(csNextGo, "▶", 36);

        // PLAY button (confirms selection + loads game scene)
        var csPlayGo = GetOrCreateChild(charPanel, "PlayButton");
        var csPlayRt = Ensure<RectTransform>(csPlayGo);
        csPlayRt.anchorMin = new Vector2(0.5f, 0.07f); csPlayRt.anchorMax = new Vector2(0.5f, 0.07f);
        csPlayRt.anchoredPosition = Vector2.zero; csPlayRt.sizeDelta = new Vector2(280f, 70f);
        Ensure<Image>(csPlayGo).color = new Color(0.14f, 0.48f, 0.14f);
        var csPlayBtn = Ensure<Button>(csPlayGo);
        MakeButtonLabel(csPlayGo, "PLAY", 30);

        // BACK button (→ main panel)
        var csBackGo = GetOrCreateChild(charPanel, "BackButton");
        var csBackRt = Ensure<RectTransform>(csBackGo);
        csBackRt.anchorMin = new Vector2(0.04f, 0.04f); csBackRt.anchorMax = new Vector2(0.04f, 0.04f);
        csBackRt.anchoredPosition = Vector2.zero; csBackRt.sizeDelta = new Vector2(140f, 50f);
        Ensure<Image>(csBackGo).color = new Color(0.30f, 0.14f, 0.08f);
        var csBackBtn = Ensure<Button>(csBackGo);
        MakeButtonLabel(csBackGo, "BACK", 22);

        charPanel.SetActive(false);

        // ── Character Data Assets ────────────────────────────────────────────
        if (!AssetDatabase.IsValidFolder("Assets/Data"))
            AssetDatabase.CreateFolder("Assets", "Data");
        if (!AssetDatabase.IsValidFolder("Assets/Data/Characters"))
            AssetDatabase.CreateFolder("Assets/Data", "Characters");

        var charDefs = new (string n, Color col, float ms, float jf, string d)[]
        {
            ("Echoes", new Color(1.00f, 0.85f, 0.40f), 6f,   12f, "Balanced and steady.\nFeel the wild in harmony."),
            ("Wilder", new Color(0.28f, 0.75f, 0.42f), 8f,   10f, "Swift as the wind.\nOutrun any storm."),
            ("Serene", new Color(0.52f, 0.72f, 1.00f), 4.5f, 14f, "Calm and precise.\nRise above the chaos."),
        };
        var charAssets = new CharacterData[charDefs.Length];
        for (int i = 0; i < charDefs.Length; i++)
        {
            string ap = $"Assets/Data/Characters/{charDefs[i].n}.asset";
            var cd = AssetDatabase.LoadAssetAtPath<CharacterData>(ap);
            if (cd == null)
            {
                cd = ScriptableObject.CreateInstance<CharacterData>();
                AssetDatabase.CreateAsset(cd, ap);
            }
            cd.characterName  = charDefs[i].n;
            cd.characterColor = charDefs[i].col;
            cd.moveSpeed      = charDefs[i].ms;
            cd.jumpForce      = charDefs[i].jf;
            cd.description    = charDefs[i].d;
            EditorUtility.SetDirty(cd);
            charAssets[i] = cd;
        }
        AssetDatabase.SaveAssets();

        // ── Menu Managers ────────────────────────────────────────────────────
        var managers = GetOrCreate("MenuManagers");
        var mmgr  = Ensure<MainMenuManager>(managers);
        var smgr  = Ensure<SettingsManager>(managers);
        var htmgr = Ensure<HowToPlayManager>(managers);
        var csmgr = Ensure<CharacterSelectManager>(managers);

        // Wire MainMenuManager
        var mmgrSo = new SerializedObject(mmgr);
        mmgrSo.FindProperty("_mainPanel").objectReferenceValue              = mainPanel;
        mmgrSo.FindProperty("_settingsPanel").objectReferenceValue          = settingsPanel;
        mmgrSo.FindProperty("_howToPlayPanel").objectReferenceValue         = htpPanel;
        mmgrSo.FindProperty("_characterPanel").objectReferenceValue         = charPanel;
        mmgrSo.FindProperty("_characterSelectManager").objectReferenceValue = csmgr;
        mmgrSo.FindProperty("_howToPlayBackButton").objectReferenceValue    = htpBackBtn;
        mmgrSo.ApplyModifiedProperties();

        // Wire SettingsManager
        var smgrSo = new SerializedObject(smgr);
        smgrSo.FindProperty("_musicSlider").objectReferenceValue     = musicSlider;
        smgrSo.FindProperty("_sfxSlider").objectReferenceValue       = sfxSlider;
        smgrSo.FindProperty("_qualityDropdown").objectReferenceValue = qualityDropdown;
        smgrSo.ApplyModifiedProperties();

        // Wire HowToPlayManager
        var htmgrSo = new SerializedObject(htmgr);
        var slidesProp = htmgrSo.FindProperty("_slides");
        slidesProp.arraySize = slideGos.Length;
        for (int i = 0; i < slideGos.Length; i++)
            slidesProp.GetArrayElementAtIndex(i).objectReferenceValue = slideGos[i];
        htmgrSo.FindProperty("_prevButton").objectReferenceValue = prevBtn;
        htmgrSo.FindProperty("_nextButton").objectReferenceValue = nextBtn;
        htmgrSo.ApplyModifiedProperties();

        // Wire CharacterSelectManager
        var csmgrSo = new SerializedObject(csmgr);
        var csProp = csmgrSo.FindProperty("_characters");
        csProp.arraySize = charAssets.Length;
        for (int i = 0; i < charAssets.Length; i++)
            csProp.GetArrayElementAtIndex(i).objectReferenceValue = charAssets[i];
        csmgrSo.FindProperty("_portrait").objectReferenceValue = csPortraitImg;
        csmgrSo.FindProperty("_nameText").objectReferenceValue = csNameTmp;
        csmgrSo.FindProperty("_descText").objectReferenceValue = csDescTmp;
        csmgrSo.ApplyModifiedProperties();

        // Wire button onClick — must use AddPersistentListener so calls are serialised into the scene file.
        // AddListener() only adds runtime-only calls that are discarded on save.
        UnityEventTools.AddPersistentListener(mainBtns[0].onClick, new UnityAction(mmgr.ShowCharacterSelect));
        UnityEventTools.AddPersistentListener(mainBtns[1].onClick, new UnityAction(mmgr.ShowSettings));
        UnityEventTools.AddPersistentListener(mainBtns[2].onClick, new UnityAction(mmgr.ShowHowToPlay));
        UnityEventTools.AddPersistentListener(settingsBackBtn.onClick, new UnityAction(mmgr.ShowMain));
        UnityEventTools.AddPersistentListener(csPrevBtn.onClick, new UnityAction(csmgr.SelectPrev));
        UnityEventTools.AddPersistentListener(csNextBtn.onClick, new UnityAction(csmgr.SelectNext));
        UnityEventTools.AddPersistentListener(csPlayBtn.onClick, new UnityAction(mmgr.StartGame));
        UnityEventTools.AddPersistentListener(csBackBtn.onClick, new UnityAction(mmgr.ShowMain));

        EditorSceneManager.SaveScene(scene, scenePath);
        EditorUtility.SetDirty(managers);

        Debug.Log("[EotW] MainMenu scene created at " + scenePath +
                  "\nTODO: Add both scenes to Build Settings (MainMenu=0, SampleScene=1).");
    }
}
#endif