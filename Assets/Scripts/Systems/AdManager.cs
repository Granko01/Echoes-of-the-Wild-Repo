using System;
using UnityEngine;
using UnityEngine.Advertisements;

public enum AdRewardType { Coins, Gems, Match3Lives, ContinueAfterFailure }

public class AdManager : MonoBehaviour,
    IUnityAdsInitializationListener,
    IUnityAdsLoadListener,
    IUnityAdsShowListener
{
    public static AdManager Instance { get; private set; }

    [Header("Game IDs — fill from Unity Dashboard")]
    [SerializeField] private string _androidGameId = "YOUR_ANDROID_GAME_ID";
    [SerializeField] private string _iosGameId     = "YOUR_IOS_GAME_ID";
    [SerializeField] private bool   _testMode      = true;

    [Header("Ad Unit IDs")]
    [SerializeField] private string _rewardedAndroid     = "Rewarded_Android";
    [SerializeField] private string _rewardedIOS         = "Rewarded_iOS";
    [SerializeField] private string _interstitialAndroid = "Interstitial_Android";
    [SerializeField] private string _interstitialIOS     = "Interstitial_iOS";

    [Header("Frequency — Interstitials")]
    [SerializeField] private float _minInterstitialGapSecs = 180f; // 3 min between interstitials

    private bool          _rewardedLoaded;
    private bool          _interstitialLoaded;
    private float         _lastInterstitialTime = float.MinValue;
    private AdRewardType  _pendingRewardType;
    private Action        _onRewardSuccess;
    private Action        _onRewardFail;

    // Ads are blocked if the player bought Remove Ads or has active VIP
    public bool AdsRemoved => SaveSystem.Data.adsRemoved || SaveSystem.Data.vipActive;

    private string RewardedUnitId =>
        Application.platform == RuntimePlatform.IPhonePlayer ? _rewardedIOS : _rewardedAndroid;

    private string InterstitialUnitId =>
        Application.platform == RuntimePlatform.IPhonePlayer ? _interstitialIOS : _interstitialAndroid;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        string gameId = Application.platform == RuntimePlatform.IPhonePlayer
            ? _iosGameId : _androidGameId;
        Advertisement.Initialize(gameId, _testMode, this);
    }

    // ── Public API ────────────────────────────────────────────────────────────

    // Rewarded ads are always available regardless of Remove Ads purchase
    public void ShowRewardedAd(AdRewardType rewardType, Action onSuccess = null, Action onFail = null)
    {
        if (!_rewardedLoaded)
        {
            Debug.LogWarning("[AdManager] Rewarded ad not ready.");
            onFail?.Invoke();
            return;
        }

        _pendingRewardType = rewardType;
        _onRewardSuccess   = onSuccess;
        _onRewardFail      = onFail;
        Advertisement.Show(RewardedUnitId, this);
    }

    // Call this at natural break points (between stages, after Match-3 levels).
    // Returns true if an ad was shown, false if blocked or on cooldown.
    public bool TryShowInterstitial()
    {
        if (AdsRemoved)        return false;
        if (!_interstitialLoaded) return false;
        if (Time.unscaledTime - _lastInterstitialTime < _minInterstitialGapSecs) return false;

        _lastInterstitialTime = Time.unscaledTime;
        Advertisement.Show(InterstitialUnitId, this);
        return true;
    }

    // ── IUnityAdsInitializationListener ──────────────────────────────────────

    public void OnInitializationComplete()
    {
        Debug.Log("[AdManager] Unity Ads initialized.");
        LoadRewarded();
        LoadInterstitial();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message) =>
        Debug.LogError($"[AdManager] Init failed: {error} — {message}");

    // ── IUnityAdsLoadListener ─────────────────────────────────────────────────

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        if (adUnitId == RewardedUnitId)     _rewardedLoaded     = true;
        if (adUnitId == InterstitialUnitId) _interstitialLoaded = true;
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.LogWarning($"[AdManager] Load failed ({adUnitId}): {error} — {message}");
    }

    // ── IUnityAdsShowListener ─────────────────────────────────────────────────

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState completionState)
    {
        if (adUnitId == RewardedUnitId)
        {
            if (completionState == UnityAdsShowCompletionState.COMPLETED)
            {
                GrantReward(_pendingRewardType);
                _onRewardSuccess?.Invoke();
            }
            else
            {
                _onRewardFail?.Invoke();
            }

            _onRewardSuccess = null;
            _onRewardFail    = null;
            _rewardedLoaded  = false;
            LoadRewarded(); // pre-load next ad immediately
        }
        else if (adUnitId == InterstitialUnitId)
        {
            _interstitialLoaded = false;
            LoadInterstitial();
        }
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.LogWarning($"[AdManager] Show failed ({adUnitId}): {error} — {message}");
        if (adUnitId == RewardedUnitId)
        {
            _onRewardFail?.Invoke();
            _onRewardSuccess = null;
            _onRewardFail    = null;
        }
    }

    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }

    // ── Reward Grant ──────────────────────────────────────────────────────────

    private void GrantReward(AdRewardType type)
    {
        switch (type)
        {
            case AdRewardType.Coins:
                CurrencyManager.Instance.AddCoins(100);
                break;
            case AdRewardType.Gems:
                CurrencyManager.Instance.AddGems(10);
                break;
            case AdRewardType.Match3Lives:
                EnergyManager.Instance.AddEnergy(5);
                break;
            case AdRewardType.ContinueAfterFailure:
                // Signal only — respawn logic handled by the caller's onSuccess callback
                break;
        }

        GameEvents.RaiseRewardedAdCompleted(type);
    }

    // ── Internal ──────────────────────────────────────────────────────────────

    private void LoadRewarded()
    {
        _rewardedLoaded = false;
        Advertisement.Load(RewardedUnitId, this);
    }

    private void LoadInterstitial()
    {
        _interstitialLoaded = false;
        Advertisement.Load(InterstitialUnitId, this);
    }
}
