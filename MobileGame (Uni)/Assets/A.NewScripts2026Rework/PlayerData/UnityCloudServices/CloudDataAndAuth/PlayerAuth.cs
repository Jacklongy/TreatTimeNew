using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Threading.Tasks;
using System;
using System.Linq;

/// <summary>
/// This code was from the Unity Documentation, for implimenting Player authenication. 
/// Allows players to sign in via google play with an annyomous token. 
/// </summary>
public class PlayerAuth : MonoBehaviour
{
    public static PlayerAuth Instance;

    private Task signInTask;
    private bool eventsInitialized;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Initializes Unity Services and signs in anonymously. Callers should await this
    /// before touching AuthenticationService.Instance. Safe to call multiple times.
    /// </summary>
    public Task SignInAsync()
    {
        return signInTask ??= SignInInternalAsync();
    }

    private async Task SignInInternalAsync()
    {
        try
        {
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
                Debug.Log("Unity Services Initialized.");
            }

            if (!eventsInitialized)
            {
                SetupEvents();
                eventsInitialized = true;
            }

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.Log("Player is not signed in, signing in anonymously...");
                await SignInAnonymouslyAsync();
            }
            else
            {
                Debug.Log("Player is already signed in.");
                await EnsureTokenIsAvailable();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Initialization or sign-in failed:");
            Debug.LogException(e);
            signInTask = null; // allow a retry
            throw;
        }
    }

    /// <summary>
    /// Sets the dashboard-visible player name for the signed-in account.
    /// The service rejects whitespace, so it's stripped before sending.
    /// </summary>
    public async Task<bool> SetPlayerNameAsync(string playerName)
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.LogWarning("Cannot set player name before signing in.");
            return false;
        }

        try
        {
            string sanitized = new string(playerName.Where(c => !char.IsWhiteSpace(c)).ToArray());
            await AuthenticationService.Instance.UpdatePlayerNameAsync(sanitized);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to set player name:");
            Debug.LogException(e);
            return false;
        }
    }

    void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Player successfully signed in.");
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
            EnsureTokenIsAvailable().Wait();
        };

        AuthenticationService.Instance.SignInFailed += (err) =>
        {
            Debug.LogError($"Sign-in failed: {err}");
        };

        AuthenticationService.Instance.SignedOut += () =>
        {
            Debug.Log("Player signed out.");
        };

        AuthenticationService.Instance.Expired += () =>
        {
            Debug.LogWarning("Player session expired. Reauthenticating...");
            ReauthenticateAsync();
        };
    }

    private async Task EnsureTokenIsAvailable()
    {
        int attempts = 0;
        const int maxAttempts = 5;

        while (attempts < maxAttempts)
        {
            if (!string.IsNullOrEmpty(AuthenticationService.Instance.AccessToken))
            {
                Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");
                return;
            }

            attempts++;
            Debug.LogWarning($"Access token missing. Attempt {attempts}/{maxAttempts}. Retrying...");
            await Task.Delay(500);
        }

        Debug.LogError("Access token is still missing after multiple attempts.");
    }

    private async void ReauthenticateAsync()
    {
        try
        {
            Debug.Log("Attempting reauthentication...");
            await SignInAnonymouslyAsync();
            Debug.Log("Reauthentication succeeded!");
        }
        catch (Exception ex)
        {
            Debug.LogError("Reauthentication failed:");
            Debug.LogException(ex);
        }
    }

    private async Task SignInAnonymouslyAsync()
    {
        try
        {
            Debug.Log("Signing in anonymously...");
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log("Anonymous sign-in succeeded!");
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            await EnsureTokenIsAvailable();
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError("Authentication failed:");
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError("Sign-in request failed:");
            Debug.LogException(ex);
        }

        Debug.Log($"IsSignedIn: {AuthenticationService.Instance.IsSignedIn}");
        Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
