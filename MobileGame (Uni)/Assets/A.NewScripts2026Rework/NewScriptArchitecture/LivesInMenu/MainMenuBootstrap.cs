using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// This script is responsible for bootstrapping(Booting up/ Initalising the game) the main menu.
/// It checks if the player data manager is present and loads the player data.
///  If no save file is found, it creates default data. 
/// It also handles starting the game and transitioning to the game scene.
/// </summary>
public class MainMenuBootstrap : MonoBehaviour // Checks the data exists before strapping the game. 
// E.g if the boot(game) has data, Strap the boot. Lol. I could re name this to like DataCheckAndLoad or something.
{
    [SerializeField] private MainMenuXPDisplay xpDisplay;

    [SerializeField] private TextMeshProUGUI PlayerNameText;
    [SerializeField] private TextMeshProUGUI DogNameText;

    /// <summary>
    /// Checks if the PlayerProfileHolder is present in the scene and loads the player data.
    /// If no save file is found, it creates default data.
    /// </summary>
    private void Start()
    {
        if (PlayerProfileHolder.Instance == null)
        {
            Debug.LogError("PlayerProfileHolder is missing from the scene.");
            return;
        }

        bool loaded = LocalSaveStorage.Instance != null && LocalSaveStorage.Instance.Load();

        if (!loaded)
        {
            PlayerProfileHolder.Instance.ResetData();
            Debug.Log("No save file found. Created default data.");
        }

        xpDisplay?.Refresh();

        if (PlayerNameText != null)
        {
            PlayerNameText.text = PlayerProfileHolder.Instance.CurrentData.playername;
        }

        if (DogNameText != null)
        {
            DogNameText.text = PlayerProfileHolder.Instance.CurrentData.DogName;
        }
    }


/// <summary>
/// Starts the game by setting the HasPlayed flag to true, saving the current profile, and loading the game scene.
/// </summary>
    public void StartGame()
    {
        if (PlayerProfileHolder.Instance == null)
            return;

        PlayerProfileHolder.Instance.CurrentData.HasPlayed = true;

        _ = ProfileSyncService.SaveAndSyncAsync();

        SceneController.Instance.LoadScene("TreatTimeNew");
    }
}
