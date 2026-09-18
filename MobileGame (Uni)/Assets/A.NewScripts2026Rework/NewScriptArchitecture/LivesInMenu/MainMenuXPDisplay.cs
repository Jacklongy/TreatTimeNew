using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays the persisted profile's level/XP progress in the main menu.
/// Uses the same XPCurveConfig as XPManager so level thresholds match gameplay exactly.
/// </summary>
public class MainMenuXPDisplay : MonoBehaviour
{
    [Header("XP Curve")]
    [SerializeField] private XPCurveConfig xpCurveConfig;

    [Header("UI")]
    [SerializeField] private Image xpProgressFillImage;
    [SerializeField] private TMP_Text currentLevelText;
    [SerializeField] private TMP_Text xpProgressText;


    /// <summary>
    /// Reads the current profile and updates the level/XP display.
    /// Call this after PlayerDataManager has loaded/reset the profile.
    /// </summary>
    public void Refresh()
    {
        if (PlayerDataManager.Instance == null || xpCurveConfig == null)
        {

           
            return;
        }

        var data = PlayerDataManager.Instance.CurrentData;

        Debug.Log($"Current Level: {data.CurrentLevel}, Total XP: {data.TotalXP}");

        float xpForPreviousLevel = xpCurveConfig.GetXPForNextLevel(data.CurrentLevel - 1);
        float xpForNextLevel = xpCurveConfig.GetXPForNextLevel(data.CurrentLevel);

        float xpIntoLevel = data.TotalXP - xpForPreviousLevel;
        float xpNeededForLevel = Mathf.Max(1f, xpForNextLevel - xpForPreviousLevel);

        if (xpProgressFillImage != null)
        {
            xpProgressFillImage.fillAmount = Mathf.Clamp01(xpIntoLevel / xpNeededForLevel);
        }

        SetText(currentLevelText, $"{data.CurrentLevel}");
        SetText(xpProgressText, $"{Mathf.Max(0f, xpIntoLevel):N0}/{xpNeededForLevel:N0}XP");
    }

    private void SetText(TMP_Text targetText, string value)
    {
        if (targetText != null)
        {
            targetText.text = value;
        }
    }
}
