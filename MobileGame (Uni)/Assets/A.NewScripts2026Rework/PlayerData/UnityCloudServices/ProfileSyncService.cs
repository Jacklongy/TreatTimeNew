using System;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Keeps the profile on this device (local) and the profile in Unity Cloud Save (cloud) consistent.
///
/// Rules:
///   - Local is always written first and is the fallback when the cloud can't be reached.
///   - Each local save bumps SaveRevision. After a successful upload, SyncedRevision catches up to it.
///     SaveRevision greater than SyncedRevision means "local has changes the cloud hasn't seen".
///   - On startup we compare local and cloud and either pull, push or do nothing (see Decide).
///   - Revisions are used instead of timestamps, so a wrong device clock can't pick the wrong save.
/// </summary>
public static class ProfileSyncService
{
    private const string CloudKey = "PlayerProfile";

    private static bool isSyncing;

    private enum SyncAction
    {
        None,
        Push,
        Pull
    }

    #region Connectivity

    /// <summary>Debug only: makes every online check fail so offline flows can be tested. Set by the splash controller.</summary>
    public static bool ForceOffline { get; set; }

    /// <summary>Quick device-level check. A real cloud call can still fail on a captive or dead network.</summary>
    public static bool HasInternet => !ForceOffline && Application.internetReachability != NetworkReachability.NotReachable;

    /// <summary>Checks connectivity, then signs in and confirms cloud saving is usable.</summary>
    public static async Task<bool> TryGoOnlineAsync()
    {
        if (!HasInternet || UnityAuthService.Instance == null || CloudSaveStorage.instance == null)
        {
            return false;
        }

        return await UnityAuthService.Instance.TrySignInAsync() && CloudSaveStorage.instance.IsReady;
    }

    #endregion

    #region Startup Sync

    /// <summary>
    /// Reconciles local and cloud profiles. Call once after going online on startup.
    /// Returns true if the cloud was reachable and local and cloud now agree.
    /// </summary>
    public static Task<bool> SyncAsync()
    {
        return RunExclusiveAsync(RunSyncAsync);
    }

    private static async Task<bool> RunSyncAsync()
    {
        // Step 1: make sure we can talk to the cloud.
        if (!await TryGoOnlineAsync())
        {
            return false;
        }

        // Step 2: download the cloud copy (null is fine, it means nothing is saved there yet).
        (bool downloaded, PlayerProfileData cloud) = await DownloadCloudProfileAsync();
        if (!downloaded)
        {
            return false;
        }

        // Step 3: get the copy saved on this device (null if this device has no save file).
        PlayerProfileData local = GetLocalProfile();

        // Step 4: decide which side is newer.
        SyncAction action = Decide(local, cloud);
        Debug.Log($"[Sync] local rev {local?.SaveRevision} (synced {local?.SyncedRevision}), cloud rev {cloud?.SaveRevision} -> {action}");

        // Step 5: carry out the decision.
        switch (action)
        {
            case SyncAction.Pull:
                ApplyCloudProfile(cloud);
                return true;

            case SyncAction.Push:
                return await PushCurrentAsync();

            default:
                return true;
        }
    }

    /// <summary>
    /// Downloads and parses the cloud profile.
    /// downloaded is false if the cloud couldn't be read, so the caller must not overwrite anything.
    /// profile is null when nothing has been saved to the cloud yet.
    /// </summary>
    private static async Task<(bool downloaded, PlayerProfileData profile)> DownloadCloudProfileAsync()
    {
        CloudLoadResult result = await CloudSaveStorage.instance.LoadStringAsync(CloudKey);

        switch (result.Status)
        {
            case CloudLoadStatus.NotFound:
                return (true, null);

            case CloudLoadStatus.Found:
                bool parsed = TryParse(result.Json, out PlayerProfileData profile);
                return (parsed, profile);

            default:
                return (false, null);
        }
    }

    /// <summary>Returns the profile loaded from this device's save file, or null if there isn't one.</summary>
    private static PlayerProfileData GetLocalProfile()
    {
        bool hasSaveFile = LocalSaveStorage.Instance != null && LocalSaveStorage.Instance.HasSaveData();
        return hasSaveFile ? PlayerProfileHolder.Instance.CurrentData : null;
    }

