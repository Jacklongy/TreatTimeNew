using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///Right goals of this script - 
/// - Handles the purchase of items
/// - Send updates to the Wardrobe when purchases are made
/// - Locks items that are not yet available for purchase
/// - Updates the shop UI to reflect the current state of items
/// - finally saves the state of the shop to saved data
/// </summary>
public enum ShopItemState { Locked, Available, Owned }

public enum PurchaseResult { Success, NothingSelected, Locked, AlreadyOwned, NotEnoughCurrency, NoProfile }

public class ShopManager : MonoBehaviour
{
    [SerializeField] private ShopCatalog catalog;
    [SerializeField] private Wardrobe wardrobe;
    [SerializeField] private ShopPreviewController previewController;

    public static event Action OnCurrencyChanged;
    public event Action<ShopItemData> OnSelectionChanged;
    public event Action<string> OnTargetChanged;
    public event Action<ShopItemData, PurchaseResult> OnPurchaseAttempted;

    public ShopItemData SelectedItem { get; private set; }

    /// <summary>The slot id of whatever is currently shown in the preview (egg, chicken, dog...).</summary>
    public string SelectedSlotId
    {
        get
        {
            HatSlot slot = previewController != null ? previewController.CurrentHatSlot : null;
            return slot != null ? slot.SlotId : null;
        }
    }

    /// <summary>Called by the side list buttons: chooses which item (egg, chicken, dog) is being dressed.</summary>
    public void SelectTarget(string slotId)
    {
        // Put the old target back to its saved hat so a stale preview isn't left on it.
        HatSlot previous = previewController.CurrentHatSlot;
        if (previous != null) previous.ShowSavedHat();

        previewController.PreviewItem(slotId);
        ShowPreviewHat(SelectedItem);
        OnTargetChanged?.Invoke(SelectedSlotId);
    }

    /// <summary>Drops the hat being previewed and puts the target back to what it really wears.</summary>
    public void ClearPreview()
    {
        SelectedItem = null;
        HatSlot slot = previewController != null ? previewController.CurrentHatSlot : null;
        if (slot != null) slot.ShowSavedHat();
        OnSelectionChanged?.Invoke(null);
    }

    /// <summary>Called by a shop slot: shows the hat on the current target without saving.</summary>
    public void SelectItem(ShopItemData item)
    {
        SelectedItem = item;
        ShowPreviewHat(item);
        OnSelectionChanged?.Invoke(item);
    }

    public ShopItemState GetState(ShopItemData item)
    {
        if (PlayerInventory.IsOwned(item)) return ShopItemState.Owned;

        PlayerProfileData data = PlayerProfileHolder.Instance != null ? PlayerProfileHolder.Instance.CurrentData : null;
        if (data != null && data.CurrentLevel < GetRequiredLevel(item)) return ShopItemState.Locked;

        return ShopItemState.Available;
    }

    public int GetRequiredLevel(ShopItemData item)
    {
        return catalog != null ? catalog.GetRequiredLevel(item) : 0;
    }

    /// <summary>Buys the selected hat and equips it on the selected target. Returns the result so the slot can animate.</summary>
    public PurchaseResult TryBuySelected()
    {
        PurchaseResult result = TryPurchase(SelectedItem);
        OnPurchaseAttempted?.Invoke(SelectedItem, result);
        return result;
    }

    /// <summary>Equips an already owned selected hat on the selected target.</summary>
    public bool TryEquipSelected()
    {
        return wardrobe.TryEquip(SelectedSlotId, SelectedItem);
    }

    public PurchaseResult TryPurchase(ShopItemData item)
    {
        if (item == null) return PurchaseResult.NothingSelected;

        PlayerProfileData data = PlayerProfileHolder.Instance != null ? PlayerProfileHolder.Instance.CurrentData : null;
        if (data == null) return PurchaseResult.NoProfile;

        ShopItemState state = GetState(item);
        if (state == ShopItemState.Owned) return PurchaseResult.AlreadyOwned;
        if (state == ShopItemState.Locked) return PurchaseResult.Locked;

        if (item.Currency == CurrencyType.Coins)
        {
            if (data.Coins < item.Price) return PurchaseResult.NotEnoughCurrency;
            data.Coins -= item.Price;
        }
        else
        {
            if (data.Gems < item.Price) return PurchaseResult.NotEnoughCurrency;
            data.Gems -= item.Price;
        }

        PlayerInventory.AddOwned(item);
        // Auto-equip on the current target, saving once below. Skipped if the hat doesn't fit that target.
        wardrobe.TryEquip(SelectedSlotId, item, false);

        _ = ProfileSyncService.SaveAndSyncAsync();
        OnCurrencyChanged?.Invoke();
        return PurchaseResult.Success;
    }

    private void ShowPreviewHat(ShopItemData item)
    {
        HatSlot slot = previewController.CurrentHatSlot;
        if (slot == null) return;

        if (item != null && Wardrobe.FitsSlot(slot.SlotId, item)) slot.ShowPreview(item);
        else slot.ShowSavedHat();
    }
}
