Accounts
========

The splash scene. Two scripts, split by responsibility:

SplashStartupController   "The boss": runs the startup steps and decides where the player goes.
SignUpScreenController    "The screens": provider choice (Guest / Apple / Google) and guest name entry.

SplashStartupController - RunStartupAsync steps
  1. Debug options: resetDataThisSession wipes the local save; forceOfflineThisSession pretends there is no
     internet (editor/dev builds only) so you can test the offline flow.
  2. Hide the offline notice.
  3. Load the profile saved on the device (or blank data for a new player).
  4. Try to go online (start Unity Services and sign in).
  5. Online:  ProfileSyncService.SyncAsync, then make sure the Unity account name matches the profile.
     Offline: show the offline notice and wait for the player to press Continue (skipped if the notice UI
     isn't assigned, so it can never block the player).
  6. Route:
       profile has a name -> load the main menu
       no name            -> SignUpScreenController.Begin()

SignUpScreenController - what it does
  Begin()           shows Guest / Apple / Google buttons (Apple and Google only if enabled by the toggles).
  ShowNameEntry()   shows the player name and dog name inputs.
  Guest + Confirm   saves the names into the profile, sets the Unity account name if online, saves locally then
                    to the cloud if online, then raises GuestSetupCompleted.
  Apple / Google    need internet; sign in through UnityAuthService (stubs for now), then raise ProviderSignedIn.
  It never loads scenes itself. It raises events and the splash controller reacts:
    GuestSetupCompleted -> splash loads the menu.
    ProviderSignedIn    -> splash syncs (the account may already have a cloud profile), then menu or name entry.

INSPECTOR WIRING (splash scene)
  SplashStartupController: accountSetup, statusText, offlinePanel + continueButton (optional; the notice only
                           shows if both are set), menuSceneName.
                           The offline message is plain text on the panel's own text object. Edit it in Unity.
  SignUpScreenController:  provider panel + 3 buttons, name panel, 2 inputs, confirm button, status text.
