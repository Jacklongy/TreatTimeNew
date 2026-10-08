using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the current run's state and saves it to the player's profile when the run ends.
/// </summary>
public class NewRunManager : MonoBehaviour
{
    public static NewRunManager Instance { get; private set; }

    [SerializeField] private XPManager xpManager;
    [SerializeField] private CoinManager coinManager;

    [Header("Run Results")]
    public int CurrentRunScore;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (PlayerProfileHolder.Instance == null)
        {
            Debug.LogError("PlayerProfileHolder missing.");
            return;
        }

        LoadProfileIntoGameRun();

        // Play Game Music 
        FindObjectOfType<SoundManagerScript>().StopPlaying("LobbyMusic");
        FindObjectOfType<SoundManagerScript>().Play("Treat Time Music");
    }

    /// <summary>
    /// Initializes run systems from the persistent profile.
    /// </summary>
    public void LoadProfileIntoGameRun()
    {
        var data = PlayerProfileHolder.Instance.CurrentData;

        if (xpManager != null)
        {
            xpManager.InitializeFromProfile(data.CurrentLevel, data.TotalXP);
        }
    }


    /// <summary>
    /// Adds this run's rewards to the profile and persists the updated profile.
    /// </summary>
    public void SaveRun()
    {
        if (PlayerProfileHolder.Instance == null)
            return;

        var data = PlayerProfileHolder.Instance.CurrentData;
        data.HighScoreTimed = Mathf.Max(data.HighScoreTimed, CurrentRunScore);

        if (coinManager != null)
        {
            data.Coins += coinManager.CoinsEarnedThisRun;
        }

        if (xpManager != null)
        {
            data.TotalXP = xpManager.ProfileXPAfterRun;
            data.CurrentLevel = xpManager.ProfileLevelAfterRun;
        }

        // Saves locally right away; the cloud push runs in the background when online.
        _ = ProfileSyncService.SaveAndSyncAsync();
    }
}
