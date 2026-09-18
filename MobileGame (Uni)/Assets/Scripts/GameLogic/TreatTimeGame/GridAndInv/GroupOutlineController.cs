using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Detects connected groups of same-tagged items on the grid.
/// - Rebuilds groups every frame (deferred in LateUpdate)
/// - Shows visual outlines around groups of 3+ items
/// - Fires HighlightedGroupsUpdated event so scoring system can calculate points
/// </summary>
public class GroupOutlineController : MonoBehaviour
{
    public static GroupOutlineController Instance;

    [System.Serializable]
    public class OutlineLevelStyle
    {
        [Range(1, 4)] public int level = 1;
        public Color color = Color.white;
    }

    /// <summary>
    /// Snapshot of a single highlighted group.
    /// Contains all items in the group (for reference) and their positions.
    /// </summary>
    [System.Serializable]
    public class HighlightedGroupSnapshot
    {
        public string Tag;                  // "Egg", "Chicken", etc. - items must match this to be in group
        public int Count;                   // Number of items in group
        public Tile RepresentativeTile;     // First tile (used as spawn point if no per-tile spawning)
        public Item RepresentativeItem;     // First item (used to get level for scoring)
        public List<Tile> Tiles;            // ALL tiles in the group (used for per-tile popup spawning)
    }

    [SerializeField] private bool logGroups = true;  // Print group detection to console (debug)
    [SerializeField] private List<OutlineLevelStyle> levelStyles = new List<OutlineLevelStyle>
    {
        new OutlineLevelStyle { level = 1 },
        new OutlineLevelStyle { level = 2 },
        new OutlineLevelStyle { level = 3 },
        new OutlineLevelStyle { level = 4 }
    };

    private bool refreshQueued;                     // Flag: do a refresh next LateUpdate?
    private readonly List<HighlightedGroupSnapshot> highlightedGroups = new List<HighlightedGroupSnapshot>();  // Current groups

