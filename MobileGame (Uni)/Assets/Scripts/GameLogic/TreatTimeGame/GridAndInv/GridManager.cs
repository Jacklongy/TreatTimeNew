using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Manages the unified grid where all items can be placed.
/// Handles empty tile detection and item placement.
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager instance;
    
    [Header("Grid Setup")]
    public Tile[] allTiles; // All tiles in the grid

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        // Auto-find all Tile components in children if not assigned
        if (allTiles == null || allTiles.Length == 0)
        {
            allTiles = GetComponentsInChildren<Tile>();
            Debug.Log($"GridManager found {allTiles.Length} tiles");
        }
    }

    /// <summary>
    /// Get a random empty tile from the grid
    /// </summary>
    /// <returns>Empty tile, or null if grid is full</returns>
    public Tile GetRandomEmptyTile()
    {
        List<Tile> emptyTiles = new List<Tile>();

        foreach (Tile tile in allTiles)
        {
            tile.CheckIfFull();
            if (!tile.IsFull)
            {
                emptyTiles.Add(tile);
            }
        }

        if (emptyTiles.Count == 0)
        {
            Debug.LogWarning("No empty tiles available on grid!");
            return null;
        }

        return emptyTiles[Random.Range(0, emptyTiles.Count)];
    }

    /// <summary>
    /// Get all empty tiles
    /// </summary>
    public List<Tile> GetAllEmptyTiles()
    {
        List<Tile> emptyTiles = new List<Tile>();

        foreach (Tile tile in allTiles)
        {
            tile.CheckIfFull();
            if (!tile.IsFull)
            {
                emptyTiles.Add(tile);
            }
        }

        return emptyTiles;
    }

    /// <summary>
    /// Get all occupied tiles
    /// </summary>
    public List<Tile> GetAllOccupiedTiles()
    {
        List<Tile> occupiedTiles = new List<Tile>();

        foreach (Tile tile in allTiles)
        {
            tile.CheckIfFull();
            if (tile.IsFull)
            {
                occupiedTiles.Add(tile);
            }
        }

        return occupiedTiles;
    }

    /// <summary>
    /// Check how many tiles are available
    /// </summary>
    public int GetEmptyTileCount()
    {
        int count = 0;
        foreach (Tile tile in allTiles)
        {
            tile.CheckIfFull();
            if (!tile.IsFull)
                count++;
        }
        return count;
    }

    /// <summary>
    /// Place an item on a specific tile
    /// </summary>
    public bool PlaceItemOnTile(GameObject item, Tile tile)
    {
        if (tile == null)
        {
            Debug.LogError("Cannot place item: Tile is null");
            return false;
        }

        if (tile.IsFull)
        {
            Debug.LogWarning("Cannot place item: Tile is already full");
            return false;
        }

        tile.TakeObject(item);
        return true;
    }

    /// <summary>
    /// Place an item on a random empty tile
    /// </summary>
    public bool PlaceItemOnRandomTile(GameObject item)
    {
        Tile emptyTile = GetRandomEmptyTile();

        if (emptyTile == null)
        {
            Debug.LogWarning("Cannot place item: No empty tiles available");
            return false;
        }

        return PlaceItemOnTile(item, emptyTile);
    }

    /// <summary>
    /// Get the nearest empty tile to a position
    /// </summary>
    public Tile GetNearestEmptyTile(Vector3 position)
    {
        List<Tile> emptyTiles = GetAllEmptyTiles();

        if (emptyTiles.Count == 0)
        {
            Debug.LogWarning("No empty tiles available!");
            return null;
        }

        Tile nearestTile = emptyTiles[0];
        float shortestDistance = Vector3.Distance(position, nearestTile.transform.position);

        foreach (Tile tile in emptyTiles)
        {
            float distance = Vector3.Distance(position, tile.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestTile = tile;
            }
        }

        return nearestTile;
    }
}
