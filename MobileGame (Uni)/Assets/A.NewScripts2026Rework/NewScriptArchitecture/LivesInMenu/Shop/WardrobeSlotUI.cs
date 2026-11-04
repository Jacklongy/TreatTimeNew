using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One owned hat in the wardrobe, spawned by WardrobeGrid. Tap once to preview it on the selected target,
/// tap again to equip it.
/// </summary>
[RequireComponent(typeof(Button))]
public class WardrobeSlotUI : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image rarityImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private GameObject equippedMark;
    [SerializeField] private Color[] rarityColours = new Color[4]
    {
        Color.white, Color.cyan, new Color(0.7f, 0.3f, 1f), new Color(1f, 0.7f, 0.1f)
    };

    [Header("Animation (optional)")]
    [Tooltip("Trigger: Equipped. Bools: IsSelected, IsEquipped.")]
    [SerializeField] private Animator animator;

    private static readonly int EquippedHash = Animator.StringToHash("Equipped");
    private static readonly int IsSelectedHash = Animator.StringToHash("IsSelected");
    private static readonly int IsEquippedHash = Animator.StringToHash("IsEquipped");

    private ShopItemData item;
    private ShopManager shopManager;
    private Wardrobe wardrobe;

    public ShopItemData Item => item;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnTapped);
    }

    /// <summary>Called by WardrobeGrid right after the slot is spawned.</summary>
    public void Setup(ShopItemData hat, ShopManager manager, Wardrobe wardrobeComponent)
    {
        item = hat;
        shopManager = manager;
        wardrobe = wardrobeComponent;

        if (iconImage != null) iconImage.sprite = item.Icon;
        if (nameText != null) nameText.text = item.DisplayName;
        if (rarityImage != null) rarityImage.color = rarityColours[(int)item.Rarity];

        Subscribe();
        Refresh();
    }

    private void OnEnable()
    {
        if (item == null) return;
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        // Unsubscribe first so Setup followed by OnEnable can't register twice.
        Unsubscribe();
        PlayerInventory.OnChanged += HandleInventoryChanged;
        shopManager.OnSelectionChanged += HandleSelectionChanged;
        shopManager.OnTargetChanged += HandleTargetChanged;
    }

    private void Unsubscribe()
    {
        PlayerInventory.OnChanged -= HandleInventoryChanged;
        if (shopManager == null) return;
        shopManager.OnSelectionChanged -= HandleSelectionChanged;
        shopManager.OnTargetChanged -= HandleTargetChanged;
    }

    private void HandleInventoryChanged(string _) => Refresh();

    private void HandleTargetChanged(string _) => Refresh();

    private void HandleSelectionChanged(ShopItemData _) => Refresh();

    private void OnTapped()
    {
        if (shopManager.SelectedItem != item)
        {
            shopManager.SelectItem(item);
            return;
        }

        if (wardrobe.TryEquip(shopManager.SelectedSlotId, item) && animator != null)
        {
            animator.SetTrigger(EquippedHash);
        }
    }

    private void Refresh()
    {
        bool worn = PlayerInventory.GetEquippedId(shopManager.SelectedSlotId) == item.Id;
        if (equippedMark != null) equippedMark.SetActive(worn);

        if (animator != null)
        {
            animator.SetBool(IsEquippedHash, worn);
            animator.SetBool(IsSelectedHash, shopManager.SelectedItem == item);
        }
    }
}
