using System;

/// <summary>
/// The only code that reads or writes shop data (owned and equipped hats) in the player profile.
/// It changes the in-memory profile only; callers decide when to save.
/// </summary>
public static class PlayerInventory
{
    /// <summary>Raised with the slot id whose hat changed, or null when only ownership changed.</summary>
    public static event Action<string> OnChanged;

    private static PlayerProfileData Data =>
        PlayerProfileHolder.Instance != null ? PlayerProfileHolder.Instance.CurrentData : null;

    public static bool IsOwned(ShopItemData item)
    {
        PlayerProfileData data = Data;
        return item != null && data != null && data.PurchasedItemIds.Contains(item.Id);
    }

    public static void AddOwned(ShopItemData item)
    {
        PlayerProfileData data = Data;
        if (item == null || data == null || data.PurchasedItemIds.Contains(item.Id)) return;

        data.PurchasedItemIds.Add(item.Id);
        OnChanged?.Invoke(null);
    }

    /// <summary>Returns the equipped hat id for a slot, or an empty string if none.</summary>
    public static string GetEquippedId(string slotId)
    {
        PlayerProfileData data = Data;
        if (data == null) return "";

        foreach (EquippedSlot slot in data.EquippedSlots)
        {
            if (slot.SlotId == slotId) return slot.ItemId;
        }
        return "";
    }

    public static void SetEquipped(string slotId, ShopItemData item)
    {
        PlayerProfileData data = Data;
        if (data == null || string.IsNullOrEmpty(slotId)) return;

        if (item == null)
        {
            Unequip(slotId);
            return;
        }

        EquippedSlot entry = new EquippedSlot { SlotId = slotId, ItemId = item.Id };
        int index = data.EquippedSlots.FindIndex(s => s.SlotId == slotId);
        if (index >= 0) data.EquippedSlots[index] = entry;
        else data.EquippedSlots.Add(entry);

        OnChanged?.Invoke(slotId);
    }

    public static void Unequip(string slotId)
    {
        PlayerProfileData data = Data;
        if (data == null) return;

        if (data.EquippedSlots.RemoveAll(s => s.SlotId == slotId) > 0)
        {
            OnChanged?.Invoke(slotId);
        }
    }
}
