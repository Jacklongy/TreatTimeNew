using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Goes on a wardrobe page (Items or Dog). Spawns a slot for every owned hat in its category,
/// adding new ones when hats are bought. Slots are never removed because hats can't be lost.
/// </summary>
public class WardrobeGrid : MonoBehaviour
{
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private Wardrobe wardrobe;
    [SerializeField] private ShopCatalog catalog;
    [SerializeField] private ItemCategory category;
    [SerializeField] private WardrobeSlotUI slotPrefab;
    [Tooltip("Parent of the spawned slots, e.g. the object with the Grid Layout Group.")]
    [SerializeField] private Transform slotParent;

    private readonly Dictionary<string, WardrobeSlotUI> spawned = new Dictionary<string, WardrobeSlotUI>();

    private void OnEnable()
    {
        PlayerInventory.OnChanged += HandleInventoryChanged;
        AddMissingSlots();
    }

    private void OnDisable()
    {
        PlayerInventory.OnChanged -= HandleInventoryChanged;
    }

    private void HandleInventoryChanged(string _) => AddMissingSlots();

    private void AddMissingSlots()
    {
        foreach (ShopItemData hat in catalog.GetByCategory(category))
        {
            if (hat == null || !PlayerInventory.IsOwned(hat) || spawned.ContainsKey(hat.Id)) continue;

            WardrobeSlotUI slot = Instantiate(slotPrefab, slotParent);
            slot.Setup(hat, shopManager, wardrobe);
            spawned.Add(hat.Id, slot);
        }
    }

    /// <summary>Hook this to the "No hat" button's On Click.</summary>
    public void RemoveHat()
    {
        string slotId = shopManager.SelectedSlotId;
        if (!string.IsNullOrEmpty(slotId)) wardrobe.Unequip(slotId);
    }
}
