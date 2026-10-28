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
    [SerializeField] private Wardrobe wardrobe;
    [SerializeField] private ShopPreviewController previewController;

    public static event Action OnCurrencyChanged;
    public event Action<ShopItemData> OnSelectionChanged;
    public event Action<ShopItemData, PurchaseResult> OnPurchaseAttempted;

    public ShopItemData SelectedItem { get; private set; }
    public string SelectedSlotId { get; private set; }

    /// <summary>Called by the side list buttons: chooses which item (egg, chicken, dog) is being dressed.</summary>
    public void SelectTarget(string slotId)
    {
        SelectedSlotId = slotId;
        previewController.PreviewItem(slotId);
        ShowPreviewHat(SelectedItem);
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
        if (data != null && data.CurrentLevel < item.RequiredLevel) return ShopItemState.Locked;

        return ShopItemState.Available;
    }

    /// <summary>Button hook: buys the selected hat and equips it on the selected target.</summary>
    public void BuySelected()
    {
        PurchaseResult result = TryPurchase(SelectedItem);
        OnPurchaseAttempted?.Invoke(SelectedItem, result);
    }

    /// <summary>Button hook: equips an already owned selected hat on the selected target.</summary>
    public void EquipSelected()
    {
        wardrobe.TryEquip(SelectedSlotId, SelectedItem);
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
