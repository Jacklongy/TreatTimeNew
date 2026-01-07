using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using System.Collections;

public class RewardedAdsButton : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] Button _showAdButton;
    [SerializeField] string _androidAdUnitId = "Rewarded_Android";
    //[SerializeField] string _iOSAdUnitId = "Rewarded_iOS";
    string _adUnitId = null; // This will remain null for unsupported platforms

    [SerializeField] private int maxRetries = 3;
    [SerializeField] private float retryDelay = 2f;
    private int currentRetryCount = 0;

    DogFeeder dog;

    void Awake()
    {
        // Get the Ad Unit ID for the current platform:
#if UNITY_IOS
        _adUnitId = _iOSAdUnitId;
#elif UNITY_ANDROID
        _adUnitId = _androidAdUnitId;
#elif UNITY_EDITOR
        _adUnitId = _androidAdUnitId; // Use Android ID for testing in Editor
#endif

        // Disable the button until the ad is ready to show:
        _showAdButton.interactable = false;
    }

    // Call this public method when you want to get an ad ready to show.
    public void LoadAd()
    {
        // IMPORTANT! Only load content AFTER initialization (in this example, initialization is handled in a different script).
        if (string.IsNullOrEmpty(_adUnitId))
        {
            Debug.LogWarning("Ad Unit ID not set for this platform. Ads not available.");
            return;
        }

        currentRetryCount = 0;
        Debug.Log("Loading Ad: " + _adUnitId);
        Advertisement.Load(_adUnitId, this);
    }

    // If the ad successfully loads, add a listener to the button and enable it:
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        Debug.Log("Ad Loaded: " + adUnitId);

        if (adUnitId.Equals(_adUnitId))
        {
            // Configure the button to call the ShowAd() method when clicked:
            _showAdButton.onClick.AddListener(ShowAd);
            // Enable the button for users to click:
            _showAdButton.interactable = true;
            currentRetryCount = 0; // Reset retry count on success
        }
    }

    // Implement a method to execute when the user clicks the button:
    public void ShowAd()
    {
        // Disable the button:
        _showAdButton.interactable = false;
        // Then show the ad:
        Advertisement.Show(_adUnitId, this);
    }

    // Implement the Show Listener's OnUnityAdsShowComplete callback method to determine if the user gets a reward:
    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Unity Ads Rewarded Ad Completed");

            dog = FindObjectOfType<DogFeeder>(); 

            if (dog != null)
            {
                dog.AdReward(true);
            }
            else
            {
                Debug.LogWarning("DogFeeder not found in scene!");
            }

            _showAdButton.interactable = true;
            // Grant a reward.
        }
        else
        {
            // Ad was skipped or closed
            Debug.Log("Rewarded ad was not completed.");
            _showAdButton.interactable = true;
        }
    }

    // Implement Load and Show Listener error callbacks:
    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");

        if (adUnitId.Equals(_adUnitId))
        {
            if (currentRetryCount < maxRetries)
            {
                currentRetryCount++;
                Debug.Log($"Retrying ad load ({currentRetryCount}/{maxRetries}) after {retryDelay} seconds...");
                StartCoroutine(RetryLoadAd());
            }
            else
            {
                Debug.LogError($"Failed to load ad after {maxRetries} attempts. Button will remain disabled.");
                _showAdButton.interactable = false;
            }
        }
    }

    /// <summary>
    /// Coroutine to retry loading the ad after a delay
    /// </summary>
    private IEnumerator RetryLoadAd()
    {
        yield return new WaitForSeconds(retryDelay);
        Debug.Log("Retrying ad load...");
        Advertisement.Load(_adUnitId, this);
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
        
        if (adUnitId.Equals(_adUnitId))
        {
            _showAdButton.interactable = true;
            // Try to load another ad
            LoadAd();
        }
    }

    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }

    void OnDestroy()
    {
        // Clean up the button listeners:
        _showAdButton.onClick.RemoveAllListeners();
        StopAllCoroutines();
    }

    private void OnEnable()
    {
        LoadAd();
    }
}