using UnityEngine;

/// <summary>
/// Goes on anything that can wear a hat (dog, egg, chicken...). Spawns the saved hat under hatAnchor.
/// </summary>
public class HatSlot : MonoBehaviour
{
    [Tooltip("Unique permanent id, e.g. egg, chicken, dog. Must match the shop preview itemName.")]
    [SerializeField] private string slotId;
    [SerializeField] private Transform hatAnchor;
    [SerializeField] private ShopCatalog catalog;

    private GameObject currentHat;

    public string SlotId => slotId;

    private void Awake()
    {
        ShowSavedHat();
    }

    private void OnEnable()
    {
        PlayerInventory.OnChanged += HandleInventoryChanged;
    }

    private void OnDisable()
    {
        PlayerInventory.OnChanged -= HandleInventoryChanged;
    }

    private void HandleInventoryChanged(string changedSlot)
    {
        if (changedSlot == slotId) ShowSavedHat();
    }

    public void ShowSavedHat()
    {
        ShowHat(catalog != null ? catalog.GetById(PlayerInventory.GetEquippedId(slotId)) : null);
    }

    /// <summary>Shows a hat without saving it. Call ShowSavedHat to go back.</summary>
    public void ShowPreview(ShopItemData item)
    {
        ShowHat(item);
    }

    private void ShowHat(ShopItemData item)
    {
        if (currentHat != null)
        {
            Destroy(currentHat);
            currentHat = null;
        }

        if (item == null || item.HatPrefab == null || hatAnchor == null) return;

        currentHat = Instantiate(item.HatPrefab, hatAnchor);
        currentHat.transform.localPosition = Vector3.zero;
    }
}
