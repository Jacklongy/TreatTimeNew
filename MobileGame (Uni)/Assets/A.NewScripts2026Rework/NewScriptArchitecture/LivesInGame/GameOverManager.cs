using UnityEngine;

/// <summary>
/// Coordinates the end of a run and commits its results when the player exits.
/// </summary>
public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [SerializeField] private XPManager xpManager;
    [SerializeField] private CoinManager coinManager;
    [SerializeField] private GameOverScreenUiController gameOverScreen;
    [SerializeField] private AdRewardManager adRewardManager;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool runHasEnded;

    private void Awake()
    {
        Instance = this;

        if (gameOverScreen != null)
        {
            gameOverScreen.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Finalizes reward calculations and presents the completed run to the player.
    /// </summary>
    public void EndRun(int finalScore)
    {
        if (runHasEnded)
        {
            return;
        }

        if (NewRunManager.Instance == null || xpManager == null || coinManager == null)
        {
            Debug.LogError("Game over cannot start because a required run manager is missing.");
            return;
        }

        runHasEnded = true;
        NewRunManager.Instance.CurrentRunScore = finalScore;
        xpManager.TakeScore(finalScore);

        Time.timeScale = 0f;

        if (gameOverScreen != null)
        {
            gameOverScreen.gameObject.SetActive(true);
            gameOverScreen.ShowResults(
                finalScore,
                coinManager.CoinsEarnedThisRun,
                xpManager.XPGainedThisRun,
                xpManager.LevelAtRunStart,
                xpManager.ProfileLevelAfterRun);
        }
    }

    /// <summary>
    /// Saves the completed run and returns the player to the main menu.
    /// </summary>
    public void ExitToMenu()
    {
        if (!runHasEnded)
        {
            return;
        }

        NewRunManager.Instance?.SaveRun();
        Time.timeScale = 1f;

        if (SceneController.Instance == null)
        {
            Debug.LogError("SceneController is missing.");
            return;
        }

        SceneController.Instance.LoadScene(mainMenuSceneName);
    }

    /// <summary>
    /// Requests an ad-granted reward for the completed run (e.g. bonus coins, revive).
    /// </summary>
    public void WatchAdForReward()
    {
        adRewardManager?.GrantReward();
    }
}
