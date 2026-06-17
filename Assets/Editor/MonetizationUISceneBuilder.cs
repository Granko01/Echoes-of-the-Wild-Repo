using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class MonetizationUISceneBuilder
{
    const string PrefabDir = "Assets/Prefabs/UI";

    [MenuItem("Punch/Build Monetization UI Prefabs")]
    public static void BuildPrefabs()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        if (!AssetDatabase.IsValidFolder(PrefabDir))
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");

        // Temporary root — never saved, just a workspace
        var tempRoot = new GameObject("_TempBuildRoot", typeof(RectTransform));

        // Build item prefabs first (panels reference these)
        var shopItemPrefab    = BuildAndSaveItemPrefab(tempRoot.transform, CreateShopItemTemplate,    "ShopItemUI");
        var passCardPrefab    = BuildAndSaveItemPrefab(tempRoot.transform, CreatePassCardTemplate,    "PassLevelCard");
        var wardrobeItemPrefab = BuildAndSaveItemPrefab(tempRoot.transform, CreateWardrobeItemTemplate, "WardrobeItem");

        // Build each panel, wire the item prefab reference, save as prefab
        BuildAndSavePanel(tempRoot.transform, "ShopPanel",          (host, content) => BuildShopPanel(host, content, shopItemPrefab));
        BuildAndSavePanel(tempRoot.transform, "DailyLoginPopup",    BuildDailyLoginPopup);
        BuildAndSavePanel(tempRoot.transform, "MissionsPanel",      BuildMissionsPanel);
        BuildAndSavePanel(tempRoot.transform, "PassPanel",          (host, content) => BuildPassPanel(host, content, passCardPrefab));
        BuildAndSavePanel(tempRoot.transform, "WardrobePanel",      (host, content) => BuildWardrobePanel(host, content, wardrobeItemPrefab));
        BuildAndSavePanel(tempRoot.transform, "VIPPanel",           BuildVIPPanel);
        BuildAndSavePanel(tempRoot.transform, "ChestOpeningPanel",  BuildChestOpeningPanel);

        Object.DestroyImmediate(tempRoot);
        AssetDatabase.Refresh();

        Debug.Log($"[Punch] 7 panel prefabs + 3 item prefabs saved to {PrefabDir}/. Drag them onto your MainMenu Canvas.");
    }

    // ── Prefab save helpers ───────────────────────────────────────────────────

    delegate void PanelBuilder(GameObject scriptHost, GameObject contentRoot);

    static void BuildAndSavePanel(Transform tempRoot, string name, PanelBuilder builder)
    {
        // Root stays active so Awake/OnEnable fire — singletons + event subscriptions work.
        // Content child holds all visuals and starts inactive — toggled by Open()/Close().
        var go = CreateFullscreenRect(tempRoot, name);

        var content = CreateFullscreenRect(go.transform, "Content");
        content.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.13f, 0.95f);

        builder(go, content);
        content.SetActive(false);

        PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabDir}/{name}.prefab");
        Object.DestroyImmediate(go);
    }

    delegate GameObject TemplateBuilder(Transform parent);

    static GameObject BuildAndSaveItemPrefab(Transform tempRoot, TemplateBuilder builder, string name)
    {
        var go = builder(tempRoot);
        go.SetActive(true);
        var saved = PrefabUtility.SaveAsPrefabAsset(go, $"{PrefabDir}/{name}.prefab");
        Object.DestroyImmediate(go);
        return saved;
    }

    // ── Shop Panel ────────────────────────────────────────────────────────────

    static void BuildShopPanel(GameObject scriptHost, GameObject contentRoot, GameObject itemPrefabAsset)
    {
        var shopUI = scriptHost.AddComponent<ShopUI>();

        CreateHeader(contentRoot.transform, "SHOP");

        var tabBar = CreateHorizontalGroup(contentRoot.transform, "TabBar", 60);
        string[] tabNames = { "Featured", "Passes", "Gems", "Costumes", "Companions", "Boosters", "Premium" };
        var tabButtons = new Button[tabNames.Length];
        for (int i = 0; i < tabNames.Length; i++)
            tabButtons[i] = CreateTabButton(tabBar.transform, tabNames[i]);

        var scroll = CreateScrollArea(contentRoot.transform, "ItemScroll", -130, -200);
        var grid = scroll.content.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(320, 400);
        grid.spacing = new Vector2(20, 20);
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;

        var currBar = CreateHorizontalGroup(contentRoot.transform, "CurrencyBar", 50);
        SetAnchors(currBar.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -60), new Vector2(0, 50));
        var gemsText = CreateTMPLabel(currBar.transform, "GemsText", "0 Gems");
        var coinsText = CreateTMPLabel(currBar.transform, "CoinsText", "0 Coins");

        var closeBtn = CreateButton(contentRoot.transform, "CloseBtn", "X");
        SetAnchors(closeBtn.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-50, -50), new Vector2(80, 80));

        var so = new SerializedObject(shopUI);
        so.FindProperty("_shopRoot").objectReferenceValue = contentRoot;
        SetButtonArray(so, "_tabButtons", tabButtons);
        so.FindProperty("_itemContainer").objectReferenceValue = scroll.content;
        so.FindProperty("_itemPrefab").objectReferenceValue = itemPrefabAsset.GetComponent<ShopItemUI>();
        so.FindProperty("_gemsText").objectReferenceValue = gemsText;
        so.FindProperty("_coinsText").objectReferenceValue = coinsText;
        so.FindProperty("_closeButton").objectReferenceValue = closeBtn;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static GameObject CreateShopItemTemplate(Transform parent)
    {
        var go = CreateChildWithRect(parent, "ShopItemUI");
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(320, 400);
        go.AddComponent<Image>().color = new Color(0.18f, 0.18f, 0.22f);

        var itemUI = go.AddComponent<ShopItemUI>();

        var icon = CreateChildWithRect(go.transform, "Icon");
        SetAnchors(icon.GetComponent<RectTransform>(), new Vector2(0.1f, 0.5f), new Vector2(0.9f, 0.95f));
        var iconImg = icon.AddComponent<Image>();

        var nameText = CreateTMPLabel(go.transform, "NameText", "Item Name");
        SetAnchors(nameText.GetComponent<RectTransform>(), new Vector2(0, 0.35f), new Vector2(1, 0.5f));
        var descText = CreateTMPLabel(go.transform, "DescText", "Description");
        SetAnchors(descText.GetComponent<RectTransform>(), new Vector2(0, 0.2f), new Vector2(1, 0.35f));
        descText.fontSize = 18;

        var priceText = CreateTMPLabel(go.transform, "PriceText", "$0.99");
        SetAnchors(priceText.GetComponent<RectTransform>(), new Vector2(0, 0.05f), new Vector2(0.5f, 0.2f));

        var buyBtn = CreateButton(go.transform, "BuyBtn", "BUY");
        SetAnchors(buyBtn.GetComponent<RectTransform>(), new Vector2(0.5f, 0.05f), new Vector2(0.95f, 0.2f));

        var ownedBadge = CreateChildWithRect(go.transform, "OwnedBadge");
        ownedBadge.AddComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
        CreateTMPLabel(ownedBadge.transform, "Text", "OWNED");
        SetAnchors(ownedBadge.GetComponent<RectTransform>(), new Vector2(0.6f, 0.85f), new Vector2(1, 1));
        ownedBadge.SetActive(false);

        var cantAfford = CreateChildWithRect(go.transform, "CantAffordOverlay");
        cantAfford.AddComponent<Image>().color = new Color(0, 0, 0, 0.5f);
        SetAnchors(cantAfford.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        cantAfford.SetActive(false);

        var itemSO = new SerializedObject(itemUI);
        itemSO.FindProperty("_icon").objectReferenceValue = iconImg;
        itemSO.FindProperty("_nameText").objectReferenceValue = nameText;
        itemSO.FindProperty("_descriptionText").objectReferenceValue = descText;
        itemSO.FindProperty("_priceText").objectReferenceValue = priceText;
        itemSO.FindProperty("_buyButton").objectReferenceValue = buyBtn;
        itemSO.FindProperty("_ownedBadge").objectReferenceValue = ownedBadge;
        itemSO.FindProperty("_cantAffordOverlay").objectReferenceValue = cantAfford;
        itemSO.ApplyModifiedPropertiesWithoutUndo();

        return go;
    }

    // ── Daily Login Popup ─────────────────────────────────────────────────────

    static void BuildDailyLoginPopup(GameObject scriptHost, GameObject contentRoot)
    {
        contentRoot.GetComponent<Image>().color = new Color(0, 0, 0, 0.85f);
        var ui = scriptHost.AddComponent<DailyLoginUI>();

        var card = CreateChildWithRect(contentRoot.transform, "Card");
        card.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f);
        SetAnchors(card.GetComponent<RectTransform>(), new Vector2(0.1f, 0.25f), new Vector2(0.9f, 0.75f));

        var titleText = CreateTMPLabel(card.transform, "Title", "Daily Login");
        SetAnchors(titleText.GetComponent<RectTransform>(), new Vector2(0, 0.85f), new Vector2(1, 1));
        titleText.fontSize = 36;
        titleText.alignment = TextAlignmentOptions.Center;

        var dayText = CreateTMPLabel(card.transform, "DayText", "Day 1");
        SetAnchors(dayText.GetComponent<RectTransform>(), new Vector2(0, 0.65f), new Vector2(1, 0.85f));
        dayText.fontSize = 48;
        dayText.alignment = TextAlignmentOptions.Center;

        var rewardText = CreateTMPLabel(card.transform, "RewardText", "200 Coins");
        SetAnchors(rewardText.GetComponent<RectTransform>(), new Vector2(0, 0.45f), new Vector2(1, 0.65f));
        rewardText.fontSize = 30;
        rewardText.alignment = TextAlignmentOptions.Center;

        var dayBar = CreateHorizontalGroup(card.transform, "DayIndicators", 40);
        SetAnchors(dayBar.GetComponent<RectTransform>(), new Vector2(0.05f, 0.3f), new Vector2(0.95f, 0.45f));
        var dayIndicators = new Image[7];
        for (int i = 0; i < 7; i++)
        {
            var dot = CreateChildWithRect(dayBar.transform, $"Day{i + 1}");
            dayIndicators[i] = dot.AddComponent<Image>();
            dayIndicators[i].color = new Color(1, 1, 1, 0.3f);
            var le = dot.AddComponent<LayoutElement>();
            le.preferredWidth = 40;
            le.preferredHeight = 40;
        }

        var claimBtn = CreateButton(card.transform, "ClaimBtn", "CLAIM");
        SetAnchors(claimBtn.GetComponent<RectTransform>(), new Vector2(0.2f, 0.05f), new Vector2(0.8f, 0.25f));

        var so = new SerializedObject(ui);
        so.FindProperty("_popupRoot").objectReferenceValue = contentRoot;
        so.FindProperty("_dayText").objectReferenceValue = dayText;
        so.FindProperty("_rewardText").objectReferenceValue = rewardText;
        so.FindProperty("_claimButton").objectReferenceValue = claimBtn;
        SetImageArray(so, "_dayIndicators", dayIndicators);
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ── Missions Panel ────────────────────────────────────────────────────────

    static void BuildMissionsPanel(GameObject scriptHost, GameObject contentRoot)
    {
        var ui = scriptHost.AddComponent<MissionUI>();
        CreateHeader(contentRoot.transform, "MISSIONS");

        var tabBar = CreateHorizontalGroup(contentRoot.transform, "TabBar", 60);
        SetAnchors(tabBar.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), new Vector2(0, 60));
        var dailyTab  = CreateTabButton(tabBar.transform, "Daily");
        var weeklyTab = CreateTabButton(tabBar.transform, "Weekly");

        var dailyContent = CreateChildWithRect(contentRoot.transform, "DailyContent");
        SetAnchors(dailyContent.GetComponent<RectTransform>(), new Vector2(0, 0.05f), new Vector2(1, 0.85f));
        var dailyNames    = new TextMeshProUGUI[4];
        var dailyFills    = new Image[4];
        var dailyProgress = new TextMeshProUGUI[4];
        var dailyClaims   = new Button[4];
        for (int i = 0; i < 4; i++)
            CreateMissionRow(dailyContent.transform, $"DailyMission{i}", i, 4,
                out dailyNames[i], out dailyFills[i], out dailyProgress[i], out dailyClaims[i]);

        var dailyBonusBtn = CreateButton(dailyContent.transform, "DailyBonusBtn", "Claim Bonus");
        SetAnchors(dailyBonusBtn.GetComponent<RectTransform>(), new Vector2(0.1f, 0), new Vector2(0.6f, 0.08f));
        var dailyBonusText = CreateTMPLabel(dailyContent.transform, "DailyBonusText", "Bonus: 50 Gems");
        SetAnchors(dailyBonusText.GetComponent<RectTransform>(), new Vector2(0.6f, 0), new Vector2(1, 0.08f));

        var weeklyContent = CreateChildWithRect(contentRoot.transform, "WeeklyContent");
        SetAnchors(weeklyContent.GetComponent<RectTransform>(), new Vector2(0, 0.05f), new Vector2(1, 0.85f));
        weeklyContent.SetActive(false);
        var weeklyNames    = new TextMeshProUGUI[4];
        var weeklyFills    = new Image[4];
        var weeklyProgress = new TextMeshProUGUI[4];
        var weeklyClaims   = new Button[4];
        for (int i = 0; i < 4; i++)
            CreateMissionRow(weeklyContent.transform, $"WeeklyMission{i}", i, 4,
                out weeklyNames[i], out weeklyFills[i], out weeklyProgress[i], out weeklyClaims[i]);

        var weeklyBonusBtn = CreateButton(weeklyContent.transform, "WeeklyBonusBtn", "Claim Bonus");
        SetAnchors(weeklyBonusBtn.GetComponent<RectTransform>(), new Vector2(0.1f, 0), new Vector2(0.6f, 0.08f));
        var weeklyBonusText = CreateTMPLabel(weeklyContent.transform, "WeeklyBonusText", "Bonus: Legendary Chest");
        SetAnchors(weeklyBonusText.GetComponent<RectTransform>(), new Vector2(0.6f, 0), new Vector2(1, 0.08f));

        var closeBtn = CreateButton(contentRoot.transform, "CloseBtn", "X");
        SetAnchors(closeBtn.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-50, -50), new Vector2(80, 80));

        var so = new SerializedObject(ui);
        so.FindProperty("_panelRoot").objectReferenceValue = contentRoot;
        so.FindProperty("_dailyTabButton").objectReferenceValue = dailyTab;
        so.FindProperty("_weeklyTabButton").objectReferenceValue = weeklyTab;
        so.FindProperty("_dailyContent").objectReferenceValue = dailyContent;
        so.FindProperty("_weeklyContent").objectReferenceValue = weeklyContent;
        SetTMPArray(so, "_dailyNameTexts", dailyNames);
        SetImageArray(so, "_dailyProgressFills", dailyFills);
        SetTMPArray(so, "_dailyProgressTexts", dailyProgress);
        SetButtonArray(so, "_dailyClaimButtons", dailyClaims);
        so.FindProperty("_dailyBonusButton").objectReferenceValue = dailyBonusBtn;
        so.FindProperty("_dailyBonusText").objectReferenceValue = dailyBonusText;
        SetTMPArray(so, "_weeklyNameTexts", weeklyNames);
        SetImageArray(so, "_weeklyProgressFills", weeklyFills);
        SetTMPArray(so, "_weeklyProgressTexts", weeklyProgress);
        SetButtonArray(so, "_weeklyClaimButtons", weeklyClaims);
        so.FindProperty("_weeklyBonusButton").objectReferenceValue = weeklyBonusBtn;
        so.FindProperty("_weeklyBonusText").objectReferenceValue = weeklyBonusText;
        so.FindProperty("_closeButton").objectReferenceValue = closeBtn;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void CreateMissionRow(Transform parent, string name, int index, int total,
        out TextMeshProUGUI nameText, out Image fill, out TextMeshProUGUI progressText, out Button claimBtn)
    {
        float rowH = 1f / (total + 1);
        float top  = 1f - index * rowH - 0.02f;

        var row = CreateChildWithRect(parent, name);
        SetAnchors(row.GetComponent<RectTransform>(), new Vector2(0.02f, top - rowH + 0.02f), new Vector2(0.98f, top));
        row.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f);

        nameText = CreateTMPLabel(row.transform, "Name", "Mission");
        SetAnchors(nameText.GetComponent<RectTransform>(), new Vector2(0.02f, 0.5f), new Vector2(0.6f, 1));
        nameText.fontSize = 22;

        var barBg = CreateChildWithRect(row.transform, "BarBg");
        SetAnchors(barBg.GetComponent<RectTransform>(), new Vector2(0.02f, 0.1f), new Vector2(0.6f, 0.45f));
        barBg.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.12f);

        var barFill = CreateChildWithRect(barBg.transform, "Fill");
        SetAnchors(barFill.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        fill = barFill.AddComponent<Image>();
        fill.color = new Color(0.3f, 0.8f, 0.3f);
        fill.type = Image.Type.Filled;
        fill.fillMethod = Image.FillMethod.Horizontal;
        fill.fillAmount = 0f;

        progressText = CreateTMPLabel(row.transform, "Progress", "0/3");
        SetAnchors(progressText.GetComponent<RectTransform>(), new Vector2(0.62f, 0.1f), new Vector2(0.78f, 0.9f));
        progressText.fontSize = 22;

        claimBtn = CreateButton(row.transform, "ClaimBtn", "Claim");
        SetAnchors(claimBtn.GetComponent<RectTransform>(), new Vector2(0.8f, 0.1f), new Vector2(0.98f, 0.9f));
    }

    // ── Pass Panel ────────────────────────────────────────────────────────────

    static void BuildPassPanel(GameObject scriptHost, GameObject contentRoot, GameObject cardPrefabAsset)
    {
        var ui = scriptHost.AddComponent<PassUI>();
        CreateHeader(contentRoot.transform, "MEMORY CHRONICLE");

        var levelText = CreateTMPLabel(contentRoot.transform, "LevelText", "Level 0/50");
        SetAnchors(levelText.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(0.5f, 1), new Vector2(20, -110), new Vector2(0, 40));
        levelText.fontSize = 28;

        var statusText = CreateTMPLabel(contentRoot.transform, "StatusText", "FREE");
        SetAnchors(statusText.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(1, 1), new Vector2(0, -110), new Vector2(-20, 40));
        statusText.fontSize = 28;
        statusText.alignment = TextAlignmentOptions.Right;

        var xpBarBg = CreateChildWithRect(contentRoot.transform, "XPBarBg");
        SetAnchors(xpBarBg.GetComponent<RectTransform>(), new Vector2(0.02f, 1), new Vector2(0.98f, 1), new Vector2(0, -160), new Vector2(0, 20));
        xpBarBg.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.12f);
        var xpFill = CreateChildWithRect(xpBarBg.transform, "XPFill");
        SetAnchors(xpFill.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        var xpFillImg = xpFill.AddComponent<Image>();
        xpFillImg.color = new Color(0.2f, 0.6f, 1f);
        xpFillImg.type = Image.Type.Filled;
        xpFillImg.fillMethod = Image.FillMethod.Horizontal;

        var scrollGo = CreateChildWithRect(contentRoot.transform, "TrackScroll");
        SetAnchors(scrollGo.GetComponent<RectTransform>(), new Vector2(0, 0.05f), new Vector2(1, 0.85f));
        var scrollRect = scrollGo.AddComponent<ScrollRect>();
        scrollRect.horizontal = true;
        scrollRect.vertical = false;
        scrollGo.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.1f);
        scrollGo.AddComponent<Mask>().showMaskGraphic = true;

        var trackContent = CreateChildWithRect(scrollGo.transform, "TrackContent");
        var trackContentRT = trackContent.GetComponent<RectTransform>();
        trackContentRT.anchorMin = Vector2.zero;
        trackContentRT.anchorMax = new Vector2(0, 1);
        trackContentRT.pivot = new Vector2(0, 0.5f);
        var hlg = trackContent.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        var csf = trackContent.AddComponent<ContentSizeFitter>();
        csf.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        scrollRect.content = trackContentRT;

        var closeBtn = CreateButton(contentRoot.transform, "CloseBtn", "X");
        SetAnchors(closeBtn.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-50, -50), new Vector2(80, 80));

        var so = new SerializedObject(ui);
        so.FindProperty("_panelRoot").objectReferenceValue = contentRoot;
        so.FindProperty("_levelText").objectReferenceValue = levelText;
        so.FindProperty("_xpFill").objectReferenceValue = xpFillImg;
        so.FindProperty("_statusText").objectReferenceValue = statusText;
        so.FindProperty("_trackScroll").objectReferenceValue = scrollRect;
        so.FindProperty("_trackContent").objectReferenceValue = trackContentRT;
        so.FindProperty("_cardPrefab").objectReferenceValue = cardPrefabAsset.GetComponent<PassLevelCardUI>();
        so.FindProperty("_closeButton").objectReferenceValue = closeBtn;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static GameObject CreatePassCardTemplate(Transform parent)
    {
        var go = CreateChildWithRect(parent, "PassLevelCard");
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(160, 400);
        go.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.2f);
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = 160;

        var cardUI = go.AddComponent<PassLevelCardUI>();

        var levelText = CreateTMPLabel(go.transform, "LevelText", "1");
        SetAnchors(levelText.GetComponent<RectTransform>(), new Vector2(0, 0.9f), new Vector2(1, 1));
        levelText.alignment = TextAlignmentOptions.Center;
        levelText.fontSize = 24;

        var freeArea = CreateChildWithRect(go.transform, "FreeReward");
        SetAnchors(freeArea.GetComponent<RectTransform>(), new Vector2(0.05f, 0.5f), new Vector2(0.95f, 0.88f));
        freeArea.AddComponent<Image>().color = new Color(0.2f, 0.25f, 0.2f);

        var freeIcon = CreateChildWithRect(freeArea.transform, "Icon");
        SetAnchors(freeIcon.GetComponent<RectTransform>(), new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.95f));
        var freeIconImg = freeIcon.AddComponent<Image>();

        var freeText = CreateTMPLabel(freeArea.transform, "RewardText", "10 Gems");
        SetAnchors(freeText.GetComponent<RectTransform>(), new Vector2(0, 0.02f), new Vector2(1, 0.38f));
        freeText.fontSize = 16;
        freeText.alignment = TextAlignmentOptions.Center;

        var claimFreeBtn = CreateButton(freeArea.transform, "ClaimFree", "Claim");
        SetAnchors(claimFreeBtn.GetComponent<RectTransform>(), new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.25f));

        var freeClaimedBadge = CreateChildWithRect(freeArea.transform, "ClaimedBadge");
        freeClaimedBadge.AddComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
        SetAnchors(freeClaimedBadge.GetComponent<RectTransform>(), new Vector2(0.1f, 0.02f), new Vector2(0.9f, 0.25f));
        CreateTMPLabel(freeClaimedBadge.transform, "Text", "Done").alignment = TextAlignmentOptions.Center;
        freeClaimedBadge.SetActive(false);

        var premArea = CreateChildWithRect(go.transform, "PremiumReward");
        SetAnchors(premArea.GetComponent<RectTransform>(), new Vector2(0.05f, 0.08f), new Vector2(0.95f, 0.48f));
        premArea.AddComponent<Image>().color = new Color(0.3f, 0.2f, 0.25f);

        var premIcon = CreateChildWithRect(premArea.transform, "Icon");
        SetAnchors(premIcon.GetComponent<RectTransform>(), new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.95f));
        var premIconImg = premIcon.AddComponent<Image>();

        var premText = CreateTMPLabel(premArea.transform, "RewardText", "20 Gems");
        SetAnchors(premText.GetComponent<RectTransform>(), new Vector2(0, 0.02f), new Vector2(1, 0.38f));
        premText.fontSize = 16;
        premText.alignment = TextAlignmentOptions.Center;

        var claimPremBtn = CreateButton(premArea.transform, "ClaimPremium", "Claim");
        SetAnchors(claimPremBtn.GetComponent<RectTransform>(), new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.25f));

        var premClaimedBadge = CreateChildWithRect(premArea.transform, "ClaimedBadge");
        premClaimedBadge.AddComponent<Image>().color = new Color(0.2f, 0.8f, 0.2f, 0.8f);
        SetAnchors(premClaimedBadge.GetComponent<RectTransform>(), new Vector2(0.1f, 0.02f), new Vector2(0.9f, 0.25f));
        CreateTMPLabel(premClaimedBadge.transform, "Text", "Done").alignment = TextAlignmentOptions.Center;
        premClaimedBadge.SetActive(false);

        var locked = CreateChildWithRect(go.transform, "LockedOverlay");
        SetAnchors(locked.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        locked.AddComponent<Image>().color = new Color(0, 0, 0, 0.6f);
        CreateTMPLabel(locked.transform, "LockText", "LOCKED").alignment = TextAlignmentOptions.Center;
        locked.SetActive(false);

        var cso = new SerializedObject(cardUI);
        cso.FindProperty("_levelText").objectReferenceValue = levelText;
        cso.FindProperty("_freeRewardIcon").objectReferenceValue = freeIconImg;
        cso.FindProperty("_freeRewardText").objectReferenceValue = freeText;
        cso.FindProperty("_claimFreeButton").objectReferenceValue = claimFreeBtn;
        cso.FindProperty("_freeClaimedBadge").objectReferenceValue = freeClaimedBadge;
        cso.FindProperty("_premiumRewardIcon").objectReferenceValue = premIconImg;
        cso.FindProperty("_premiumRewardText").objectReferenceValue = premText;
        cso.FindProperty("_claimPremiumButton").objectReferenceValue = claimPremBtn;
        cso.FindProperty("_premiumClaimedBadge").objectReferenceValue = premClaimedBadge;
        cso.FindProperty("_lockedOverlay").objectReferenceValue = locked;
        cso.ApplyModifiedPropertiesWithoutUndo();

        return go;
    }

    // ── Wardrobe Panel ────────────────────────────────────────────────────────

    static void BuildWardrobePanel(GameObject scriptHost, GameObject contentRoot, GameObject itemPrefabAsset)
    {
        var ui = scriptHost.AddComponent<WardrobeUI>();
        CreateHeader(contentRoot.transform, "WARDROBE");

        var tabBar = CreateHorizontalGroup(contentRoot.transform, "TabBar", 60);
        SetAnchors(tabBar.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -80), new Vector2(0, 60));
        var costumesTab = CreateTabButton(tabBar.transform, "Costumes");
        var skinsTab    = CreateTabButton(tabBar.transform, "Companion Skins");

        var scroll = CreateScrollArea(contentRoot.transform, "GridScroll", -150, -60);
        var grid = scroll.content.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(240, 320);
        grid.spacing = new Vector2(20, 20);
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;

        var closeBtn = CreateButton(contentRoot.transform, "CloseBtn", "X");
        SetAnchors(closeBtn.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-50, -50), new Vector2(80, 80));

        var so = new SerializedObject(ui);
        so.FindProperty("_panelRoot").objectReferenceValue = contentRoot;
        so.FindProperty("_costumesTabButton").objectReferenceValue = costumesTab;
        so.FindProperty("_skinsTabButton").objectReferenceValue = skinsTab;
        so.FindProperty("_gridContent").objectReferenceValue = scroll.content;
        so.FindProperty("_itemPrefab").objectReferenceValue = itemPrefabAsset.GetComponent<WardrobeItemUI>();
        so.FindProperty("_closeButton").objectReferenceValue = closeBtn;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static GameObject CreateWardrobeItemTemplate(Transform parent)
    {
        var go = CreateChildWithRect(parent, "WardrobeItem");
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(240, 320);
        go.AddComponent<Image>().color = new Color(0.18f, 0.18f, 0.22f);

        var itemUI = go.AddComponent<WardrobeItemUI>();

        var preview = CreateChildWithRect(go.transform, "Preview");
        SetAnchors(preview.GetComponent<RectTransform>(), new Vector2(0.1f, 0.4f), new Vector2(0.9f, 0.95f));
        var previewImg = preview.AddComponent<Image>();

        var nameText = CreateTMPLabel(go.transform, "NameText", "Costume");
        SetAnchors(nameText.GetComponent<RectTransform>(), new Vector2(0, 0.25f), new Vector2(1, 0.4f));
        nameText.alignment = TextAlignmentOptions.Center;

        var tierText = CreateTMPLabel(go.transform, "TierText", "Rare");
        SetAnchors(tierText.GetComponent<RectTransform>(), new Vector2(0, 0.15f), new Vector2(1, 0.25f));
        tierText.fontSize = 20;
        tierText.alignment = TextAlignmentOptions.Center;

        var equipBtn = CreateButton(go.transform, "EquipBtn", "Equip");
        SetAnchors(equipBtn.GetComponent<RectTransform>(), new Vector2(0.1f, 0.02f), new Vector2(0.9f, 0.14f));

        var equippedBadge = CreateChildWithRect(go.transform, "EquippedBadge");
        equippedBadge.AddComponent<Image>().color = new Color(0.3f, 0.7f, 1f, 0.8f);
        SetAnchors(equippedBadge.GetComponent<RectTransform>(), new Vector2(0.6f, 0.88f), new Vector2(1, 1));
        CreateTMPLabel(equippedBadge.transform, "Text", "Equipped").fontSize = 14;
        equippedBadge.SetActive(false);

        var locked = CreateChildWithRect(go.transform, "LockedOverlay");
        locked.AddComponent<Image>().color = new Color(0, 0, 0, 0.6f);
        SetAnchors(locked.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        CreateTMPLabel(locked.transform, "LockText", "LOCKED").alignment = TextAlignmentOptions.Center;
        locked.SetActive(false);

        var iso = new SerializedObject(itemUI);
        iso.FindProperty("_previewImage").objectReferenceValue = previewImg;
        iso.FindProperty("_nameText").objectReferenceValue = nameText;
        iso.FindProperty("_tierText").objectReferenceValue = tierText;
        iso.FindProperty("_equipButton").objectReferenceValue = equipBtn;
        iso.FindProperty("_equippedBadge").objectReferenceValue = equippedBadge;
        iso.FindProperty("_lockedOverlay").objectReferenceValue = locked;
        iso.ApplyModifiedPropertiesWithoutUndo();

        return go;
    }

    // ── VIP Panel ─────────────────────────────────────────────────────────────

    static void BuildVIPPanel(GameObject scriptHost, GameObject contentRoot)
    {
        var ui = scriptHost.AddComponent<VIPPanelUI>();
        CreateHeader(contentRoot.transform, "VIP MEMBERSHIP");

        var statusText = CreateTMPLabel(contentRoot.transform, "StatusText", "VIP Inactive");
        SetAnchors(statusText.GetComponent<RectTransform>(), new Vector2(0, 0.75f), new Vector2(1, 0.88f));
        statusText.fontSize = 36;
        statusText.alignment = TextAlignmentOptions.Center;

        var benefitsText = CreateTMPLabel(contentRoot.transform, "BenefitsText",
            "- 3x Pass XP\n- Faster Energy Recovery\n- Daily: 20 Gems + 200 Coins + 10 Energy\n- Exclusive Shop Deals");
        SetAnchors(benefitsText.GetComponent<RectTransform>(), new Vector2(0.05f, 0.3f), new Vector2(0.95f, 0.72f));
        benefitsText.fontSize = 26;

        var claimBtn = CreateButton(contentRoot.transform, "ClaimDailyBtn", "Claim Daily Reward");
        SetAnchors(claimBtn.GetComponent<RectTransform>(), new Vector2(0.15f, 0.15f), new Vector2(0.85f, 0.27f));

        var dailyRewardText = CreateTMPLabel(contentRoot.transform, "DailyRewardText", "20 Gems + 200 Coins + 10 Energy");
        SetAnchors(dailyRewardText.GetComponent<RectTransform>(), new Vector2(0, 0.08f), new Vector2(1, 0.15f));
        dailyRewardText.alignment = TextAlignmentOptions.Center;

        var closeBtn = CreateButton(contentRoot.transform, "CloseBtn", "X");
        SetAnchors(closeBtn.GetComponent<RectTransform>(), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-50, -50), new Vector2(80, 80));

        var so = new SerializedObject(ui);
        so.FindProperty("_panelRoot").objectReferenceValue = contentRoot;
        so.FindProperty("_statusText").objectReferenceValue = statusText;
        so.FindProperty("_benefitsText").objectReferenceValue = benefitsText;
        so.FindProperty("_claimDailyButton").objectReferenceValue = claimBtn;
        so.FindProperty("_dailyRewardText").objectReferenceValue = dailyRewardText;
        so.FindProperty("_closeButton").objectReferenceValue = closeBtn;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ── Chest Opening Panel ───────────────────────────────────────────────────

    static void BuildChestOpeningPanel(GameObject scriptHost, GameObject contentRoot)
    {
        contentRoot.GetComponent<Image>().color = new Color(0, 0, 0, 0.9f);
        var ui = scriptHost.AddComponent<ChestOpeningUI>();

        var chestTypeText = CreateTMPLabel(contentRoot.transform, "ChestTypeText", "Epic Chest");
        SetAnchors(chestTypeText.GetComponent<RectTransform>(), new Vector2(0, 0.75f), new Vector2(1, 0.9f));
        chestTypeText.fontSize = 48;
        chestTypeText.alignment = TextAlignmentOptions.Center;

        var chestImage = CreateChildWithRect(contentRoot.transform, "ChestImage");
        SetAnchors(chestImage.GetComponent<RectTransform>(), new Vector2(0.2f, 0.35f), new Vector2(0.8f, 0.73f));
        var chestImg = chestImage.AddComponent<Image>();
        chestImg.color = new Color(0.8f, 0.6f, 0.2f);

        var rewardsText = CreateTMPLabel(contentRoot.transform, "RewardsText", "Gems, Coins, Bombs & Rockets");
        SetAnchors(rewardsText.GetComponent<RectTransform>(), new Vector2(0, 0.2f), new Vector2(1, 0.33f));
        rewardsText.fontSize = 28;
        rewardsText.alignment = TextAlignmentOptions.Center;

        var collectBtn = CreateButton(contentRoot.transform, "CollectBtn", "COLLECT");
        SetAnchors(collectBtn.GetComponent<RectTransform>(), new Vector2(0.2f, 0.05f), new Vector2(0.8f, 0.17f));

        var so = new SerializedObject(ui);
        so.FindProperty("_panelRoot").objectReferenceValue = contentRoot;
        so.FindProperty("_chestTypeText").objectReferenceValue = chestTypeText;
        so.FindProperty("_chestImage").objectReferenceValue = chestImg;
        so.FindProperty("_rewardsText").objectReferenceValue = rewardsText;
        so.FindProperty("_collectButton").objectReferenceValue = collectBtn;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    // ══════════════════════════════════════════════════════════════════════════
    // Utility methods
    // ══════════════════════════════════════════════════════════════════════════

    static GameObject CreateFullscreenRect(Transform parent, string name)
    {
        var go = CreateChildWithRect(parent, name);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        return go;
    }

    static TextMeshProUGUI CreateHeader(Transform parent, string text)
    {
        var tmp = CreateTMPLabel(parent, "Header", text);
        SetAnchors(tmp.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(20, -60), new Vector2(-20, 60));
        tmp.fontSize = 36;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        return tmp;
    }

    static GameObject CreateChildWithRect(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return go;
    }

    static TextMeshProUGUI CreateTMPLabel(Transform parent, string name, string text)
    {
        var go = CreateChildWithRect(parent, name);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 24;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Left;
        return tmp;
    }

    static Button CreateButton(Transform parent, string name, string label)
    {
        var go = CreateChildWithRect(parent, name);
        go.AddComponent<Image>().color = new Color(0.25f, 0.4f, 0.7f);
        var btn = go.AddComponent<Button>();
        var btnLabel = CreateTMPLabel(go.transform, "Label", label);
        btnLabel.alignment = TextAlignmentOptions.Center;
        SetAnchors(btnLabel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        return btn;
    }

    static Button CreateTabButton(Transform parent, string label)
    {
        var btn = CreateButton(parent, $"Tab_{label}", label);
        var le = btn.gameObject.AddComponent<LayoutElement>();
        le.flexibleWidth = 1;
        return btn;
    }

    static GameObject CreateHorizontalGroup(Transform parent, string name, float height)
    {
        var go = CreateChildWithRect(parent, name);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 1);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(0, height);

        var hlg = go.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 5;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childForceExpandWidth = true;
        hlg.childForceExpandHeight = true;
        hlg.padding = new RectOffset(10, 10, 5, 5);
        return go;
    }

    static ScrollRect CreateScrollArea(Transform parent, string name, float top, float bottom)
    {
        var go = CreateChildWithRect(parent, name);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(10, -bottom);
        rt.offsetMax = new Vector2(-10, top);

        go.AddComponent<Image>().color = new Color(0.08f, 0.08f, 0.1f);
        go.AddComponent<Mask>().showMaskGraphic = true;

        var scrollRect = go.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        var content = CreateChildWithRect(go.transform, "Content");
        var crt = content.GetComponent<RectTransform>();
        crt.anchorMin = new Vector2(0, 1);
        crt.anchorMax = Vector2.one;
        crt.pivot = new Vector2(0.5f, 1);
        crt.anchoredPosition = Vector2.zero;

        var csf = content.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.content = crt;
        return scrollRect;
    }

    static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }

    static void SetButtonArray(SerializedObject so, string propName, Button[] buttons)
    {
        var prop = so.FindProperty(propName);
        prop.arraySize = buttons.Length;
        for (int i = 0; i < buttons.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = buttons[i];
    }

    static void SetImageArray(SerializedObject so, string propName, Image[] images)
    {
        var prop = so.FindProperty(propName);
        prop.arraySize = images.Length;
        for (int i = 0; i < images.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = images[i];
    }

    static void SetTMPArray(SerializedObject so, string propName, TextMeshProUGUI[] texts)
    {
        var prop = so.FindProperty(propName);
        prop.arraySize = texts.Length;
        for (int i = 0; i < texts.Length; i++)
            prop.GetArrayElementAtIndex(i).objectReferenceValue = texts[i];
    }
}
