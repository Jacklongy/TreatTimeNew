using System;

/// <summary>
/// Serializable local player data. It contains values only; managers and save services own the behavior.
/// </summary>
[Serializable]
public class PlayerDataLocal
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

    public PlayerDataLocal()
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
    }
}
