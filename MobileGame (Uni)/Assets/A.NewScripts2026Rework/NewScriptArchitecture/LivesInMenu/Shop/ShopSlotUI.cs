using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One slot in the shop grid. Place as many as you like, drag a ShopItemData onto each one.
/// Tap once to preview the hat, tap again to buy it (or equip it if owned). ShopManager does the buying.
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
    [SerializeField] private GameObject lockedOverlay;
    [SerializeField] private Color[] rarityColours = new Color[4]
    {
        Color.white, Color.cyan, new Color(0.7f, 0.3f, 1f), new Color(1f, 0.7f, 0.1f)
    };

    [Header("Animation (optional)")]
    [Tooltip("Triggers: Success, Locked, CantAfford, Equipped. Bools: IsLocked, IsSelected, IsOwned.")]
    [SerializeField] private Animator animator;

    private static readonly int SuccessHash = Animator.StringToHash("Success");
    private static readonly int LockedHash = Animator.StringToHash("Locked");
    private static readonly int CantAffordHash = Animator.StringToHash("CantAfford");
    private static readonly int EquippedHash = Animator.StringToHash("Equipped");
    private static readonly int IsLockedHash = Animator.StringToHash("IsLocked");
    private static readonly int IsSelectedHash = Animator.StringToHash("IsSelected");
    private static readonly int IsOwnedHash = Animator.StringToHash("IsOwned");

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnTapped);
    }

    private void OnEnable()
    {
        PlayerInventory.OnChanged += HandleChanged;
        ShopManager.OnCurrencyChanged += Refresh;
        shopManager.OnSelectionChanged += HandleSelectionChanged;
        Refresh();
    }

    private void OnDisable()
    {
        PlayerInventory.OnChanged -= HandleChanged;
        ShopManager.OnCurrencyChanged -= Refresh;
        shopManager.OnSelectionChanged -= HandleSelectionChanged;
    }

    private void HandleChanged(string _) => Refresh();

    private void HandleSelectionChanged(ShopItemData selected)
    {
        if (animator != null) animator.SetBool(IsSelectedHash, selected == item);
    }

    // First tap selects and previews. Tapping the selected slot again buys it, or equips it if owned.
    private void OnTapped()
    {
        if (shopManager.SelectedItem != item)
        {
            shopManager.SelectItem(item);
            if (shopManager.GetState(item) == ShopItemState.Locked) Play(LockedHash);
            return;
        }

        switch (shopManager.GetState(item))
        {
            case ShopItemState.Locked:
                Play(LockedHash);
                break;
            case ShopItemState.Owned:
                if (shopManager.TryEquipSelected()) Play(EquippedHash);
                break;
            default:
                PurchaseResult result = shopManager.TryBuySelected();
                if (result == PurchaseResult.Success) Play(SuccessHash);
                else if (result == PurchaseResult.NotEnoughCurrency) Play(CantAffordHash);
                break;
        }
    }

    private void Play(int triggerHash)
    {
        if (animator != null) animator.SetTrigger(triggerHash);
    }

    public void Refresh()
    {
        if (item == null || shopManager == null) return;

        if (iconImage != null) iconImage.sprite = item.Icon;
        if (nameText != null) nameText.text = item.DisplayName;
        if (rarityImage != null) rarityImage.color = rarityColours[(int)item.Rarity];

        ShopItemState state = shopManager.GetState(item);
        if (lockedOverlay != null) lockedOverlay.SetActive(state == ShopItemState.Locked);

        if (animator != null)
        {
            animator.SetBool(IsLockedHash, state == ShopItemState.Locked);
            animator.SetBool(IsOwnedHash, state == ShopItemState.Owned);
            animator.SetBool(IsSelectedHash, shopManager.SelectedItem == item);
        }

        if (priceText != null)
        {
            priceText.text = state switch
            {
                ShopItemState.Owned => "Owned",
                ShopItemState.Locked => $"Level {shopManager.GetRequiredLevel(item)}",
                _ => item.Price.ToString("N0")
            };
        }
    }
}
