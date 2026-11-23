using TMPro;
using UnityEngine;

/// <summary>
/// Displays the persisted profile's coin total in the main menu.
/// </summary>
public class MainMenuCoinDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text coinsText;

    private void OnEnable()
    {
        ShopManager.OnCurrencyChanged += Refresh;
        TaskProgressManager.Changed += Refresh;
    }

    private void OnDisable()
    {
        ShopManager.OnCurrencyChanged -= Refresh;
        TaskProgressManager.Changed -= Refresh;
    }

    /// <summary>
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
