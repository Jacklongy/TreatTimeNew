using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Reads and writes PlayerProfileData to the device's persistent local storage.
/// </summary>
public class LocalSaveStorage : MonoBehaviour
{
    public static LocalSaveStorage Instance;

    private const string SaveFileName = "player_data.json";

    private string SavePath
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, SaveFileName);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Saves the current profile owned by PlayerProfileHolder.
    /// </summary>
    public bool Save()
    {
        if (PlayerProfileHolder.Instance == null || PlayerProfileHolder.Instance.CurrentData == null)
        {
            Debug.LogError("Cannot save because PlayerProfileHolder has no current data.");
            return false;
        }

        return Save(PlayerProfileHolder.Instance.CurrentData);
    }

    /// <summary>
    /// Saves a supplied profile as formatted JSON on the device.
    /// Pass markModified false only when persisting sync bookkeeping, so the save isn't counted as a new change.
    /// </summary>
    public bool Save(PlayerProfileData data, bool markModified = true)
    {
        if (data == null)
        {
            Debug.LogError("Cannot save null player data.");
            return false;
        }

        try
        {
            if (markModified)
            {
                data.SaveRevision++;
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to save player data: {exception.Message}");
            return false;
        }
    }

    /// <summary>
    /// Loads a profile and gives it to PlayerProfileHolder.
    /// </summary>
    public bool Load()
    {
        if (!File.Exists(SavePath))
        {
            return false;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            PlayerProfileData data = JsonUtility.FromJson<PlayerProfileData>(json);

            if (data == null)
            {
                Debug.LogWarning("Player data file was empty or invalid.");
                return false;
            }

            if (PlayerProfileHolder.Instance == null)
            {
                Debug.LogError("Cannot load because PlayerProfileHolder is missing.");
                return false;
            }

            PlayerProfileHolder.Instance.SetData(data);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to load player data: {exception.Message}");
            return false;
        }
    }

    /// <summary>
    /// Returns whether a local profile file exists on the device.
    /// </summary>
    public bool HasSaveData()
    {
        return File.Exists(SavePath);
    }

    /// <summary>
    /// Deletes the local profile, primarily for testing and account reset flows.
    /// </summary>
    public bool DeleteSave()
    {
        if (!File.Exists(SavePath))
        {
            return true;
        }

        try
        {
            File.Delete(SavePath);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError($"Failed to delete player data: {exception.Message}");
            return false;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
