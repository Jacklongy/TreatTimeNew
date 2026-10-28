using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One slot in the shop grid. Place as many as you like, drag a ShopItemData onto each one.
/// Tapping it previews the hat; ShopManager does the buying.
/// </summary>
[RequireComponent(typeof(Button))]
public class ShopSlotUI : MonoBehaviour
{
    [SerializeField] private ShopItemData item;
    [SerializeField] private ShopManager shopManager;

    [Header("Display")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image rarityImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Color[] rarityColours = new Color[4]
    {
        Color.white, Color.cyan, new Color(0.7f, 0.3f, 1f), new Color(1f, 0.7f, 0.1f)
    };

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(() => shopManager.SelectItem(item));
    }

    private void OnEnable()
    {
        PlayerInventory.OnChanged += HandleChanged;
        ShopManager.OnCurrencyChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        PlayerInventory.OnChanged -= HandleChanged;
        ShopManager.OnCurrencyChanged -= Refresh;
    }

    private void HandleChanged(string _) => Refresh();

    public void Refresh()
    {
        if (item == null || shopManager == null) return;

        if (iconImage != null) iconImage.sprite = item.Icon;
        if (nameText != null) nameText.text = item.DisplayName;
        if (rarityImage != null) rarityImage.color = rarityColours[(int)item.Rarity];

        ShopItemState state = shopManager.GetState(item);
        if (canvasGroup != null) canvasGroup.alpha = state == ShopItemState.Locked ? 0.4f : 1f;

        if (priceText != null)
        {
            priceText.text = state switch
            {
                ShopItemState.Owned => "Owned",
                ShopItemState.Locked => $"Level {item.RequiredLevel}",
                _ => item.Price.ToString("N0")
            };
        }
    }
}
