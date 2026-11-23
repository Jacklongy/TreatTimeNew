using UnityEngine;

/// <summary>
/// Handles equipping and unequipping owned hats.
/// </summary>
public class Wardrobe : MonoBehaviour
{
    /// <summary>
    /// Equips an owned hat on the selected slot. Hats are shared across all targets.
    /// Pass save false when the caller saves afterwards (e.g. a purchase).
    /// </summary>
    public bool TryEquip(string slotId, ShopItemData item, bool save = true)
    {
        if (item == null || string.IsNullOrEmpty(slotId)) return false;
        if (!PlayerInventory.IsOwned(item)) return false;
        PlayerInventory.SetEquipped(slotId, item);
        if (save) _ = ProfileSyncService.SaveAndSyncAsync();
        return true;
    }

    public void Unequip(string slotId)
    {
        PlayerInventory.Unequip(slotId);
        _ = ProfileSyncService.SaveAndSyncAsync();
    }
}
