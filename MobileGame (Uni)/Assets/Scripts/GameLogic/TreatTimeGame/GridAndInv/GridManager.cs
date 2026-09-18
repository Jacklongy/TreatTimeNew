using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the unified grid where all items can be placed.
/// Handles empty tile detection and item placement.
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager instance;
    
    [Header("Grid Setup")]
    public Tile[] allTiles; // All tiles in the grid
    [SerializeField] private float gridStepX = 0.6868f;
    [SerializeField] private float gridStepY = 0.6868f;
    [SerializeField] private float neighborTolerance = 0.08f;
    [SerializeField] private float laneTolerance = 0.15f;

    private readonly Dictionary<Tile, TileNeighborData> neighborByTile = new Dictionary<Tile, TileNeighborData>();

    private struct TileNeighborData
    {
        public Tile Top;
        public Tile Bottom;
        public Tile Left;
        public Tile Right;
    }

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

        BuildNeighborLookup();

        GroupOutlineController controller = GroupOutlineController.Instance != null
            ? GroupOutlineController.Instance
            : FindObjectOfType<GroupOutlineController>();

        if (controller == null)
        {
            GameObject controllerObject = new GameObject("GroupOutlineController");
            controller = controllerObject.AddComponent<GroupOutlineController>();
        }

        controller?.RequestRefresh();
    }

    public void BuildNeighborLookup()
    {
        neighborByTile.Clear();

        if (allTiles == null || allTiles.Length == 0)
        {
            return;
        }

        foreach (Tile tile in allTiles)
        {
            if (tile == null)
            {
                continue;
            }

            TileNeighborData neighborData = new TileNeighborData
            {
                Top = FindDirectionalNeighbor(tile, Vector2.up),
                Bottom = FindDirectionalNeighbor(tile, Vector2.down),
                Left = FindDirectionalNeighbor(tile, Vector2.left),
                Right = FindDirectionalNeighbor(tile, Vector2.right)
            };

            neighborByTile[tile] = neighborData;
        }
    }

    private Tile FindDirectionalNeighbor(Tile origin, Vector2 direction)
    {
        if (origin == null || allTiles == null || allTiles.Length == 0)
        {
            return null;
        }

        bool vertical = Mathf.Abs(direction.y) > 0.5f;
        float expectedStep = vertical ? Mathf.Abs(gridStepY) : Mathf.Abs(gridStepX);
        float maxDistance = expectedStep > 0f
            ? (expectedStep * 1.75f) + Mathf.Max(0.001f, neighborTolerance)
            : float.MaxValue;
        float laneMax = Mathf.Max(0.001f, laneTolerance);

        Tile bestTile = null;
        float bestScore = float.MaxValue;

        for (int i = 0; i < allTiles.Length; i++)
        {
            Tile candidate = allTiles[i];
            if (candidate == null || candidate == origin)
            {
                continue;
            }

            Vector2 delta = candidate.transform.position - origin.transform.position;
            float primary = vertical ? delta.y : delta.x;
            float lateral = vertical ? Mathf.Abs(delta.x) : Mathf.Abs(delta.y);

            if (vertical && direction.y > 0f && primary <= 0f) continue;
            if (vertical && direction.y < 0f && primary >= 0f) continue;
            if (!vertical && direction.x > 0f && primary <= 0f) continue;
            if (!vertical && direction.x < 0f && primary >= 0f) continue;

            float absPrimary = Mathf.Abs(primary);
            if (lateral > laneMax) continue;
            if (absPrimary > maxDistance) continue;

            float stepError = expectedStep > 0f ? Mathf.Abs(absPrimary - expectedStep) : absPrimary;
            float score = stepError + (lateral * 0.5f);

            if (score < bestScore)
            {
                bestScore = score;
                bestTile = candidate;
            }
        }

        return bestTile;
    }

    private void EnsureNeighborLookup()
    {
        if (allTiles == null)
        {
            return;
        }

        int nonNullTileCount = 0;
        for (int i = 0; i < allTiles.Length; i++)
        {
            if (allTiles[i] != null)
            {
                nonNullTileCount++;
            }
        }

        if (neighborByTile.Count != nonNullTileCount)
        {
            BuildNeighborLookup();
        }
    }

    public string GetItemTagForTile(Tile tile)
    {
        if (tile == null || tile.ObjectContainer == null)
        {
            return null;
        }

        return tile.ObjectContainer.tag;
    }

    public bool IsOccupied(Tile tile)
    {
        if (tile == null)
        {
            return false;
        }

        tile.CheckIfFull();
        return tile.IsFull && tile.ObjectContainer != null;
    }

    public Tile GetNeighborTop(Tile tile)
    {
        return GetNeighbor(tile, Vector2.up);
    }

    public Tile GetNeighborBottom(Tile tile)
    {
        return GetNeighbor(tile, Vector2.down);
    }

    public Tile GetNeighborLeft(Tile tile)
    {
        return GetNeighbor(tile, Vector2.left);
    }

    public Tile GetNeighborRight(Tile tile)
    {
        return GetNeighbor(tile, Vector2.right);
    }

    public List<Tile> GetOrthogonalNeighbors(Tile tile)
    {
        List<Tile> neighbors = new List<Tile>();

        Tile top = GetNeighborTop(tile);
        Tile bottom = GetNeighborBottom(tile);
        Tile left = GetNeighborLeft(tile);
        Tile right = GetNeighborRight(tile);

        if (top != null) neighbors.Add(top);
        if (bottom != null) neighbors.Add(bottom);
        if (left != null) neighbors.Add(left);
        if (right != null) neighbors.Add(right);

        return neighbors;
    }

    private Tile GetNeighbor(Tile origin, Vector2 direction)
    {
        if (origin == null || allTiles == null || allTiles.Length == 0)
        {
            return null;
        }

        EnsureNeighborLookup();

        if (!neighborByTile.TryGetValue(origin, out TileNeighborData neighbors))
        {
            return GetNeighborByPosition(origin, direction);
        }

        if (direction == Vector2.up)
        {
            return neighbors.Top;
        }

        if (direction == Vector2.down)
        {
            return neighbors.Bottom;
        }

        if (direction == Vector2.left)
        {
            return neighbors.Left;
        }

        if (direction == Vector2.right)
        {
            return neighbors.Right;
        }

        return GetNeighborByPosition(origin, direction);
    }

    private Tile GetNeighborByPosition(Tile origin, Vector2 direction)
    {
        if (origin == null || allTiles == null || allTiles.Length == 0)
        {
            return null;
        }

        if (gridStepX <= 0f || gridStepY <= 0f)
        {
            return null;
        }

        Vector3 targetPosition = origin.transform.position + new Vector3(direction.x * gridStepX, direction.y * gridStepY, 0f);
        float tolerance = Mathf.Max(0.001f, neighborTolerance);

        Tile bestTile = null;
        float bestDistance = float.MaxValue;

        for (int i = 0; i < allTiles.Length; i++)
        {
            Tile tile = allTiles[i];
            if (tile == null || tile == origin)
            {
                continue;
            }

            float distance = Vector2.Distance(tile.transform.position, targetPosition);
            if (distance <= tolerance && distance < bestDistance)
            {
                bestDistance = distance;
                bestTile = tile;
            }
        }

        return bestTile;
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

    /// <summary>
    /// Get the nearest tile (empty or occupied) to a position
    /// </summary>
    public Tile GetNearestTile(Vector3 position)
    {
        if (allTiles == null || allTiles.Length == 0) return null;

        Tile nearestTile = null;
        float shortestDistance = float.MaxValue;

        foreach (Tile tile in allTiles)
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

    /// <summary>
    /// Find which tile an item is sitting on (by ObjectContainer reference)
    /// </summary>
    public Tile GetTileForItem(GameObject item)
    {
        if (item == null) return null;

        foreach (Tile tile in allTiles)
        {
            if (tile.ObjectContainer == item)
                return tile;
        }

        return null;
    }

    public Tile RemoveItemFromAnyTile(GameObject item)
    {
        if (item == null || allTiles == null)
        {
            return null;
        }

        Tile firstTile = null;

        foreach (Tile tile in allTiles)
        {
            if (tile == null)
            {
                continue;
            }

            if (tile.ObjectContainer == item)
            {
                if (firstTile == null)
                {
                    firstTile = tile;
                }

                tile.RemoveObject();
            }
        }

        return firstTile;
    }


}
