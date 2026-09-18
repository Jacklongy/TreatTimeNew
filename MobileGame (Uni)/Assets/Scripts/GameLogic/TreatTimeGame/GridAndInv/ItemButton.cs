using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attached to spawn buttons.
/// When clicked (via OnClick event), spawns a random item from the array onto the grid.
/// </summary>
public class ItemButton : MonoBehaviour
{
    [Header("Item Setup")]
    public GameObject[] itemPrefabs; // Array of items to randomly spawn
    
    [Header("References")]
    private GridManager gridManager;
    private SoundManagerScript soundManager;
    
    public Image ProgressRing;

    // 5 Presses until cooldown, can be adjusted in inspector.
    public int ButtonPresses = 5;

    // how long it takes to recharge back to 5 presses, can be adjusted in inspector. 
    // so when the player presses 5 times, the button will be disabled for 30 seconds. then when time is up the player can press 5 times again.
    public float spawnCooldown = 30f;
    private bool canSpawn = true;
    private int pressesRemaining;

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        soundManager = FindObjectOfType<SoundManagerScript>();
        pressesRemaining = ButtonPresses;

        // Ring starts full (ready to use)
        if (ProgressRing != null)
        {
            ProgressRing.fillAmount = 1f;
        }

        if (itemPrefabs == null || itemPrefabs.Length == 0)
        {
            Debug.LogWarning($"ItemButton on {gameObject.name} has no item prefabs assigned!");
        }
    }

    /// <summary>
    /// Called by Button OnClick event
    /// Spawns random item from array onto grid
    /// </summary>
    public void SpawnRandomItem()
    {
        if (!canSpawn) return;

        if (itemPrefabs == null || itemPrefabs.Length == 0)
        {
            Debug.LogError("No item prefabs assigned!");
            return;
        }

        if (gridManager == null)
        {
            Debug.LogError("GridManager not found!");
            return;
        }

        // Get random empty tile
        Tile emptyTile = gridManager.GetRandomEmptyTile();

        if (emptyTile == null)
        {
            Debug.LogWarning("No empty tiles available on grid!");
            return;
        }

        // Pick random item from array
        GameObject randomItemPrefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];

        // Spawn the item
        SpawnItemOnTile(randomItemPrefab, emptyTile);

        // Play sound
        if (soundManager != null)
        {
            soundManager.Play("ItemSpawn");
        }

        Debug.Log($"Spawned {randomItemPrefab.name} on grid");

        // Cooldown temporarily disabled for playtesting.
        // pressesRemaining--;
        //
        // if (ProgressRing != null)
        // {
        //     ProgressRing.fillAmount = (float)pressesRemaining / ButtonPresses;
        // }
        //
        // if (pressesRemaining <= 0)
        // {
        //     canSpawn = false;
        //     StartCoroutine(CooldownRoutine());
        // }
    }

    private IEnumerator CooldownRoutine()
    {
        float elapsed = 0f;

        while (elapsed < spawnCooldown)
        {
            elapsed += Time.deltaTime;
            if (ProgressRing != null)
            {
                ProgressRing.fillAmount = elapsed / spawnCooldown;
            }
            yield return null;
        }

        // Cooldown complete — reset
        if (ProgressRing != null)
        {
            ProgressRing.fillAmount = 1f;
        }
        pressesRemaining = ButtonPresses;
        canSpawn = true;
    }

    /// <summary>
    /// Spawn item on specific tile
    /// </summary>
    private void SpawnItemOnTile(GameObject itemPrefab, Tile tile)
    {
        // Instantiate item at tile position
        GameObject spawnedItem = Instantiate(itemPrefab, tile.transform.position, Quaternion.identity);

        // Place on tile
        tile.TakeObject(spawnedItem);

        // Reset animation
        Item itemComponent = spawnedItem.GetComponent<Item>();
        if (itemComponent != null)
        {
            itemComponent.ResetAnim();
        }
    }
}

