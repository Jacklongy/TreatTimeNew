using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Splash screen entry point. First shows a sign-in choice (Guest / Apple / Google),
/// then for guests, collects player name and dog name. Signs in anonymously and sets
/// the dashboard-visible player name. Returning players are signed in silently and
/// sent straight to the menu.
/// </summary>
public class SplashAccountController : MonoBehaviour
{
    [Header("Debug")]
    public bool resetDataThisSession;

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

    [Header("Scene Flow")]
    [SerializeField] private string menuSceneName = "MainMenu";


/// <summary>
/// It wont seem to sign in automatically for returning players on the development build.
/// </summary>
    private async void Start()
    {
        if (resetDataThisSession)
        {
            LocalSaveService.Instance.DeleteSave();
            PlayerDataManager.Instance.ResetData();
        }

        providerChoicePanel.SetActive(false);
        nameEntryPanel.SetActive(false);

        guestButton.onClick.AddListener(OnGuestPressed);
        appleSignInButton.onClick.AddListener(OnAppleSignInPressed);
        googleSignInButton.onClick.AddListener(OnGoogleSignInPressed);
        confirmButton.onClick.AddListener(OnConfirmPressed);

        bool loaded = LocalSaveService.Instance != null && LocalSaveService.Instance.Load();
        bool hasName = loaded && !string.IsNullOrWhiteSpace(PlayerDataManager.Instance.CurrentData.playername);

        if (hasName)
        {
            ShowStatus("Signing in...");
            await PlayerAuth.Instance.SignInAsync();
            GoToMenu();
            return;
        }

        if (!loaded)
        {
            PlayerDataManager.Instance.ResetData();
        }

        ShowProviderChoice();
    }

    /// <summary>Shows the guest/Apple/Google choice, hiding providers not enabled for this build.</summary>
    private void ShowProviderChoice()
    {
        appleSignInButton.gameObject.SetActive(appleSignInEnabled);
        googleSignInButton.gameObject.SetActive(googleSignInEnabled);
        providerChoicePanel.SetActive(true);
    }

    private void OnGuestPressed()
    {
        providerChoicePanel.SetActive(false);
        nameEntryPanel.SetActive(true);
    }

    // TODO: wire up once the Apple Game Center SDK is integrated.
    private void OnAppleSignInPressed()
    {
        ShowStatus("Apple sign-in isn't set up yet.");
    }

    // TODO: wire up once the Google Play Games SDK is integrated.
    private void OnGoogleSignInPressed()
    {
        ShowStatus("Google sign-in isn't set up yet.");
    }

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

        await PlayerAuth.Instance.SignInAsync();
        await PlayerAuth.Instance.SetPlayerNameAsync(playerName);

        PlayerDataManager.Instance.CurrentData.playername = playerName;
        PlayerDataManager.Instance.CurrentData.DogName = dogName;
        LocalSaveService.Instance.Save();



        // Save to cloud if available. Testing if it works! we havnt set up pulling from the cloud yet.
        if (CloudData.instance != null)
        {
            string json = JsonUtility.ToJson(PlayerDataManager.Instance.CurrentData);
            CloudData.instance.SaveData("PlayerProfile", json);
        }

        GoToMenu();
    }

    private void GoToMenu()
    {
        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadScene(menuSceneName);
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
