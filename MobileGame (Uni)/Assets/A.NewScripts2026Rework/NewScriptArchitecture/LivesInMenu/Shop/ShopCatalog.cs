using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// The list of every shop item. Resolves saved ids back to ShopItemData assets.
/// </summary>
[CreateAssetMenu(fileName = "ShopCatalog", menuName = "Shop/Shop Catalog")]
public class ShopCatalog : ScriptableObject
{
    [HideInInspector]
    [SerializeField] private List<ShopItemData> dogHats = new List<ShopItemData>();

    [Header("Shared hats")]
    [FormerlySerializedAs("itemHats")]
    [SerializeField] private List<ShopItemData> hats = new List<ShopItemData>();

    [Header("Unlock levels")]
    [Tooltip("Player level needed to buy each rarity, in enum order: Common, Rare, Epic, Legendary.")]
    [SerializeField] private int[] requiredLevelByRarity = { 0, 20, 40, 60 };

    private Dictionary<string, ShopItemData> lookup;

    public int GetRequiredLevel(ShopItemData item)
    {
        int index = (int)item.Rarity;
        return index >= 0 && index < requiredLevelByRarity.Length ? requiredLevelByRarity[index] : 0;
    }

    public IEnumerable<ShopItemData> Hats
    {
        get
        {
            foreach (ShopItemData hat in hats) yield return hat;
            foreach (ShopItemData hat in dogHats) yield return hat;
        }
    }

    public ShopItemData GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        if (lookup == null) BuildLookup();
        lookup.TryGetValue(id, out ShopItemData item);
        return item;
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<string, ShopItemData>();
        foreach (ShopItemData item in Hats)
        {
            if (item == null) continue;

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
