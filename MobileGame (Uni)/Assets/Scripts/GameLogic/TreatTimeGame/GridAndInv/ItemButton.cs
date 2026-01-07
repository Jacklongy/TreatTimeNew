using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attached to each item spawn button in the hotbar.
/// When clicked, spawns that item on a random empty grid tile.
/// </summary>
public class ItemButton : MonoBehaviour
{
    [Header("Item Setup")]
    public GameObject itemPrefab; // The item to spawn when button is clicked
    public Image buttonImage; // Visual representation of the item
    public Button button; // The button component

    [Header("References")]
    private ItemSpawner itemSpawner;
    private GridManager gridManager;

    private void Start()
    {
        itemSpawner = FindObjectOfType<ItemSpawner>();
        gridManager = FindObjectOfType<GridManager>();

        if (button == null)
            button = GetComponent<Button>();

        // Add button click listener
        button.onClick.AddListener(OnButtonClicked);

        // Set button image if not assigned
        if (buttonImage == null && itemPrefab != null)
        {
            // Try to get the sprite from the item prefab
            SpriteRenderer spriteRenderer = itemPrefab.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && buttonImage != null)
            {
                buttonImage.sprite = spriteRenderer.sprite;
            }
        }

        Debug.Log($"ItemButton initialized for: {itemPrefab.name}");
    }

    /// <summary>
    /// Called when the button is clicked
    /// </summary>
    public void OnButtonClicked()
    {
        if (itemPrefab == null)
        {
            Debug.LogError("Item prefab not assigned to button!");
            return;
        }

        if (gridManager == null)
        {
            Debug.LogError("GridManager not found!");
            return;
        }

        // Get a random empty tile
        Tile emptyTile = gridManager.GetRandomEmptyTile();

        if (emptyTile == null)
        {
            Debug.LogWarning("No empty tiles available on grid!");
            return;
        }

        // Spawn the item
        SpawnItemOnTile(emptyTile);

        // Play sound effect
        SoundManagerScript soundManager = FindObjectOfType<SoundManagerScript>();
        if (soundManager != null)
        {
            soundManager.Play("ItemSpawn"); // Change to whatever your spawn sound is
        }

        Debug.Log($"Spawned {itemPrefab.name} on grid");
    }

    /// <summary>
    /// Spawn the item on a specific tile
    /// </summary>
    private void SpawnItemOnTile(Tile tile)
    {
        // Instantiate the item
        GameObject spawnedItem = Instantiate(itemPrefab, tile.transform.position, Quaternion.identity);

        // Place it on the tile
        tile.TakeObject(spawnedItem);

        // Reset the item's animation state
        Item itemComponent = spawnedItem.GetComponent<Item>();
        if (itemComponent != null)
        {
            itemComponent.ResetAnim();
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
        }
    }
}
