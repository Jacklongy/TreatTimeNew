using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>Displays meal progress and handles menu refills and play access.</summary>
public class MainMealManagerHome : MonoBehaviour
{
    private const float RefreshIntervalSeconds = 1f;

    [Serializable]
    private class BoneVisual
    {
        public GameObject full;
        public GameObject empty;
    }

    [Header("Meal display")]
    [SerializeField] private BoneVisual[] bones = new BoneVisual[MealProgress.MaxBones];
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text fullMessageText;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Play and refill buttons")]
    [SerializeField] private GameObject playButton;
    [SerializeField] private Button gemRefillButton;
    [SerializeField] private Button adButton;
    [SerializeField] private RewardedAdsButton rewardedAd;
    [SerializeField, Min(1)] private int gemRefillCost = 50;
    [SerializeField] private TMP_Text gemBalanceText;

    private bool initialized;
    private bool adConfigured;
    private bool wasFull;
    private float nextRefresh;
    private string feedback = "";

    /// <summary>Call after the menu has loaded the player profile.</summary>
    public void Initialize()
    {
        if (PlayerProfileHolder.Instance == null || PlayerProfileHolder.Instance.CurrentData == null ||
            bones == null || bones.Length != MealProgress.MaxBones || timerText == null ||
            fullMessageText == null || playButton == null || gemRefillButton == null || adButton == null ||
            gemRefillCost <= 0)
        {
            Debug.LogError("MainMealManagerHome is missing a profile or required UI references.", this);
            return;
        }

        foreach (BoneVisual bone in bones)
        {
            if (bone == null || bone.full == null || bone.empty == null || bone.full == bone.empty)
            {
                Debug.LogError("Each meal bone needs separate full and empty objects.", this);
                return;
            }
        }

        if (initialized) return;
        initialized = true;
        gemRefillButton.onClick.AddListener(RefillWithGems);
        adButton.interactable = false;
        // Returning to the menu creates a new scene object; use the persistent ad service.
        if (rewardedAd != null && Ads.Instance != null)
        {
            RewardedAdsButton persistentAd = Ads.Instance.GetComponent<RewardedAdsButton>();
            if (persistentAd != null) rewardedAd = persistentAd;
        }
        // The controller must remain active when the menu hides its buttons.
        if (rewardedAd != null &&
            !rewardedAd.transform.IsChildOf(adButton.transform) &&
            !rewardedAd.transform.IsChildOf(gemRefillButton.transform) &&
            !rewardedAd.transform.IsChildOf(playButton.transform))
        {
            adConfigured = true;
            rewardedAd.StateChanged += Refresh;
            adButton.onClick.AddListener(rewardedAd.ShowAd);
            rewardedAd.LoadAd();
        }
        else
        {
            Debug.LogError("Assign a rewarded ad controller on an active object outside the ad button, using the same ad button.", this);
        }
        Refresh();
    }

