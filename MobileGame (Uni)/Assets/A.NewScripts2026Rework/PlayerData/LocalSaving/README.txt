LocalSaving
===========

The profile in memory and the save file on the device.

PlayerProfileData   Plain data class: names, coins, gems, XP, level, high scores, settings, plus SaveRevision
                    and SyncedRevision (see PlayerData/README.txt). No behaviour.
                    HasUnsyncedChanges is true when SaveRevision > SyncedRevision.
                    TaskStatistics stores lifetime meal, score and per-tag merge counters. TaskPeriods
                    stores each period's selected task IDs, progress, claims and selected bonus.
                    The task manager initializes these fields when loading older profiles.

PlayerProfileHolder The one live PlayerProfileData for the session (CurrentData). Everything reads and edits it
                    here. SetData replaces it (after loading or a cloud pull), ResetData makes a blank one.

LocalSaveStorage    Reads and writes CurrentData as JSON at Application.persistentDataPath/player_data.json.
                    Save()                 bumps SaveRevision, then writes the file.
                    Save(data, false)      writes without bumping. Used only to record sync bookkeeping.
                    Load()                 reads the file into PlayerProfileHolder.
                    HasSaveData / DeleteSave  check or remove the file.

All three are scene objects that survive scene changes (DontDestroyOnLoad) and expose a static Instance.
Game code should call ProfileSyncService.SaveAndSyncAsync() to save, not LocalSaveStorage.Save() directly,
so the cloud upload isn't forgotten.
TaskProgressManager checkpoints progress periodically and on pause/game over. Selections and claims
are saved locally immediately before syncing; see NewScriptArchitecture/README.txt for setup.
