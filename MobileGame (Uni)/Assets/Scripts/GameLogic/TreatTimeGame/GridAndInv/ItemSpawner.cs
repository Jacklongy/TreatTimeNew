using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ItemSpawner manages global spawn settings.
/// Items are spawned via ItemButton clicks onto the unified grid.
/// Also handles the Board Clear (Shuffle) feature to help players reset when overwhelmed.
/// </summary>
public class ItemSpawner : MonoBehaviour
{
    [Header("Board Clear System")]
    public int shuffleCount = 5;
    public TextMeshProUGUI ShuffleDisplay;
    public Button ShuffleButton;

    [Header("Grid Reference")]
    private GridManager gridManager;

    private void Start()
    {
        gridManager = FindObjectOfType<GridManager>();

        if (ShuffleButton != null)
        {
            ShuffleButton.onClick.AddListener(Shuffle);
        }

        UpdateShuffleDisplay();
    }

    private void Update()
    {
        // Clamp shuffle count to minimum 0
        if (shuffleCount < 0)
        {
            shuffleCount = 0;
        }

        UpdateShuffleDisplay();
    }

    /// <summary>
    /// Update the shuffle count display
    /// </summary>
    private void UpdateShuffleDisplay()
    {
        if (ShuffleDisplay != null)
        {
            ShuffleDisplay.text = shuffleCount.ToString();
        }
    }

    /// <summary>
    /// Clear all items from the grid (Board Shuffle)
    /// Allows player to reset and continue if overwhelmed or out of space
    /// </summary>
    public void Shuffle()
    {
        if (shuffleCount <= 0)
        {
            Debug.LogWarning("No board clears remaining!");
            return;
        }

        if (gridManager == null)
        {
            Debug.LogError("GridManager not found!");
            return;
        }

        // Play sound effect
        SoundManagerScript soundManager = FindObjectOfType<SoundManagerScript>();
        if (soundManager != null)
        {
            soundManager.Play("Shuffle");
        }

        shuffleCount--;

        // Get all occupied tiles
        List<Tile> occupiedTiles = gridManager.GetAllOccupiedTiles();

        Debug.Log($"Board Clear: Removing {occupiedTiles.Count} items from grid");

        // Destroy all items on the grid
        foreach (Tile occupiedTile in occupiedTiles)
        {
            if (occupiedTile.ObjectContainer != null)
            {
                GameObject itemToDestroy = occupiedTile.ObjectContainer;
                occupiedTile.RemoveObject();
                Destroy(itemToDestroy);
            }
        }

        // Visual/Audio feedback
        if (soundManager != null)
        {
            soundManager.Play("CantPlace"); // Or another "clear" sound
        }

        Debug.Log("Board cleared! Player can now continue with fresh space");

        UpdateShuffleDisplay();
    }

    private void OnDestroy()
    {
        if (ShuffleButton != null)
        {
            ShuffleButton.onClick.RemoveListener(Shuffle);
        }
    }
}
