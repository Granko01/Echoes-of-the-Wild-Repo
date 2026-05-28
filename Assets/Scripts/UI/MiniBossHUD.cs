using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Wire this to a Canvas panel with:
//   - _nameLabel  : TextMeshProUGUI showing the mini-boss name
//   - _stateFill  : Image (filled, horizontal) whose fillAmount maps 1 = Turbulent → 0 = Stable
//   - _panel      : CanvasGroup on the root panel (for fade in/out)
// MiniBossController fires GameEvents; HUDManager forwards them here.
public class MiniBossHUD : MonoBehaviour
{
    [SerializeField] private CanvasGroup      _panel;
    [SerializeField] private TextMeshProUGUI  _nameLabel;
    [SerializeField] private Image            _stateFill;
    [SerializeField] private float            _fadeDuration = 0.4f;

    private static readonly string[] Names =
    {
        "Cave Maw",          // MiniBossType.CaveMaw
        "Baby Deer",         // MiniBossType.BabyDeer
        "Ice Hollow Gorilla" // MiniBossType.IceHollowGorilla
    };

    private void Awake()
    {
        if (_panel != null) { _panel.alpha = 0f; _panel.blocksRaycasts = false; }
    }

    public void OnMiniBossActivated(MiniBossType type)
    {
        if (_nameLabel != null) _nameLabel.text = Names[(int)type];
        if (_stateFill  != null) _stateFill.fillAmount = 1f;
        StopAllCoroutines();
        StartCoroutine(Fade(0f, 1f));
    }

    // Called whenever the active mini-boss entity changes state.
    // Maps EntityState value (1–5) to a 0–1 fill (Receptive→Turbulent).
    public void OnStateChanged(EntityState state)
    {
        if (_stateFill == null) return;
        // Clamp to the 5 non-Stable states; Stable is handled by OnMiniBossDefeated
        int stateVal = Mathf.Clamp((int)state, 0, 5);
        _stateFill.fillAmount = stateVal / 5f;
    }

    public void OnMiniBossDefeated()
    {
        StopAllCoroutines();
        StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator Fade(float from, float to)
    {
        if (_panel == null) yield break;
        _panel.blocksRaycasts = (to > 0f);
        for (float t = 0f; t < _fadeDuration; t += Time.deltaTime)
        {
            _panel.alpha = Mathf.Lerp(from, to, t / _fadeDuration);
            yield return null;
        }
        _panel.alpha = to;
        if (to == 0f) _panel.blocksRaycasts = false;
    }
}
