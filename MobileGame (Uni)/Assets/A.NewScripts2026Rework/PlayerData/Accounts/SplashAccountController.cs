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
/// 
/// This is the first script that sort of runs so ill type the saving flow here.
/// - First, check if there is existing local save data.
/// - If not, reset the player data to default.
/// - Show the provider choice panel for sign-in.
/// - For guest sign-in, collect player and dog names.
/// - Sign in anonymously and save the names locally.
/// - For returning players with existing data, sign in silently and go to the menu.
/// This flow ensures that player data is always initialized and that returning players experience a seamless sign-in process.
/// - We now have cloud save integration planned for after local save if they are connected to the internet.
/// - If they are not connected to the internet, rely solely on the local save.
/// - when they log in next time, the system will attempt to use the cloud save if available, otherwise it will fall back to the local save.
/// - This ensures that the most recent progress is preserved across devices while maintaining a reliable local backup.
/// - plan to set this up early october when i have Ai back to assist with the integration. 
/// This will allow me to properly handle conflicts and ensure data consistency across local and cloud saves.
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

        // Sign in the player and set their name in the authentication system.
        await PlayerAuth.Instance.SignInAsync();
        await PlayerAuth.Instance.SetPlayerNameAsync(playerName);

        // then save the player and dog names locally. Player data manager is just the data structure. Holding the data.
        PlayerDataManager.Instance.CurrentData.playername = playerName;
        PlayerDataManager.Instance.CurrentData.DogName = dogName;

        // Local save service takes the current data from the player data manager and saves it locally.
        //  To a JSON file on the device.
        LocalSaveService.Instance.Save();

        // Plan going forward: integrate cloud save properly, By saving locally first, Check internet connectivity,
        //  and then sync with the cloud.

        // Check connectivity before attempting to save to the cloud.
        // if not show pop up warning... 
        // If successful, proceed with cloud save.
        // we then want to check again for connectivity and ensure the cloud save was successful.
        //  And take the recent local and save to cloud first.

        CloudSave();

        GoToMenu();
    }


    private void CloudSave()
    {
        if (CloudData.instance != null)
        {
            string json = JsonUtility.ToJson(PlayerDataManager.Instance.CurrentData);
            CloudData.instance.SaveData("PlayerProfile", json);
        }
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
