NewScriptArchitecture
=====================

Game logic, split by where it runs:

LivesInGame/            Only exists in the game scene (one run of play).
LivesInMenu/            Only exists in the main menu scene.
SingletonAcrossScenes/  Shared by every scene (scene changing, XP curve).

Rule: if a script needs the saved player profile, it reads it from PlayerProfileHolder
(see PlayerData/LocalSaving). It never reads the save file directly.
