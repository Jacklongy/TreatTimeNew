using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;

/// <summary>Outcome of a cloud read. Separates "nothing saved yet" from "could not reach the cloud".</summary>
public enum CloudLoadStatus
{
    Found,
    NotFound,
    Failed
}

public readonly struct CloudLoadResult
{
    public readonly CloudLoadStatus Status;
    public readonly string Json;

    public CloudLoadResult(CloudLoadStatus status, string json = null)
    {
        Status = status;
        Json = json;
    }
}

/// <summary>
/// Thin wrapper over Unity Cloud Save that moves raw strings. Deciding what to save,
/// load or overwrite lives in ProfileSyncService. Sign-in and service init are owned by UnityAuthService.
/// </summary>
public class CloudSaveStorage : MonoBehaviour
{
    public static CloudSaveStorage instance;

    private const int RequestTimeoutMilliseconds = 15000;
    private bool writesBlocked;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    /// <summary>
    /// True when Unity Services is initialised and the player holds a session.
    /// Does not prove the network is reachable; the save/load calls report that.
    /// </summary>
    public bool IsReady =>
        UnityServices.State == ServicesInitializationState.Initialized
        && AuthenticationService.Instance.IsSignedIn;

    /// <summary>Writes one string value to the player's cloud data. Returns false on any failure.</summary>
    public async Task<bool> SaveStringAsync(string key, string value)
    {
        if (!IsReady || writesBlocked)
        {
            Debug.LogWarning("Cloud save unavailable; local changes remain pending.");
            return false;
        }

        Task<bool> request = SaveStringInternalAsync(key, value);
        if (await Task.WhenAny(request, Task.Delay(RequestTimeoutMilliseconds)) != request)
        {
            // The request cannot be cancelled and may still finish. Do not start another
            // cloud operation that could race with this write during this process.
            writesBlocked = true;
            Debug.LogWarning("Cloud save timed out. Cloud sync is disabled until the app restarts.");
            return false;
        }

        return await request;
    }

    private async Task<bool> SaveStringInternalAsync(string key, string value)
    {
        try
        {
            var data = new Dictionary<string, object> { { key, value } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Cloud save failed ({key}): {e.Message}");
            return false;
        }
    }

    /// <summary>Reads one string value from the player's cloud data.</summary>
    public async Task<CloudLoadResult> LoadStringAsync(string key)
    {
        if (!IsReady || writesBlocked)
        {
            Debug.LogWarning("Cloud load unavailable; keeping local data.");
            return new CloudLoadResult(CloudLoadStatus.Failed);
        }

        Task<CloudLoadResult> request = LoadStringInternalAsync(key);
        if (await Task.WhenAny(request, Task.Delay(RequestTimeoutMilliseconds)) != request)
        {
            Debug.LogWarning("Cloud load timed out; keeping local data.");
            return new CloudLoadResult(CloudLoadStatus.Failed);
        }

        return await request;
    }

    private async Task<CloudLoadResult> LoadStringInternalAsync(string key)
    {
        try
        {
            var loaded = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });

            if (loaded.TryGetValue(key, out var item))
            {
                return new CloudLoadResult(CloudLoadStatus.Found, item.Value.GetAs<string>());
            }

            return new CloudLoadResult(CloudLoadStatus.NotFound);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Cloud load failed ({key}): {e.Message}");
            return new CloudLoadResult(CloudLoadStatus.Failed);
        }
    }
}
