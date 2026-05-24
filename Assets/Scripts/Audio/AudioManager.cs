using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private AudioMixer  _mixer;
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;
    [SerializeField] private AudioSource _ambientSource;

    private const string ParamAmbientVol = "AmbientVolume";
    private const string ParamReverbWet  = "ReverbWet";
    private const string ParamMusicVol   = "MusicVolume";
    private const string ParamSFXVol     = "SFXVolume";

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        GameEvents.OnHealComplete    += OnHealComplete;
        GameEvents.OnRealmTransition += OnRealmTransition;
    }

    private void OnDisable()
    {
        GameEvents.OnHealComplete    -= OnHealComplete;
        GameEvents.OnRealmTransition -= OnRealmTransition;
    }

    public void SetMusicVolume(float normalizedValue)
    {
        if (_mixer != null)
            _mixer.SetFloat(ParamMusicVol, Mathf.Log10(Mathf.Max(normalizedValue, 0.0001f)) * 20f);
    }

    public void SetSFXVolume(float normalizedValue)
    {
        if (_mixer != null)
            _mixer.SetFloat(ParamSFXVol, Mathf.Log10(Mathf.Max(normalizedValue, 0.0001f)) * 20f);
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && _sfxSource != null)
            _sfxSource.PlayOneShot(clip, volume);
    }

    public void CrossFadeMusic(AudioClip clip, float duration = 1f)
        => StartCoroutine(CrossFadeRoutine(clip, duration));

    private void OnHealComplete(BiomeArea area)
    {
        // Blend nature layers back in
        if (_mixer != null)
            StartCoroutine(FadeMixerParam(ParamAmbientVol, -20f, 0f, 2f));
    }

    private void OnRealmTransition()
    {
        // Switch to spirit realm reverb bus
        if (_mixer != null)
            StartCoroutine(FadeMixerParam(ParamReverbWet, 0f, 0.8f, 1.5f));
    }

    private IEnumerator CrossFadeRoutine(AudioClip clip, float duration)
    {
        float half = duration / 2f;
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            if (_musicSource != null) _musicSource.volume = 1f - t / half;
            yield return null;
        }
        if (_musicSource != null) { _musicSource.clip = clip; _musicSource.Play(); }
        for (float t = 0f; t < half; t += Time.deltaTime)
        {
            if (_musicSource != null) _musicSource.volume = t / half;
            yield return null;
        }
        if (_musicSource != null) _musicSource.volume = 1f;
    }

    private IEnumerator FadeMixerParam(string param, float from, float to, float duration)
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            _mixer?.SetFloat(param, Mathf.Lerp(from, to, t / duration));
            yield return null;
        }
        _mixer?.SetFloat(param, to);
    }
}
