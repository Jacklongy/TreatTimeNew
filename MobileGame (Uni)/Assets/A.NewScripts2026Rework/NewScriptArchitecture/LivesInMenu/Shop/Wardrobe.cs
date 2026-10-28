using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles equipping and unequipping owned hats.
/// </summary>
public class Wardrobe : MonoBehaviour
{
    public const string DogSlotId = "dog";

    /// <summary>
    /// Equips an owned hat on a slot. Dog hats only fit the dog slot; item hats fit every other slot.
    /// Pass save false when the caller saves afterwards (e.g. a purchase).
    /// </summary>
    public bool TryEquip(string slotId, ShopItemData item, bool save = true)
    {
        if (item == null || string.IsNullOrEmpty(slotId)) return false;
        if (!PlayerInventory.IsOwned(item)) return false;
        if (!FitsSlot(slotId, item)) return false;

        PlayerInventory.SetEquipped(slotId, item);
        if (save) _ = ProfileSyncService.SaveAndSyncAsync();
        return true;
    }

    public void Unequip(string slotId)
    {
        PlayerInventory.Unequip(slotId);
        _ = ProfileSyncService.SaveAndSyncAsync();
    }

    public static bool FitsSlot(string slotId, ShopItemData item)
    {
        bool isDog = slotId == DogSlotId;
        return isDog ? item.Category == ItemCategory.DogHat : item.Category == ItemCategory.ItemHat;
    }
}
