using UnityEngine;

/// <summary>
/// Static definition of a purchasable hat. Never stores player state (owned or equipped);
/// that lives in PlayerProfileData so it is saved and synced.
/// </summary>
[CreateAssetMenu(fileName = "NewShopItem", menuName = "Shop/Shop Item")]
public class ShopItemData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique and permanent. Saves reference this, so never change it after release.")]
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;

    [Header("Shop")]
    [SerializeField] private ItemCategory category;
    [SerializeField] private ItemRarity rarity;
    [SerializeField] private CurrencyType currency;
    [Min(0)] [SerializeField] private int price;
    [Tooltip("Player level needed before this can be bought.")]
    [Min(0)] [SerializeField] private int requiredLevel;

    [Header("Visuals")]
    [Tooltip("Prefab with a sprite, spawned under an item's hat anchor when equipped.")]
    [SerializeField] private GameObject hatPrefab;

    public string Id => id;
    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public ItemCategory Category => category;
    public ItemRarity Rarity => rarity;
    public CurrencyType Currency => currency;
    public int Price => price;
    public int RequiredLevel => requiredLevel;
    public GameObject HatPrefab => hatPrefab;

#if UNITY_EDITOR
    // Auto-fills a permanent id the first time the asset is created; never overwrites an existing one.
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
        {
            id = System.Guid.NewGuid().ToString("N");
        }
    }
#endif
}