    public IReadOnlyList<HighlightedGroupSnapshot> CurrentHighlightedGroups => highlightedGroups;
    public event System.Action<IReadOnlyList<HighlightedGroupSnapshot>> HighlightedGroupsUpdated;  // Fired when groups change

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        RequestRefresh();
    }

    private void LateUpdate()
    {
        if (!refreshQueued)
        {
            return;
        }

        refreshQueued = false;
        RefreshAllOutlines();
    }

    public void RequestRefresh()
    {
        refreshQueued = true;
    }

    public void ForceRefreshNow()
    {
        refreshQueued = false;
        RefreshAllOutlines();
    }

    public void RefreshOutlinesAround(Tile changedTile)
    {
        // Full refresh keeps behavior deterministic and avoids stale links after merges/swaps.
        RequestRefresh();
    }

    public void RefreshAllOutlines()
    {
        // Get grid reference
        GridManager gm = GridManager.instance != null ? GridManager.instance : FindObjectOfType<GridManager>();
        if (gm == null || gm.allTiles == null || gm.allTiles.Length == 0)
        {
            highlightedGroups.Clear();
            HighlightedGroupsUpdated?.Invoke(CurrentHighlightedGroups);
            return;
        }

        // Clear previous state
        highlightedGroups.Clear();

        // Hide all outlines first (we'll re-enable only for groups of 3+)
        foreach (Tile tile in gm.allTiles)
        {
            if (tile != null)
            {
                tile.HideOutline();
            }
        }

        // === DETECT ALL GROUPS ===
        HashSet<Tile> visited = new HashSet<Tile>();  // Track visited tiles to avoid reprocessing

        foreach (Tile tile in gm.allTiles)
        {
            // Skip if already visited, empty, or no item
            if (tile == null || visited.Contains(tile) || !gm.IsOccupied(tile))
            {
                continue;
            }

            string groupTag = gm.GetItemTagForTile(tile);  // Get tag of item on this tile
            if (string.IsNullOrEmpty(groupTag))
            {
                visited.Add(tile);
                continue;
            }

            // Use BFS to find all connected tiles with same tag (GetConnectedTagGroup below)
            List<Tile> group = GetConnectedTagGroup(gm, tile, groupTag, visited);

            if (logGroups)
            {
                LogGroup(groupTag, group);
            }

            // Only highlight groups with 3+ items
            if (group.Count < 3)
            {
                continue;
            }

            // Create snapshot for scoring system
            Tile representativeTile = group[0];
            Item representativeItem = representativeTile != null && representativeTile.ObjectContainer != null
                ? representativeTile.ObjectContainer.GetComponent<Item>()
                : null;

            if (!IsGridScoreableItem(representativeItem))
            {
                continue;
            }

            int outlineLevel = GetOutlineLevel(representativeItem);
            Color outlineColor = GetOutlineColor(outlineLevel);

            highlightedGroups.Add(new HighlightedGroupSnapshot
            {
                Tag = groupTag,
                Count = group.Count,
                RepresentativeTile = representativeTile,
                RepresentativeItem = representativeItem,
                Tiles = new List<Tile>(group)
            });

            // === SHOW OUTLINES ===
            // For each tile in group, show outline on edges that border non-group tiles
            HashSet<Tile> groupSet = new HashSet<Tile>(group);  // Fast lookup
            foreach (Tile groupTile in group)
            {
                // Check all 4 neighbors
                Tile top = gm.GetNeighborTop(groupTile);
                Tile bottom = gm.GetNeighborBottom(groupTile);
                Tile left = gm.GetNeighborLeft(groupTile);
                Tile right = gm.GetNeighborRight(groupTile);

                // Show outline edge if neighbor is NOT in group
                bool showTop = !groupSet.Contains(top);
                bool showBottom = !groupSet.Contains(bottom);
                bool showLeft = !groupSet.Contains(left);
                bool showRight = !groupSet.Contains(right);

                groupTile.ApplyOutline(showTop, showBottom, showLeft, showRight, outlineColor, outlineLevel);
            }
        }

        // Notify scoring system of the new group list
        HighlightedGroupsUpdated?.Invoke(CurrentHighlightedGroups);
    }

    private bool IsGridScoreableItem(Item item)
    {
        if (item is not Bowl bowl)
        {
            return item != null;
        }

        if (bowl.ItemInBowl == null)
        {
            return false;
        }

        Item itemInBowl = bowl.ItemInBowl.GetComponent<Item>();
        return itemInBowl != null && itemInBowl.IsCookedFood;
    }

    private int GetOutlineLevel(Item item)
    {
        return item is Bowl ? 4 : item.ScoreLevel;
    }

    private Color GetOutlineColor(int level)
    {
        int clampedLevel = Mathf.Clamp(level, 1, 4);

        for (int i = 0; i < levelStyles.Count; i++)
        {
            if (levelStyles[i] != null && levelStyles[i].level == clampedLevel)
            {
                return levelStyles[i].color;
            }
        }

        return Color.white;
    }

    private List<Tile> GetConnectedTagGroup(GridManager gm, Tile start, string groupTag, HashSet<Tile> visited)
    {
        List<Tile> group = new List<Tile>();
        Queue<Tile> queue = new Queue<Tile>();

        visited.Add(start);
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            Tile current = queue.Dequeue();
            group.Add(current);

            foreach (Tile neighbor in gm.GetOrthogonalNeighbors(current))
            {
                if (neighbor == null || visited.Contains(neighbor) || !gm.IsOccupied(neighbor))
                {
                    continue;
                }

                string neighborTag = gm.GetItemTagForTile(neighbor);
                if (neighborTag != groupTag)
                {
                    continue;
                }

                visited.Add(neighbor);
                queue.Enqueue(neighbor);
            }
        }

        return group;
    }

    private void LogGroup(string groupTag, List<Tile> group)
    {
        string tileList = string.Join(", ", group.Select(t => t != null ? t.name : "null"));
        Debug.Log($"[GroupOutline] Tag={groupTag} Count={group.Count} Tiles=[{tileList}]");
    }
}
