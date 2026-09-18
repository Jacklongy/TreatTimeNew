using TMPro;
using UnityEngine;

/// <summary>
/// Displays the completed run's results.
/// </summary>
public class GameOverScreenUiController : MonoBehaviour
{
    [Header("Result UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text coinsEarnedText;
    [SerializeField] private TMP_Text xpEarnedText;
    [SerializeField] private TMP_Text levelBeforeText;
    [SerializeField] private TMP_Text levelAfterText;

    /// <summary>
    /// Sets every displayed value for the completed run.
    /// </summary>
    public void ShowResults(
        int finalScore,
        int coinsEarned,
        float xpEarned,
        int startingLevel,
        int finalLevel)
    {
        SetText(scoreText, finalScore.ToString());
        SetText(coinsEarnedText, $"+{coinsEarned}");
        SetText(xpEarnedText, $"+{Mathf.RoundToInt(xpEarned)} XP");
        SetText(levelBeforeText, $"Level {startingLevel}");
        SetText(levelAfterText, $"Level {finalLevel}");
    }

    private void SetText(TMP_Text targetText, string value)
    {
        if (targetText == null)
        {
            return;
        }

        targetText.text = value;
    }
}