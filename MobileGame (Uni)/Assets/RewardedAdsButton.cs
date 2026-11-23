using System;
using System.Collections;
using Unity.Services.LevelPlay;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>Loads LevelPlay rewarded ads and saves the meal refill on a reward callback.</summary>
public class RewardedAdsButton : MonoBehaviour
{
    [SerializeField] private Ads ads;
    [SerializeField] private string androidAdUnitId;
    [SerializeField] private string iosAdUnitId;
    [SerializeField] private string placementName;
    [SerializeField, Min(0)] private int maxRetries = 3;
    [SerializeField, Min(1)] private float retryDelay = 5f;
    [FormerlySerializedAs("_showAdButton")]
    [SerializeField, HideInInspector] private Button legacyButton;

    private LevelPlayRewardedAd rewardedAd;
    private bool loading;
    private bool isShowing;
    private bool pendingReward;
    private bool requestedAd;
    private bool rewardGrantedForRequest;
    private int retryCount;
    private float nextRewardRetry;
    private Coroutine retryRoutine;

    public bool IsShowing => isShowing;
    public bool HasPendingReward => pendingReward;
    public bool IsReady => rewardedAd != null && !loading && !isShowing && !pendingReward &&
        rewardedAd.IsAdReady() && !LevelPlayRewardedAd.IsPlacementCapped(placementName);
    public event Action StateChanged;

    private void Awake()
    {
        // Old scene buttons must not promise a reward that has not been configured.
        if (legacyButton != null) legacyButton.interactable = false;
    }

    private void Start()
    {
        if (ads != null && Ads.Instance != null && ads != Ads.Instance)
        {
            enabled = false;
            return;
        }

        if (ads == null || ads != Ads.Instance || ads.gameObject != gameObject)
        {
            Debug.LogError("[LevelPlay] Assign the Ads component on this same persistent root object.", this);
            return;
        }

        ads.Initialized += OnInitialized;
        if (ads.IsReady) OnInitialized();
    }

    private void OnInitialized()
    {
        if (rewardedAd != null) return;
#if UNITY_IOS && !UNITY_EDITOR
        string adUnitId = iosAdUnitId;
#else
        string adUnitId = androidAdUnitId;
#endif
        if (string.IsNullOrWhiteSpace(adUnitId) || string.IsNullOrWhiteSpace(placementName))
        {
            Debug.LogError("[LevelPlay] Set the rewarded Ad Unit ID and meal placement name.", this);
            return;
        }

        rewardedAd = new LevelPlayRewardedAd(adUnitId);
        rewardedAd.OnAdLoaded += OnAdLoaded;
        rewardedAd.OnAdLoadFailed += OnAdLoadFailed;
        rewardedAd.OnAdDisplayFailed += OnAdDisplayFailed;
        rewardedAd.OnAdClosed += OnAdClosed;
        rewardedAd.OnAdRewarded += OnAdRewarded;
        LoadAd();
    }

    public void LoadAd()
    {
        if (rewardedAd == null || loading || isShowing || pendingReward || retryRoutine != null ||
            rewardedAd.IsAdReady()) return;
        retryCount = 0;
        RequestAd();
    }

    private void RequestAd()
    {
        loading = true;
        rewardedAd.LoadAd();
        StateChanged?.Invoke();
    }

    public void ShowAd()
    {
        if (!IsReady) return;
        isShowing = true;
        requestedAd = true;
        rewardGrantedForRequest = false;
        StateChanged?.Invoke();
        rewardedAd.ShowAd(placementName);
    }

    private void OnAdLoaded(LevelPlayAdInfo info)
    {
        loading = false;
        retryCount = 0;
        StateChanged?.Invoke();
    }

    private void OnAdLoadFailed(LevelPlayAdError error)
    {
        loading = false;
        Debug.LogWarning($"[LevelPlay] Rewarded ad failed to load: {error}", this);
        if (retryCount < maxRetries && isActiveAndEnabled)
        {
            retryCount++;
            retryRoutine = StartCoroutine(RetryLoadAd());
        }
        StateChanged?.Invoke();
    }

    private IEnumerator RetryLoadAd()
    {
        yield return new WaitForSecondsRealtime(retryDelay);
        retryRoutine = null;
        if (rewardedAd != null && !loading && !isShowing && !pendingReward) RequestAd();
    }

    private void OnAdDisplayFailed(LevelPlayAdInfo info, LevelPlayAdError error)
    {
        isShowing = false;
        requestedAd = false;
        Debug.LogWarning($"[LevelPlay] Rewarded ad failed to show: {error}", this);
        StateChanged?.Invoke();
        LoadAd();
    }

    private void OnAdClosed(LevelPlayAdInfo info)
    {
        isShowing = false;
        StateChanged?.Invoke();
        LoadAd();
    }

    private void OnAdRewarded(LevelPlayAdInfo info, LevelPlayReward reward)
    {
        // LevelPlay may deliver this after OnAdClosed; don't gate it on isShowing.
        if (!requestedAd || rewardGrantedForRequest) return;
        rewardGrantedForRequest = true;

        pendingReward = true;
        SaveReward();
    }

    private void Update()
    {
        if (pendingReward && Time.unscaledTime >= nextRewardRetry) SaveReward();
    }

    private void SaveReward()
    {
        nextRewardRetry = Time.unscaledTime + 5f;
        PlayerProfileData data = PlayerProfileHolder.Instance != null ? PlayerProfileHolder.Instance.CurrentData : null;
        if (data == null || LocalSaveStorage.Instance == null)
        {
            Debug.LogError("[LevelPlay] Cannot save the meal reward: profile or local storage is unavailable.", this);
            StateChanged?.Invoke();
            return;
        }

        int previousBones = data.FilledMealBones;
        long previousTime = data.NextMealBoneEmptiesAt;
        int previousRevision = data.SaveRevision;
        MealProgress.ClearBones(data);
        if (!LocalSaveStorage.Instance.Save())
        {
            data.FilledMealBones = previousBones;
            data.NextMealBoneEmptiesAt = previousTime;
            data.SaveRevision = previousRevision;
            Debug.LogError("[LevelPlay] Could not save the meal reward; retrying.", this);
            StateChanged?.Invoke();
            return;
        }

        pendingReward = false;
        _ = ProfileSyncService.SyncSavedProfileAsync();
        StateChanged?.Invoke();
        LoadAd();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus) LoadAd();
    }

    private void OnDestroy()
    {
        if (ads != null) ads.Initialized -= OnInitialized;
        if (rewardedAd == null) return;
        rewardedAd.OnAdLoaded -= OnAdLoaded;
        rewardedAd.OnAdLoadFailed -= OnAdLoadFailed;
        rewardedAd.OnAdDisplayFailed -= OnAdDisplayFailed;
        rewardedAd.OnAdClosed -= OnAdClosed;
        rewardedAd.OnAdRewarded -= OnAdRewarded;
        rewardedAd.DestroyAd();
    }
}