    private void Update()
    {
        if (!initialized || Time.unscaledTime < nextRefresh) return;
        nextRefresh = Time.unscaledTime + RefreshIntervalSeconds;
        Refresh();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && initialized)
        {
            Refresh();
            if (adConfigured) rewardedAd.LoadAd();
        }
    }

    private void OnDestroy()
    {
        if (initialized && gemRefillButton != null) gemRefillButton.onClick.RemoveListener(RefillWithGems);
        if (rewardedAd != null && adConfigured)
        {
            rewardedAd.StateChanged -= Refresh;
            if (adButton != null) adButton.onClick.RemoveListener(rewardedAd.ShowAd);
        }
    }

    /// <summary>Checks the latest meal state before entering the game.</summary>
    public bool TryStartRun()
    {
        if (!initialized)
        {
            Debug.LogError("Meal manager is not initialized; cannot start a run.", this);
            return false;
        }

        if (rewardedAd != null && (rewardedAd.HasPendingReward || rewardedAd.IsShowing)) return false;
        Refresh();
        return MealProgress.TryBeginRun(PlayerProfileHolder.Instance.CurrentData, MealProgress.UtcNowSeconds);
    }

    private void RefillWithGems()
    {
        if (!initialized || (rewardedAd != null && (rewardedAd.HasPendingReward || rewardedAd.IsShowing))) return;
        PlayerProfileData data = PlayerProfileHolder.Instance.CurrentData;
        Refresh();
        if (data.FilledMealBones != MealProgress.MaxBones) return;
        if (data.Gems < gemRefillCost)
        {
            SetFeedback($"Not enough gems. You need {gemRefillCost}.");
            return;
        }

        SaveRefill(data, gemRefillCost);
    }

    private void SaveRefill(PlayerProfileData data, int cost)
    {
        // Keep the spend and refill together; restore both if the local save fails.
        int previousBones = data.FilledMealBones;
        long previousTime = data.NextMealBoneEmptiesAt;
        int previousGems = data.Gems;
        int previousRevision = data.SaveRevision;
        data.Gems -= cost;
        MealProgress.ClearBones(data);

        if (LocalSaveStorage.Instance == null || !LocalSaveStorage.Instance.Save())
        {
            data.FilledMealBones = previousBones;
            data.NextMealBoneEmptiesAt = previousTime;
            data.Gems = previousGems;
            data.SaveRevision = previousRevision;
            Debug.LogError("Meal refill could not be saved; no gems were spent.", this);
            SetFeedback("Couldn't save the refill. Please try again.");
            Refresh();
            return;
        }

        _ = ProfileSyncService.SyncSavedProfileAsync();
        SetFeedback("");
        Refresh();
    }

    private void SetFeedback(string message)
    {
        feedback = message;
    }

    private void Refresh()
    {
        PlayerProfileData data = PlayerProfileHolder.Instance.CurrentData;
        long now = MealProgress.UtcNowSeconds;

        // Recalculate from the saved timestamp so time away from the app counts.
        if (MealProgress.Refresh(data, now))
        {
            _ = ProfileSyncService.SaveAndSyncAsync();
        }

        UpdateBoneVisuals(data);
        UpdateButtonsAndMessages(data);
        UpdateTimer(data, now);
    }

    private void UpdateBoneVisuals(PlayerProfileData data)
    {
        for (int i = 0; i < bones.Length; i++)
        {
            bool filled = i < data.FilledMealBones;
            bones[i].full.SetActive(filled);
            bones[i].empty.SetActive(!filled);
        }
    }

    private void UpdateButtonsAndMessages(PlayerProfileData data)
    {
        bool isFull = data.FilledMealBones == MealProgress.MaxBones;
        if (isFull && !wasFull && adConfigured)
        {
            rewardedAd.LoadAd();
        }
        wasFull = isFull;

        bool adShowing = rewardedAd != null && rewardedAd.IsShowing;
        bool pendingAdReward = rewardedAd != null && rewardedAd.HasPendingReward;
        playButton.SetActive(!isFull && !adShowing && !pendingAdReward);
        gemRefillButton.gameObject.SetActive(isFull && !adShowing && !pendingAdReward);
        gemRefillButton.interactable = data.Gems >= gemRefillCost;
        adButton.gameObject.SetActive((isFull || adShowing) && !pendingAdReward);
        adButton.interactable = adConfigured && rewardedAd.IsReady;

        string message = pendingAdReward ? "Couldn't save your ad reward. Retrying; please keep the game open." : feedback;
        if (string.IsNullOrEmpty(message) && isFull && data.Gems < gemRefillCost)
        {
            message = $"You need {gemRefillCost} gems.";
        }

        if (isFull && !adShowing && !pendingAdReward && (!adConfigured || !rewardedAd.IsReady))
        {
            message = string.IsNullOrEmpty(message) ? "Ad unavailable or loading." : $"{message}\nAd unavailable or loading.";
        }

        fullMessageText.gameObject.SetActive(isFull || (feedbackText == null && !string.IsNullOrEmpty(message)));
        fullMessageText.text = isFull ? $"{data.DogName} is full!" : "";
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }
        else if (!string.IsNullOrEmpty(message))
        {
            // Reuse the full message if no separate feedback label was assigned.
            fullMessageText.text = isFull ? $"{fullMessageText.text}\n{message}" : message;
        }

        if (gemBalanceText != null) gemBalanceText.text = data.Gems.ToString("N0");
    }

    private void UpdateTimer(PlayerProfileData data, long now)
    {
        long remaining = data.FilledMealBones > 0 ? Math.Max(0, data.NextMealBoneEmptiesAt - now) : 0;
        timerText.text = data.FilledMealBones > 0 ? $"{remaining / 60:00}:{remaining % 60:00}" : "";
    }
}
