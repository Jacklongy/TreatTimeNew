using UnityEngine;
using TMPro;

/// <summary>
/// Owns the single run score total. GridScoringSystem and DogFeeder both report
/// earned score straight into CurrentScore. GameOverManager reads it when the run ends.
/// </summary>
public class NewScoreManager : MonoBehaviour
{
    public static NewScoreManager Instance { get; private set; }

    [Header("Display")]
    [SerializeField] private TextMeshProUGUI scoreTextDisplay;

    public int CurrentScore { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ResetRun();
    }

    /// <summary>
    /// Adds earned score to the run total, from any source (grid ticks, feeding, etc).
    /// </summary>
    public void AddScore(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        CurrentScore += amount;
        RefreshScoreDisplay();
    }

    /// <summary>
    /// Resets the score for a new run.
    /// </summary>
    public void ResetRun()
    {
        CurrentScore = 0;
        RefreshScoreDisplay();
    }

    private void RefreshScoreDisplay()
    {
        if (scoreTextDisplay != null)
        {
            scoreTextDisplay.text = CurrentScore.ToString("N0");
        }
    }
}
