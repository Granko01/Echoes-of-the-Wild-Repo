using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    private const string KeyMusic   = "MusicVolume";
    private const string KeySFX     = "SFXVolume";
    private const string KeyQuality = "QualityLevel";

    [Header("Sliders")]
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    [Header("Quality")]
    [SerializeField] private TMP_Dropdown _qualityDropdown;

    private void Start()
    {
        PopulateQualityDropdown();
        LoadAndApplySettings();
        RegisterListeners();
    }

    private void PopulateQualityDropdown()
    {
        if (_qualityDropdown == null) return;
        _qualityDropdown.ClearOptions();
        _qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
    }

    private void LoadAndApplySettings()
    {
        float music   = PlayerPrefs.GetFloat(KeyMusic,   0.8f);
        float sfx     = PlayerPrefs.GetFloat(KeySFX,     0.8f);
        int   quality = PlayerPrefs.GetInt(KeyQuality,   QualitySettings.GetQualityLevel());

        if (_musicSlider    != null) _musicSlider.value    = music;
        if (_sfxSlider      != null) _sfxSlider.value      = sfx;
        if (_qualityDropdown != null)
        {
            _qualityDropdown.value = Mathf.Clamp(quality, 0, QualitySettings.names.Length - 1);
            _qualityDropdown.RefreshShownValue();
        }

        ApplyMusic(music);
        ApplySFX(sfx);
        ApplyQuality(quality);
    }

    private void RegisterListeners()
    {
        if (_musicSlider    != null) _musicSlider.onValueChanged.AddListener(OnMusicChanged);
        if (_sfxSlider      != null) _sfxSlider.onValueChanged.AddListener(OnSFXChanged);
        if (_qualityDropdown != null) _qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
    }

    private void OnMusicChanged(float value)
    {
        PlayerPrefs.SetFloat(KeyMusic, value);
        PlayerPrefs.Save();
        ApplyMusic(value);
    }

    private void OnSFXChanged(float value)
    {
        PlayerPrefs.SetFloat(KeySFX, value);
        PlayerPrefs.Save();
        ApplySFX(value);
    }

    private void OnQualityChanged(int index)
    {
        PlayerPrefs.SetInt(KeyQuality, index);
        PlayerPrefs.Save();
        ApplyQuality(index);
    }

    private static void ApplyMusic(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(value);
    }

    private static void ApplySFX(float value)
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetSFXVolume(value);
    }

    private static void ApplyQuality(int index)
    {
        QualitySettings.SetQualityLevel(index, applyExpensiveChanges: true);
    }
}
