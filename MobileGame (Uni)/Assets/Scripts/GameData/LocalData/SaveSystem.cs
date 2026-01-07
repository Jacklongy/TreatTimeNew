using UnityEngine;
using System.IO;
using System;

public static class SaveSystem
{
    private static string GetSavePath() => Path.Combine(Application.persistentDataPath, "Player.json");

    public static void Save(GameOverseer overseer)
    {
        try
        {
            PlayerData data = new PlayerData(overseer);
            string json = JsonUtility.ToJson(data, true); // true = pretty print for debugging
            string path = GetSavePath();

            File.WriteAllText(path, json);
            Debug.Log("Save file created at: " + path);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to save game data: " + e.Message);
        }
    }

    public static PlayerData LoadData()
    {
        try
        {
            string path = GetSavePath();

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                PlayerData data = JsonUtility.FromJson<PlayerData>(json);

                if (data != null)
                {
                    Debug.Log("Game data loaded successfully from: " + path);
                    return data;
                }
                else
                {
                    Debug.LogError("Failed to deserialize player data. Save file may be corrupted.");
                    return CreateDefaultData();
                }
            }
            else
            {
                Debug.LogWarning("Save file not found at path: " + path);
                Debug.LogWarning("Persistent Data Path: " + Application.persistentDataPath);
                return CreateDefaultData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading game data: " + e.Message);
            return CreateDefaultData();
        }
    }

    /// <summary>
    /// Creates default data for new players to prevent null reference crashes.
    /// </summary>
    private static PlayerData CreateDefaultData()
    {
        return new PlayerData
        {
            DogName = "Default",
            Coins = 0,
            TotalXP = 0f,
            CurrentLevel = 0,
            PreviousLvlXP = 0f,
            NextLevelXP = 30,
            Gems = 0,
            PlayerRep = 0f,
            hasPlayed = 0,
            HighScoreTimed = 0,
            HighScoreUnlim = 0
        };
    }
}
