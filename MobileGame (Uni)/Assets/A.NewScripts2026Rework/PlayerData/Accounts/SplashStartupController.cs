using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The first script that runs. It decides where the player goes: straight to the menu, or through account setup.
///
/// Startup steps (see RunStartupAsync):
///   1. Apply debug options (reset data, force offline).
///   2. Hide the offline notice.
///   3. Load the profile saved on this device (or default data for a new player).
///   4. Try to go online (init Unity Services + sign in).
///   5. Online:  sync with the cloud (see ProfileSyncService).
///      Offline: show the offline notice and wait for the player to press Continue.
///   6. Route: a profile with a name goes to the menu, otherwise SignUpScreenController takes over.
///
/// Being offline never blocks the player. Progress is saved locally and uploaded next time they are online.
/// </summary>
public class SplashStartupController : MonoBehaviour
{
    [Header("Debug")]
    [Tooltip("In editor and development builds, delete the local profile and disable cloud sync for this session. The cloud backup is not deleted.")]
    public bool resetDataThisSession;

    [Tooltip("Pretends there is no internet so the offline flow can be tested. Only works in the editor and development builds.")]
    public bool forceOfflineThisSession;

    [Header("References")]
    [SerializeField] private SignUpScreenController accountSetup;
    [SerializeField] private TMP_Text statusText;

    [Header("Offline Notice UI (optional)")]
    [SerializeField] private GameObject offlinePanel;
    [SerializeField] private Button continueButton;

    [Header("Scene Flow")]
    [SerializeField] private string menuSceneName = "MainMenu";

    // True once Unity Services is initialised and the player is signed in.
    private bool isOnline;

    // Completed when the player presses Continue on the offline notice.
    private TaskCompletionSource<bool> offlineNoticeAcknowledged;

    #region Startup

    private void OnEnable()
    {
        accountSetup.ProviderSignedIn += OnProviderSignedIn;
        accountSetup.GuestSetupCompleted += GoToMenu;

        if (continueButton != null)
        {
            continueButton.onClick.AddListener(OnContinuePressed);
        }
    }

    private void OnDisable()
    {
        accountSetup.ProviderSignedIn -= OnProviderSignedIn;
        accountSetup.GuestSetupCompleted -= GoToMenu;

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinuePressed);
        }
    }

    private async void Start()
    {
        try
        {
            await RunStartupAsync();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            ShowStatus("Something went wrong. Please restart the game.");
        }
    }

    private async Task RunStartupAsync()
    {
        // Step 1: debug options.
        ApplyDebugOffline();
        ApplyDebugReset();

        // Step 2: hide the offline notice.
        SetOfflinePanelVisible(false);

        // Step 3: local profile.
        LoadLocalProfile();

        if (ProfileSyncService.CloudSyncDisabledForSession)
        {
            RouteAfterSignIn();
            return;
        }

        // Step 4: try to go online.
        ShowStatus("Connecting...");
        isOnline = await ProfileSyncService.TryGoOnlineAsync();

        // Step 5: cloud sync when online, otherwise tell the player and wait for Continue.
        if (isOnline)
        {
            await SyncWithCloudAsync();
        }
        else
        {
            await ShowOfflineNoticeAsync();
        }

        // Step 6: menu or account setup.
        RouteAfterSignIn();
    }

    private void ApplyDebugOffline()
    {
        ProfileSyncService.ForceOffline = forceOfflineThisSession && Debug.isDebugBuild;

        if (ProfileSyncService.ForceOffline)
        {
            Debug.LogWarning("[Splash] Forced offline mode is ON. Cloud sync is disabled.");
        }
    }

    private void ApplyDebugReset()
    {
        ProfileSyncService.BeginSession(resetDataThisSession && Debug.isDebugBuild);

        if (!resetDataThisSession || !Debug.isDebugBuild)
        {
            return;
        }

        if (LocalSaveStorage.Instance == null || !LocalSaveStorage.Instance.DeleteSave())
        {
            throw new InvalidOperationException("Could not reset the local player profile.");
        }

        PlayerProfileHolder.Instance.ResetData();
        Debug.LogWarning("[Splash] Local profile reset. Cloud sync is disabled for this session; the cloud backup is unchanged.");
    }

    /// <summary>Loads player_data.json into PlayerProfileHolder, or starts from default data if there is no file.</summary>
    private void LoadLocalProfile()
    {
        if (LocalSaveStorage.Instance == null)
        {
            throw new InvalidOperationException("LocalSaveStorage is missing.");
        }

        if (!LocalSaveStorage.Instance.HasSaveData())
        {
            PlayerProfileHolder.Instance.ResetData();
            return;
        }

        if (!LocalSaveStorage.Instance.Load())
        {
            throw new InvalidOperationException("The existing local player profile could not be loaded.");
        }
    }

    /// <summary>Pulls or pushes the profile as needed, then makes sure the Unity account name matches.</summary>
    private async Task SyncWithCloudAsync()
    {
        ShowStatus("Syncing...");
        if (!await ProfileSyncService.SyncAsync())
        {
            Debug.LogWarning("[Splash] Cloud sync did not complete. Continuing with the local profile.");
        }

        // The name may have been created offline, or just pulled from the cloud.
        string playerName = PlayerProfileHolder.Instance.CurrentData.playername;
        if (!ProfileSyncService.CloudSyncDisabledForSession && !string.IsNullOrWhiteSpace(playerName))
        {
            _ = UnityAuthService.Instance.SetPlayerNameAsync(playerName);
        }
    }

    /// <summary>Returning players go to the menu; everyone else starts account setup.</summary>
    private void RouteAfterSignIn()
    {
        if (!HasProfileName())
        {
            accountSetup.Begin();
            return;
        }

        GoToMenu();
    }

    private bool HasProfileName()
    {
        return !string.IsNullOrWhiteSpace(PlayerProfileHolder.Instance.CurrentData.playername);
    }

    #endregion

    #region Offline Notice

    /// <summary>
    /// Shows the offline notice and waits for Continue.
    /// Skipped if the notice UI isn't assigned, so a missing panel can never block the player.
    /// </summary>
    private async Task ShowOfflineNoticeAsync()
    {
        ShowStatus("");

        if (offlinePanel == null || continueButton == null)
        {
            return;
        }

        offlineNoticeAcknowledged = new TaskCompletionSource<bool>();
        SetOfflinePanelVisible(true);

        await offlineNoticeAcknowledged.Task;

        SetOfflinePanelVisible(false);
    }

    private void OnContinuePressed()
    {
        offlineNoticeAcknowledged?.TrySetResult(true);
    }

    private void SetOfflinePanelVisible(bool visible)
    {
        if (offlinePanel != null)
        {
            offlinePanel.SetActive(visible);
        }
    }

    #endregion

    #region Account Setup Events

    /// <summary>
    /// After Apple/Google sign-in the account may already have a cloud profile, so sync first.
    /// Then go to the menu if the profile has a name, otherwise ask for names.
    /// </summary>
    private async void OnProviderSignedIn()
    {
        try
        {
            isOnline = true;
            await SyncWithCloudAsync();

            if (HasProfileName())
            {
                GoToMenu();
            }
            else
            {
                accountSetup.ShowNameEntry();
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            ShowStatus("Something went wrong. Please restart the game.");
        }
    }

    #endregion

    private void GoToMenu()
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadScene(menuSceneName);
        }
        else
        {
            Debug.LogError("[Splash] Cannot open the menu because SceneController is missing.");
            ShowStatus("Unable to load the menu. Please restart the game.");
        }
    }

    private void ShowStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}
