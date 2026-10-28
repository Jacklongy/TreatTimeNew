using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopPreviewController : MonoBehaviour
{
    [Serializable]
    public struct PreviewItemEntry
    {
        public string itemName;     // the string you use to reference it
        public GameObject item;
    }

    public GameObject currentPreviewItem;

    /// <summary>The HatSlot on the previewed object, if it has one.</summary>
    public HatSlot CurrentHatSlot =>
        currentPreviewItem != null ? currentPreviewItem.GetComponentInChildren<HatSlot>(true) : null;

    [SerializeField]
    private PreviewItemEntry[] previewItemArray;

    private readonly Dictionary<string, GameObject> previewItems = new Dictionary<string, GameObject>();

    void Awake()
    {
        foreach (var entry in previewItemArray)
        {
            if (entry.item == null || string.IsNullOrEmpty(entry.itemName)) continue;
            previewItems[entry.itemName] = entry.item;
            entry.item.SetActive(false);
        }

        // set the first preview item as the current one if available
        if (previewItemArray.Length > 0 && !string.IsNullOrEmpty(previewItemArray[0].itemName))
        {
            PreviewItem(previewItemArray[0].itemName);
        }
    }

    public void PreviewItem(string itemName)
    {
        if (!previewItems.TryGetValue(itemName, out var next)) return;

        if (currentPreviewItem != null)
            currentPreviewItem.SetActive(false);

        currentPreviewItem = next;
        currentPreviewItem.SetActive(true);
    }
}