    /// <summary>
    /// Chooses which side wins. "Local changed" means unsynced local saves.
    /// "Cloud changed" means the cloud holds a revision newer than the last one this device synced.
    ///
    ///   Cloud empty               -> Push (if we have a local profile)
    ///   Local missing             -> Pull
    ///   Neither changed           -> Nothing
    ///   Only local changed        -> Push (e.g. you played offline)
    ///   Only cloud changed        -> Pull (e.g. you played on another device)
    ///   Both changed (conflict)   -> keep the one with more TotalXP; a tie keeps local
    /// </summary>
    private static SyncAction Decide(PlayerProfileData local, PlayerProfileData cloud)
    {
        if (cloud == null)
        {
            return local == null ? SyncAction.None : SyncAction.Push;
        }

        if (local == null)
        {
            return SyncAction.Pull;
        }

        bool localChanged = local.HasUnsyncedChanges;
        bool cloudChanged = cloud.SaveRevision > local.SyncedRevision;

        if (!localChanged && !cloudChanged) return SyncAction.None;
        if (localChanged && !cloudChanged) return SyncAction.Push;
        if (!localChanged && cloudChanged) return SyncAction.Pull;

        return local.TotalXP >= cloud.TotalXP ? SyncAction.Push : SyncAction.Pull;
    }

    /// <summary>Replaces the live profile with the cloud one and saves it to the device as already synced.</summary>
    private static void ApplyCloudProfile(PlayerProfileData cloud)
    {
        cloud.SyncedRevision = cloud.SaveRevision;
        PlayerProfileHolder.Instance.SetData(cloud);
        LocalSaveStorage.Instance.Save(cloud, false);
    }

    #endregion

    #region Saving

    /// <summary>
    /// Saves locally (always), then uploads to the cloud if a connection is available.
    /// If the upload fails the profile stays flagged as unsynced and uploads on a later save or startup.
    /// Returns true only if the cloud upload succeeded.
    /// </summary>
    public static async Task<bool> SaveAndSyncAsync()
    {
        // Step 1: save on the device. This is the part that must never fail silently.
        if (LocalSaveStorage.Instance == null || !LocalSaveStorage.Instance.Save())
        {
            return false;
        }

        // Step 2: skip the upload if we're offline.
        if (!await TryGoOnlineAsync())
        {
            return false;
        }

        // Step 3: upload (skipped if another sync is already running).
        return await RunExclusiveAsync(PushCurrentAsync);
    }

    /// <summary>Uploads the live profile and records which revision the cloud now holds.</summary>
    private static async Task<bool> PushCurrentAsync()
    {
        // Step 1: snapshot the revision and JSON now, because the game may save again while we upload.
        PlayerProfileData snapshot = PlayerProfileHolder.Instance.CurrentData;
        int pushedRevision = snapshot.SaveRevision;
        string json = JsonUtility.ToJson(snapshot);

        // Step 2: upload. On failure SyncedRevision is untouched, so the profile stays "unsynced".
        if (!await CloudSaveStorage.instance.SaveStringAsync(CloudKey, json))
        {
            return false;
        }

        // Step 3: record success. Use the current profile (it may have changed during the upload),
        // and only advance to the revision we actually uploaded.
        PlayerProfileData current = PlayerProfileHolder.Instance.CurrentData;
        current.SyncedRevision = Mathf.Max(current.SyncedRevision, pushedRevision);
        LocalSaveStorage.Instance.Save(current, false);
        return true;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Runs one sync or upload at a time, so two saves can't overwrite each other's revision bookkeeping.
    /// Returns false without running if another one is in progress.
    /// </summary>
    private static async Task<bool> RunExclusiveAsync(Func<Task<bool>> work)
    {
        if (isSyncing)
        {
            return false;
        }

        isSyncing = true;

        try
        {
            return await work();
        }
        finally
        {
            isSyncing = false;
        }
    }

    private static bool TryParse(string json, out PlayerProfileData data)
    {
        data = null;

        try
        {
            data = JsonUtility.FromJson<PlayerProfileData>(json);
        }
        catch (ArgumentException)
        {
            return false;
        }

        return data != null;
    }
}

#endregion Helpers