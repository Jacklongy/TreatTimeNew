using UnityEngine;

/// <summary>
/// Switches between the shop and wardrobe pages. Each section has an Items page and a Dog page,
/// chosen by whichever target is selected in the preview.
/// </summary>
public class ShopNavigation : MonoBehaviour
{
    private enum Section { None, Shop, Wardrobe }

    [SerializeField] private ShopManager shopManager;

    [Header("Shop pages")]
    [SerializeField] private GameObject shopItemsPage;
    [SerializeField] private GameObject shopDogPage;

    [Header("Wardrobe pages")]
    [SerializeField] private GameObject wardrobeItemsPage;
    [SerializeField] private GameObject wardrobeDogPage;

    private Section section = Section.None;

    private void OnEnable()
    {
        shopManager.OnTargetChanged += HandleTargetChanged;
    }

    private void OnDisable()
    {
        shopManager.OnTargetChanged -= HandleTargetChanged;
    }

    /// <summary>Hook to the Shop tab button.</summary>
    public void OpenShop() => Open(Section.Shop);

    /// <summary>Hook to the Wardrobe tab button.</summary>
    public void OpenWardrobe() => Open(Section.Wardrobe);

    /// <summary>Hides every page, e.g. when leaving the shop screen.</summary>
    public void Close() => Open(Section.None);

    private void Open(Section next)
    {
        section = next;
        ShowPages();
    }

    private void HandleTargetChanged(string _)
    {
        if (section != Section.None) ShowPages();
    }

    private void ShowPages()
    {
        // An unsaved preview must not carry over to another page.
        shopManager.ClearPreview();

        bool dog = shopManager.SelectedSlotId == Wardrobe.DogSlotId;

        SetActive(shopItemsPage, section == Section.Shop && !dog);
        SetActive(shopDogPage, section == Section.Shop && dog);
        SetActive(wardrobeItemsPage, section == Section.Wardrobe && !dog);
        SetActive(wardrobeDogPage, section == Section.Wardrobe && dog);
    }

    private static void SetActive(GameObject page, bool active)
    {
        if (page != null) page.SetActive(active);
    }
}
