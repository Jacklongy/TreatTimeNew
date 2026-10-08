LivesInGame
===========

Scripts that live in the game scene and manage one run. Order of events:

START OF RUN
  NewRunManager.Start
    - Loads level and XP from the profile into XPManager (LoadProfileIntoGameRun).
    - Switches music from the lobby track to the game track.
  NewScoreManager.Start resets the score to 0.

DURING THE RUN
  NewScoreManager   Holds the run's score. Anything that scores calls AddScore(amount).
  CoinManager       Holds coins earned this run. Anything that rewards coins calls AddCoins(amount).
  Neither one touches the saved profile.

END OF RUN
  GameOverManager.EndRun(finalScore)
    - Runs once (runHasEnded guard).
    - Stores the score in NewRunManager, asks XPManager to convert score to XP (score x 0.1).
    - Pauses the game (Time.timeScale = 0) and shows the results screen.
  GameOverScreenUiController.ShowResults  Just fills in the text: score, coins, XP, level before/after.
  XPManager         Does the XP math and level-ups. Keeps "at run start" values so the screen can show before/after.
                    Uses XPCurveConfig (SingletonAcrossScenes/XPCurve) for the XP needed per level.

SAVING
  GameOverManager.ExitToMenu -> NewRunManager.SaveRun
    - Adds the run's coins, best score, XP and level to the profile.
    - Calls ProfileSyncService.SaveAndSyncAsync: saves to the device first, uploads to the cloud if online.
  Then SceneController loads the main menu and time resumes.

ADS (not finished)
  AdRewardManager   Reward cooldown. GrantReward() is a TODO.
  AdsManager        Empty placeholder for showing a rewarded ad on game over.
  GameOverManager.WatchAdForReward is the entry point the ad button should call.

SCRIPT LIST
  NewRunManager, NewScoreManager, CoinManager, XPManager, GameOverManager,
  GameOverScreenUiController, AdRewardManager, AdsManager
