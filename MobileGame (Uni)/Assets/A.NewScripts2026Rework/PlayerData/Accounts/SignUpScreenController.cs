using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles account setup for players without a profile name: the Guest/Apple/Google choice and guest name entry.
/// It doesn't decide where the player goes next. It raises events and SplashStartupController reacts.
///
/// Guest path:    Begin -> provider choice -> Guest -> name entry -> Confirm -> GuestSetupCompleted
/// Provider path: Begin -> provider choice -> Apple/Google -> ProviderSignedIn
/// </summary>
public class SignUpScreenController : MonoBehaviour
{
    /// <summary>Raised after Apple or Google sign-in succeeds. The listener should sync and decide what's next.</summary>
    public event Action ProviderSignedIn;

    /// <summary>Raised after a guest's names are saved. The profile is ready to play.</summary>
    public event Action GuestSetupCompleted;

    [Header("Sign-In Provider Toggles")]
    public bool appleSignInEnabled;
    public bool googleSignInEnabled;

    [Header("Provider Choice UI")]
    [SerializeField] private GameObject providerChoicePanel;
    [SerializeField] private Button guestButton;
    [SerializeField] private Button appleSignInButton;
    [SerializeField] private Button googleSignInButton;

    [Header("Name Entry UI")]
    [SerializeField] private GameObject nameEntryPanel;
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TMP_InputField dogNameInput;
    [SerializeField] private Button confirmButton;
    [SerializeField] private TMP_Text statusText;

    #region Setup

    // Awake, not Start: the splash may call Begin before this object's Start has run.
    private void Awake()
    {
        providerChoicePanel.SetActive(false);
        nameEntryPanel.SetActive(false);

        guestButton.onClick.AddListener(ShowNameEntry);
        appleSignInButton.onClick.AddListener(OnAppleSignInPressed);
        googleSignInButton.onClick.AddListener(OnGoogleSignInPressed);
        confirmButton.onClick.AddListener(OnConfirmPressed);
    }

    #endregion

    #region Screens

    /// <summary>Shows the guest/Apple/Google choice, hiding providers not enabled for this build.</summary>
    public void Begin()
    {
        appleSignInButton.gameObject.SetActive(appleSignInEnabled);
        googleSignInButton.gameObject.SetActive(googleSignInEnabled);

        nameEntryPanel.SetActive(false);
        providerChoicePanel.SetActive(true);
    }

    /// <summary>Shows the player and dog name inputs.</summary>
    public void ShowNameEntry()
    {
        providerChoicePanel.SetActive(false);
        nameEntryPanel.SetActive(true);
    }

    #endregion

    #region Provider Sign-In

    private void OnAppleSignInPressed()
    {
        SignInWithProviderAsync(UnityAuthService.Instance.TrySignInWithAppleAsync, "Apple");
    }

    private void OnGoogleSignInPressed()
    {
        SignInWithProviderAsync(UnityAuthService.Instance.TrySignInWithGoogleAsync, "Google");
    }

    /// <summary>
    /// Shared provider flow:
    ///   1. Needs internet.
    ///   2. Sign in with the provider (stubbed in UnityAuthService until the SDKs are added).
    ///   3. Hide the choice panel and raise ProviderSignedIn.
    /// </summary>
    private async void SignInWithProviderAsync(Func<Task<bool>> signIn, string providerName)
    {
        try
        {
            if (!ProfileSyncService.HasInternet)
            {
                ShowStatus($"Connect to the internet to sign in with {providerName}.");
                return;
            }

            if (!await signIn())
            {
                ShowStatus($"{providerName} sign-in isn't set up yet.");
                return;
            }

            providerChoicePanel.SetActive(false);
            ProviderSignedIn?.Invoke();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            ShowStatus($"{providerName} sign-in failed. Please try again.");
        }
    }

    #endregion

    #region Guest Name Entry

    /// <summary>
    /// Finishes guest sign-up:
    ///   1. Validate the names.
    ///   2. Write them into the profile.
    ///   3. If online, set the Unity account name.
    ///   4. Save locally, then upload if online (offline profiles upload on a later launch).
    ///   5. Raise GuestSetupCompleted.
    /// </summary>
    private async void OnConfirmPressed()
    {
        string playerName = playerNameInput.text.Trim();
        string dogName = dogNameInput.text.Trim();

        if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(dogName))
        {
            ShowStatus("Please enter both names.");
            return;
        }

        confirmButton.interactable = false;
        ShowStatus("Setting up your account...");

        try
        {
            PlayerProfileHolder.Instance.CurrentData.playername = playerName;
            PlayerProfileHolder.Instance.CurrentData.DogName = dogName;

            if (await ProfileSyncService.TryGoOnlineAsync())
            {
                await UnityAuthService.Instance.SetPlayerNameAsync(playerName);
            }

            await ProfileSyncService.SaveAndSyncAsync();

            GuestSetupCompleted?.Invoke();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            ShowStatus("Something went wrong. Please try again.");
            confirmButton.interactable = true;
        }
    }

    #endregion

    private void ShowStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}
