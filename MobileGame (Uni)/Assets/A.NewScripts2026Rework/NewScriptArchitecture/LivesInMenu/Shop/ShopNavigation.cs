using UnityEngine;

/// <summary>
/// Switches between the shared shop and wardrobe pages.
/// </summary>
public class ShopNavigation : MonoBehaviour
{
    private enum Section { None, Shop, Wardrobe }

    [SerializeField] private ShopManager shopManager;

    [Header("Shared pages")]
    [SerializeField] private GameObject shopItemsPage;
    [SerializeField] private GameObject wardrobeItemsPage;

    [HideInInspector, SerializeField] private GameObject shopDogPage;
    [HideInInspector, SerializeField] private GameObject wardrobeDogPage;

    /// <summary>Hook to the Shop tab button.</summary>
    public void OpenShop() => Open(Section.Shop);

    /// <summary>Hook to the Wardrobe tab button.</summary>
    public void OpenWardrobe() => Open(Section.Wardrobe);

    /// <summary>Hides every page, e.g. when leaving the shop screen.</summary>
    public void Close() => Open(Section.None);

    private void Open(Section next)
    {
        shopManager.ClearPreview();

        SetActive(shopItemsPage, next == Section.Shop);
        SetActive(wardrobeItemsPage, next == Section.Wardrobe);
        SetActive(shopDogPage, false);
        SetActive(wardrobeDogPage, false);
    }

    private static void SetActive(GameObject page, bool active)
    {
        if (page != null) page.SetActive(active);
    }
}
