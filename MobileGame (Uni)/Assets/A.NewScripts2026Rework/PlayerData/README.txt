PlayerData
==========

Everything about the player's identity and saved progress.

Accounts/             Splash startup flow and the sign-up screens.
LocalSaving/          The profile in memory and the save file on the device.
UnityCloudServices/   Sign-in, Cloud Save, and the logic that keeps cloud and device in step.

THE BIG IDEA
  - The device save (player_data.json) is the source of truth while playing. It is always written first.
  - The cloud copy is a backup that also carries progress between devices.
  - Offline play never loses anything: the profile is flagged "unsynced" and uploaded next time online.

HOW "UNSYNCED" IS TRACKED
  PlayerProfileData has two numbers:
    SaveRevision    +1 on every local save.
    SyncedRevision  the last SaveRevision known to be in the cloud.
  SaveRevision > SyncedRevision means "this device has progress the cloud hasn't seen".
  Revision numbers are used instead of timestamps so a wrong phone clock can't pick the wrong save.

READ IN THIS ORDER
  1. LocalSaving/PlayerProfileData
  2. LocalSaving/LocalSaveStorage
  3. Accounts/SplashStartupController
  4. UnityCloudServices/ProfileSyncService
  5. Accounts/SignUpScreenController
  6. UnityCloudServices/CloudDataAndAuth (CloudSaveStorage, UnityAuthService)

NOT BUILT YET
  - Google / Apple sign-in (stubs in UnityAuthService; buttons hidden by toggles on SignUpScreenController).
  - Handling a player switching cloud accounts: store the cloud player ID in the profile, link a guest account
    to Google/Apple instead of signing in over it, and ask "keep this device's progress or load the account's?".
