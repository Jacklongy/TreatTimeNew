using System;

/// <summary>
/// Serializable local player data. It contains values only; managers and save services own the behavior.
/// </summary>
[Serializable]
public class PlayerProfileData
{
    public string playername; 
    public string DogName;
    public int Coins;
    public int Gems;

    public float TotalXP;
    public int CurrentLevel;
    public float PreviousLevelXP;
    public float NextLevelXP;

    public int HighScoreTimed;
    public int HighScoreUnlimited;

    public bool HasPlayed;
    public bool VibrationsEnabled;

    // Sync bookkeeping, used instead of device clocks. Bumped on every local save.
    public int SaveRevision;
    // Last revision known to be stored in the cloud. SaveRevision > SyncedRevision means unsynced changes.
    public int SyncedRevision;

    public bool HasUnsyncedChanges => SaveRevision > SyncedRevision;

    public PlayerProfileData()
    {
        playername = "";
        DogName = "Default";
        Coins = 0;
        Gems = 0;
        TotalXP = 0f;
        CurrentLevel = 0;
        PreviousLevelXP = 0f;
        NextLevelXP = 30f;
        HighScoreTimed = 0;
        HighScoreUnlimited = 0;
        HasPlayed = false;
        VibrationsEnabled = true;
        SaveRevision = 0;
        SyncedRevision = 0;
    }
}
