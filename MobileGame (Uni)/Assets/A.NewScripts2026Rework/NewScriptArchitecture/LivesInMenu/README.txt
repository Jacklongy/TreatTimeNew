LivesInMenu
===========

Scripts that live in the main menu scene.

MainMenuBootstrap
  Start: reloads the profile from the device (mainly so pressing Play directly from the menu scene in the
         editor still works), then fills in the player name, dog name and XP display.
  StartGame(): marks HasPlayed, saves (device first, then cloud if online), loads the game scene.

MainMenuXPDisplay
  Refresh(): reads level and TotalXP from the profile and updates the XP bar and level text.
  Uses the same XPCurveConfig as the game scene so the numbers match.
