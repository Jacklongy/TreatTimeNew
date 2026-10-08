CloudDataAndAuth
================

Low-level Unity Gaming Services wrappers. They don't decide anything; ProfileSyncService does.

UnityAuthService
  Starts Unity Services and signs the player in anonymously (guest account).
  SignInAsync()      throws if it fails (e.g. offline); the next call tries again.
  TrySignInAsync()   same but returns true/false instead of throwing.
  SetPlayerNameAsync Sets the dashboard-visible name; skips the call if the account already has that name.
  TrySignInWithGoogleAsync / TrySignInWithAppleAsync   placeholders that return false until the SDKs are added.
  Note: Unity Authentication must be signed in before Cloud Save works.

CloudSaveStorage
  Reads and writes plain text in Unity Cloud Save.
  SaveStringAsync(key, text)   true on success.
  LoadStringAsync(key)         Found (with text), NotFound (nothing saved yet) or Failed (couldn't reach the cloud).
  The difference between NotFound and Failed matters: NotFound allows the first upload, Failed must never
  trigger an overwrite.
  The profile is stored as one JSON string under the key "PlayerProfile" (key lives in ProfileSyncService).
  IsReady = Unity Services initialised and signed in.
