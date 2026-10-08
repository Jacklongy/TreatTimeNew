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
        if (!IsReady)
        {
            return false;
        }

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
        if (!IsReady)
        {
            return new CloudLoadResult(CloudLoadStatus.Failed);
        }

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
