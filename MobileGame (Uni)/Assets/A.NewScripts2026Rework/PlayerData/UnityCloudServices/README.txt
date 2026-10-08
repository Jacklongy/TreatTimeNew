UnityCloudServices
==================

ProfileSyncService   Static class (no scene object needed). Keeps the device profile and the cloud profile in step.

  TryGoOnlineAsync      Quick internet check, then sign in, then confirm Cloud Save is ready. True/false, never throws.
  ForceOffline          Debug switch (set by SplashStartupController.forceOfflineThisSession). Makes HasInternet and
                        TryGoOnlineAsync fail so offline behaviour can be tested.
  The Console logs "[Sync] local rev ... cloud rev ... -> Push/Pull/None" on every startup sync.

  SyncAsync (on startup) - RunSyncAsync steps:
    1. Go online.
    2. Download the cloud copy (nothing there yet is fine; a failed download stops everything).
    3. Get the device copy (null if no save file).
    4. Decide (see table).
    5. Do it: pull (replace device copy with cloud), push (upload device copy), or nothing.

    Cloud empty                  -> Push (if there is a device profile)
    Device has no save           -> Pull
    Neither changed              -> Nothing
    Only device changed          -> Push        (you played offline)
    Only cloud changed           -> Pull        (you played on another device)
    Both changed (conflict)      -> keep the one with more TotalXP; a tie keeps the device copy
    "Device changed" = SaveRevision > SyncedRevision. "Cloud changed" = cloud SaveRevision > device SyncedRevision.

  SaveAndSyncAsync (every game save):
    1. Save to the device (always).
    2. If online, upload. If the upload fails the profile stays "unsynced" and goes up on a later save or startup.

  PushCurrentAsync notes the revision before uploading, because the game may save again mid-upload.
  RunExclusiveAsync ensures only one sync/upload runs at a time.

CloudDataAndAuth/    The two low-level building blocks ProfileSyncService uses (cloud storage + sign-in).
