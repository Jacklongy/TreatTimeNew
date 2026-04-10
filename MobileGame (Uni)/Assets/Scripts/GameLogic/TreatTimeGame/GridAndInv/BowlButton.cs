using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Attached to spawn buttons.
/// When clicked (via OnClick event), spawns a random bowl from the array onto the grid.
/// </summary>
public class BowlButton : MonoBehaviour
{
    [Header("Bowl Setup")]
    public GameObject[] bowlPrefabs; // Array of bowls to randomly spawn
    
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

        if (bowlPrefabs == null || bowlPrefabs.Length == 0)
        {
            Debug.LogWarning($"BowlButton on {gameObject.name} has no bowl prefabs assigned!");
        }
    }

    /// <summary>
    /// Called by Button OnClick event
    /// Spawns random bowl from array onto grid
    /// </summary>
    public void SpawnRandomBowl()
    {
        if (!canSpawn) return;

        if (bowlPrefabs == null || bowlPrefabs.Length == 0)
        {
            Debug.LogError("No bowl prefabs assigned!");
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

        // Pick random bowl from array
        GameObject randomBowlPrefab = bowlPrefabs[Random.Range(0, bowlPrefabs.Length)];

        // Spawn the bowl
        SpawnBowlOnTile(randomBowlPrefab, emptyTile);

        // Play sound
        if (soundManager != null)
        {
            soundManager.Play("BowlSpawn");
        }

        Debug.Log($"Spawned {randomBowlPrefab.name} on grid");

        // Track presses and start cooldown when used up
        pressesRemaining--;

        // Update ring to show remaining presses
        if (ProgressRing != null)
        {
            ProgressRing.fillAmount = (float)pressesRemaining / ButtonPresses;
        }

        if (pressesRemaining <= 0)
        {
            canSpawn = false;
            StartCoroutine(CooldownRoutine());
        }
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
    /// Spawn bowl on specific tile
    /// </summary>
    private void SpawnBowlOnTile(GameObject bowlPrefab, Tile tile)
    {
        // Instantiate bowl at tile position
        GameObject spawnedBowl = Instantiate(bowlPrefab, tile.transform.position, Quaternion.identity);

        // Place on tile
        tile.TakeObject(spawnedBowl);

        // Reset animation
        Item itemComponent = spawnedBowl.GetComponent<Item>();
        if (itemComponent != null)
        {
            itemComponent.ResetAnim();
        }
    }
}
