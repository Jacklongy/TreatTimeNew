A.NewScripts2026Rework - START HERE
===================================

Everything for the 2026 rework lives under this folder. The only thing it still uses from outside is
SoundManagerScript (called by NewRunManager for music).

FOLDER MAP
----------
PlayerData/              Who the player is and how their progress is saved (local file + cloud).
NewScriptArchitecture/   Scene-by-scene game logic: the run, the main menu, and objects shared by all scenes.
Ads/                     Empty for now. Ad scripts will go here.

Each folder has its own README.txt explaining its scripts.

HOW THE GAME FLOWS (scene order)
--------------------------------
1. Splash scene
   SplashStartupController -> loads the saved profile, goes online, syncs with the cloud, then either
   loads the menu (returning player) or shows SignUpScreenController (new player).
2. Main menu scene
   MainMenuBootstrap fills in the player/dog names, MainMenuXPDisplay shows level and XP.
   Pressing Start calls MainMenuBootstrap.StartGame, which saves and loads the game scene.
3. Game scene
   NewRunManager loads the profile into the run. NewScoreManager and CoinManager collect score/coins.
   When the run ends GameOverManager shows the result, then NewRunManager.SaveRun writes it to the profile.
4. Back to the main menu (step 2).

WHERE PLAYER DATA LIVES
-----------------------
PlayerProfileData    the fields (coins, XP, names...). Just data.
PlayerProfileHolder  the one live copy in memory during play.
LocalSaveStorage     writes/reads that copy to player_data.json on the device.
CloudSaveStorage     writes/reads that copy as text in Unity Cloud Save.
ProfileSyncService   decides whether local or cloud is newer and copies one over the other.
UnityAuthService     signs the player in (needed before the cloud can be used).

Rule of thumb: every save goes to the device first, then to the cloud if online.

NAMES CHANGED IN THE RENAME (old -> new)
----------------------------------------
SplashAccountController  -> SplashStartupController
AccountSetupController   -> SignUpScreenController
PlayerDataLocal          -> PlayerProfileData
PlayerDataManager        -> PlayerProfileHolder
LocalSaveService         -> LocalSaveStorage
CloudData                -> CloudSaveStorage
PlayerAuth               -> UnityAuthService
