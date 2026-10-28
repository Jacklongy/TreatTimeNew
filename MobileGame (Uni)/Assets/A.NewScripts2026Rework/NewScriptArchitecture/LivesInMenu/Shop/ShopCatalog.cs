using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The list of every shop item. Resolves saved ids back to ShopItemData assets.
/// </summary>
[CreateAssetMenu(fileName = "ShopCatalog", menuName = "Shop/Shop Catalog")]
public class ShopCatalog : ScriptableObject
{
    [Header("Dog")]
    [SerializeField] private List<ShopItemData> dogHats = new List<ShopItemData>();

    [Header("Items")]
    [SerializeField] private List<ShopItemData> itemHats = new List<ShopItemData>();

    private Dictionary<string, ShopItemData> lookup;

    public IReadOnlyList<ShopItemData> DogHats => dogHats;
    public IReadOnlyList<ShopItemData> ItemHats => itemHats;

    public ShopItemData GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        if (lookup == null) BuildLookup();
        lookup.TryGetValue(id, out ShopItemData item);
        return item;
    }

    public IReadOnlyList<ShopItemData> GetByCategory(ItemCategory category)
    {
        return category == ItemCategory.DogHat ? dogHats : itemHats;
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<string, ShopItemData>();
        AddToLookup(dogHats, ItemCategory.DogHat);
        AddToLookup(itemHats, ItemCategory.ItemHat);
    }

    private void AddToLookup(List<ShopItemData> list, ItemCategory expected)
    {
        foreach (ShopItemData item in list)
        {
            if (item == null) continue;

            if (item.Category != expected)
            {
                Debug.LogWarning($"Shop item '{item.name}' is in the {expected} list but its category is {item.Category}.", item);
            }

            if (string.IsNullOrEmpty(item.Id) || !lookup.TryAdd(item.Id, item))
            {
                Debug.LogWarning($"Shop item '{item.name}' has a missing or duplicate id.", item);
            }
        }
    }

    // Asset data can change in the editor, so rebuild next time it is needed.
    private void OnValidate()
    {
        lookup = null;
    }
}
