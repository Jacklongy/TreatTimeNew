using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Threading.Tasks;
using System;

/// <summary>
/// This code was from the Unity Documentation, for implimenting Player authenication. 
/// Allows players to sign in via google play with an annyomous token. 
/// </summary>
public class PlayerAuth : MonoBehaviour
{
    async void Awake()
    {
        try
        {
            // Initialize Unity Services if not already initialized
            if (UnityServices.State == ServicesInitializationState.Uninitialized)
            {
                await UnityServices.InitializeAsync();
                Debug.Log("Unity Services Initialized.");
            }

            SetupEvents();

            // If the player is not signed in, try to sign them in
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
        }
    }

    void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Player successfully signed in.");
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            // Ensure token is available after sign-in
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
            ReauthenticateAsync(); // Call an async method to handle reauthentication
        };
    }

    private async Task EnsureTokenIsAvailable()
    {
        // Ensure token is available after the sign-in process is complete
        int attempts = 0;
        const int maxAttempts = 5;

        while (attempts < maxAttempts)
        {
            if (!string.IsNullOrEmpty(AuthenticationService.Instance.AccessToken))
            {
                Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");
                return;  // Exit if token is found
            }

            attempts++;
            Debug.LogWarning($"Access token missing. Attempt {attempts}/{maxAttempts}. Retrying...");
            await Task.Delay(500);  // Wait 500ms before retrying
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

            // After sign-in, ensure access token is retrieved
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
}
