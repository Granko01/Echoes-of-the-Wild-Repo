using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    private const string GameSceneName = "SampleScene";

    [Header("Panels")]
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _howToPlayPanel;
    [SerializeField] private GameObject _characterPanel;

    [Header("Character Select")]
    [SerializeField] private CharacterSelectManager _characterSelectManager;

    [Header("Button Tween")]
    [SerializeField] private float _tweenDuration = 0.12f;
    [SerializeField] private float _tweenScale     = 0.88f;

    [Header("How To Play Back")]
    [SerializeField] private Button _howToPlayBackButton;

    private void Awake()
    {
        ShowMain();
        if (_howToPlayBackButton != null)
            _howToPlayBackButton.onClick.AddListener(ShowMain);
    }

    public void ShowMain()
    {
        SetPanel(_mainPanel,      true);
        SetPanel(_settingsPanel,  false);
        SetPanel(_howToPlayPanel, false);
        SetPanel(_characterPanel, false);
    }

    public void ShowSettings()
    {
        SetPanel(_mainPanel,      false);
        SetPanel(_settingsPanel,  true);
        SetPanel(_howToPlayPanel, false);
        SetPanel(_characterPanel, false);
    }

    public void ShowHowToPlay()
    {
        SetPanel(_mainPanel,      false);
        SetPanel(_settingsPanel,  false);
        SetPanel(_howToPlayPanel, true);
        SetPanel(_characterPanel, false);
    }

    public void ShowCharacterSelect()
    {
        SetPanel(_mainPanel,      false);
        SetPanel(_settingsPanel,  false);
        SetPanel(_howToPlayPanel, false);
        SetPanel(_characterPanel, true);
    }

    // Called by the Play button in the character select panel
    public void StartGame()
    {
        if (_characterSelectManager != null)
            _characterSelectManager.ConfirmSelection();
        SceneManager.LoadScene(GameSceneName);
    }

    public void OnButtonPress(Button button)
    {
        if (button != null)
            StartCoroutine(ButtonScaleTween(button.transform));
    }

    private IEnumerator ButtonScaleTween(Transform t)
    {
        Vector3 original = t.localScale;
        Vector3 pressed  = original * _tweenScale;

        for (float e = 0f; e < _tweenDuration; e += Time.unscaledDeltaTime)
        {
            t.localScale = Vector3.Lerp(original, pressed, e / _tweenDuration);
            yield return null;
        }
        t.localScale = pressed;

        for (float e = 0f; e < _tweenDuration; e += Time.unscaledDeltaTime)
        {
            t.localScale = Vector3.Lerp(pressed, original, e / _tweenDuration);
            yield return null;
        }
        t.localScale = original;
    }

    private static void SetPanel(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }
}
