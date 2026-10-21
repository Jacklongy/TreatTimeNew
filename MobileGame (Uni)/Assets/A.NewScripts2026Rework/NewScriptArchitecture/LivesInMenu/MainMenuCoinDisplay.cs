using TMPro;
using UnityEngine;

/// <summary>
/// Displays the persisted profile's coin total in the main menu.
/// </summary>
public class MainMenuCoinDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text coinsText;

    /// <summary>
    /// Reads the current profile and updates the coin display.
    /// Call this after PlayerProfileHolder has loaded/reset the profile.
    /// </summary>
    public void Refresh()
    {
        if (PlayerProfileHolder.Instance == null || coinsText == null)
        {
            return;
        }

        coinsText.text = PlayerProfileHolder.Instance.CurrentData.Coins.ToString("N0");
    }
}
