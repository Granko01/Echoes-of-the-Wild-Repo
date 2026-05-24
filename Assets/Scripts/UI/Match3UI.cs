using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Match3UI : MonoBehaviour
{
    private Button[,]        _buttons;
    private Image[,]         _images;
    private int              _selX = -1, _selY = -1;
    private TextMeshProUGUI  _statusText;
    private TextMeshProUGUI  _hintText;

    private static readonly Color ColSelected = new Color(1.00f, 0.85f, 0.10f); // bright yellow
    private static readonly Color ColInvalid  = new Color(0.90f, 0.20f, 0.20f); // red flash

    private static readonly Color[] TileColors =
    {
        new Color(0.25f, 0.55f, 1.00f), // Calm  — blue
        new Color(0.25f, 0.85f, 0.40f), // Trust — green
        new Color(0.70f, 0.30f, 0.90f), // Bond  — purple
        new Color(0.90f, 0.30f, 0.30f), // Fear  — red
    };

    private static readonly string[] TileLabels = { "CALM", "TRUST", "BOND", "FEAR" };

    public void BuildGrid(int width, int height)
    {
        foreach (Transform child in transform) Destroy(child.gameObject);
        _statusText = null;
        _hintText   = null;

        CreateHeader();

        var layout = GetComponent<GridLayoutGroup>();
        layout.constraint      = GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount = width;
        layout.cellSize        = new Vector2(100, 100);
        layout.spacing         = new Vector2(12, 12);
        // top padding leaves room for the header text
        layout.padding         = new RectOffset(20, 20, 100, 20);

        _buttons = new Button[width, height];
        _images  = new Image[width, height];
        _selX = _selY = -1;

        for (int y = height - 1; y >= 0; y--)
            for (int x = 0; x < width; x++)
            {
                int cx = x, cy = y;
                CreateCell(cx, cy, out _buttons[x, y], out _images[x, y]);
                _buttons[x, y].onClick.AddListener(() => OnClick(cx, cy));
            }

        UpdateStatus();
        SetHint("Tap a tile to select it");
        Refresh();
    }

    public void Refresh()
    {
        if (_buttons == null) return;
        int w = _buttons.GetLength(0), h = _buttons.GetLength(1);
        for (int x = 0; x < w; x++)
            for (int y = 0; y < h; y++)
                ApplyTile(x, y, selected: x == _selX && y == _selY);
        UpdateStatus();
    }

    private void OnClick(int x, int y)
    {
        if (_selX == -1)
        {
            _selX = x; _selY = y;
            ApplyTile(x, y, selected: true);
            SetHint("Now tap an adjacent tile to swap");
            return;
        }

        int px = _selX, py = _selY;
        _selX = _selY = -1;

        // Same tile → deselect
        if (x == px && y == py)
        {
            ApplyTile(px, py, selected: false);
            SetHint("Tap a tile to select it");
            return;
        }

        // Non-adjacent → flash red and reset
        if (Mathf.Abs(x - px) + Mathf.Abs(y - py) != 1)
        {
            StartCoroutine(FlashInvalid(px, py));
            SetHint("Tiles must be next to each other");
            return;
        }

        bool swapped = Match3Manager.Instance.OnSwap(px, py, x, y);
        if (swapped)
        {
            Refresh();
            SetHint("Keep matching to calm!");
        }
        else
        {
            StartCoroutine(FlashInvalid(px, py));
            SetHint("No match there — try a different pair");
        }
    }

    private void UpdateStatus()
    {
        if (_statusText == null) return;
        var target = Match3Manager.Instance?.Target;
        if (target == null) return;
        var sm = target.GetComponent<EntityStateMachine>();
        if (sm == null) return;

        // Distress shown as filled / empty dots (max 5 levels above Stable)
        int level = Mathf.Clamp((int)sm.Current, 0, 5);
        string dots = "";
        for (int i = 1; i <= 5; i++)
            dots += i <= level ? "<color=#FF6B35>●</color>" : "<color=#555555>●</color>";
        _statusText.text = $"<b>{target.Type}</b>   {dots}";
    }

    private void SetHint(string msg)
    {
        if (_hintText != null) _hintText.text = msg;
    }

    private IEnumerator FlashInvalid(int x, int y)
    {
        Refresh();
        _images[x, y].color = ColInvalid;
        // WaitForSecondsRealtime ignores timeScale=0.2 so the flash isn't super slow
        yield return new WaitForSecondsRealtime(0.3f);
        ApplyTile(x, y, selected: false);
    }

    private void ApplyTile(int x, int y, bool selected)
    {
        if (_images == null) return;
        var tile = Match3Manager.Instance.GetTile(x, y);
        _images[x, y].color = selected ? ColSelected : TileColors[(int)tile];
        var label = _buttons[x, y].GetComponentInChildren<TextMeshProUGUI>();
        if (label != null) label.text = TileLabels[(int)tile];
    }

    private void CreateHeader()
    {
        // Status row — entity name + distress dots
        var statusGo = new GameObject("Status");
        statusGo.transform.SetParent(transform, false);
        var sLe = statusGo.AddComponent<LayoutElement>();
        sLe.ignoreLayout = true;
        var sRt = statusGo.GetComponent<RectTransform>();
        sRt.anchorMin        = new Vector2(0, 1);
        sRt.anchorMax        = new Vector2(1, 1);
        sRt.pivot            = new Vector2(0.5f, 1f);
        sRt.anchoredPosition = new Vector2(0, -10);
        sRt.sizeDelta        = new Vector2(0, 38);
        _statusText = statusGo.AddComponent<TextMeshProUGUI>();
        _statusText.alignment       = TextAlignmentOptions.Center;
        _statusText.fontSize        = 22;
        _statusText.color           = Color.white;
        _statusText.richText        = true;
        _statusText.text            = "Match 3 to calm";

        // Hint row — contextual instruction
        var hintGo = new GameObject("Hint");
        hintGo.transform.SetParent(transform, false);
        var hLe = hintGo.AddComponent<LayoutElement>();
        hLe.ignoreLayout = true;
        var hRt = hintGo.GetComponent<RectTransform>();
        hRt.anchorMin        = new Vector2(0, 1);
        hRt.anchorMax        = new Vector2(1, 1);
        hRt.pivot            = new Vector2(0.5f, 1f);
        hRt.anchoredPosition = new Vector2(0, -55);
        hRt.sizeDelta        = new Vector2(0, 30);
        _hintText = hintGo.AddComponent<TextMeshProUGUI>();
        _hintText.alignment       = TextAlignmentOptions.Center;
        _hintText.fontSize        = 15;
        _hintText.color           = new Color(0.80f, 0.80f, 0.80f);
        _hintText.text            = "";
    }

    private void CreateCell(int x, int y, out Button btn, out Image img)
    {
        var go = new GameObject($"Tile_{x}_{y}");
        go.transform.SetParent(transform, false);

        img       = go.AddComponent<Image>();
        img.color = Color.white;

        btn = go.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(1f, 1f, 0.7f);
        colors.pressedColor     = new Color(0.75f, 0.75f, 0.75f);
        btn.colors = colors;

        var labelGo = new GameObject("Label");
        labelGo.transform.SetParent(go.transform, false);
        var rt = labelGo.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var tmp = labelGo.AddComponent<TextMeshProUGUI>();
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontSize  = 18;
        tmp.fontStyle = FontStyles.Bold;
        tmp.color     = Color.white;
    }
}
