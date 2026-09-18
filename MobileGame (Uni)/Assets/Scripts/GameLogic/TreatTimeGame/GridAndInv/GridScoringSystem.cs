using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Calculates grid-highlight score and reports it to NewScoreManager.
/// - Detects highlighted groups via GroupOutlineController
/// - Calculates score every tick (1 second default) per group
/// - Fires TickAwardsCalculated event so spawner can create popups
/// </summary>
public class GridScoringSystem : MonoBehaviour
{
    public static GridScoringSystem Instance;

    /// <summary>
    /// Data packet sent to spawner when a group ticks.
    /// Contains: tag, level, count of items, award amount, world position, and all tiles in group.
    /// </summary>
    [Serializable]
    public class GroupTickAward
    {
        public string Tag;                  // "Egg", "Chicken", etc.
        public int Level;                   // 1, 2, 3, or 4 (food upgrade level)
        public int Count;                   // How many items in the group
        public int Award;                   // Flat score per tile this tick (equals the level's base score)
        public Vector3 WorldPosition;       // Where to spawn popup (representative tile position)
        public Item RepresentativeItem;     // First item in group
        public List<Tile> Tiles;            // ALL tiles in the group (for per-tile popup spawning)
    }

    [Header("Tick Scoring")]
    [SerializeField] private bool scoringEnabled = true;            // Master on/off toggle
    [SerializeField] private bool pauseWhenTimeScaleZero = true;    // Pause scoring if game is paused (Time.timeScale = 0)

    [Header("Item Level Base Score")]
    [SerializeField, Min(0)] private int level1BaseScore = 1;       // Level 1 items: 1 point per tick
    [SerializeField, Min(0)] private int level2BaseScore = 2;       // Level 2 items: 2 points per tick
    [SerializeField, Min(0)] private int level3BaseScore = 3;       // Level 3 items: 3 points per tick
    [SerializeField, Min(0)] private int level4BaseScore = 4;       // Level 4 merged food: 4 points per tick

    [Header("Tick Speed")]
    [SerializeField, Min(0.05f)] private float baseTickInterval = 1f;             // Same base tick speed for every level
    [SerializeField, Min(0f)] private float groupSizeSpeedMultiplier = 0.2f;     // Each item in the group speeds up ticking by this much

    [Header("Safety")]
    [SerializeField, Min(0)] private int maxScorePerTick = 9999;    // Cap total score awarded in a single frame (rarely needed)

    [Header("Debug")]
    [SerializeField] private bool logTickBreakdown;                 // Print detailed breakdown of each tick calculation to console

    private GroupOutlineController groupOutlineController;
    // One timer per group (identified by GetGroupKey). Tracks time since last tick for that group.
    // Allows each group to tick independently without syncing to a global timer.
    private readonly Dictionary<string, float> groupTickTimers = new Dictionary<string, float>();

    // Tracking stats
    public int LastTickAward { get; private set; }          // Total points awarded in the last frame tick
    public int ActiveGroupCount { get; private set; }       // Number of highlighted groups currently
    public int ActiveHighlightedItemCount { get; private set; }  // Total items in all active groups

    // Event fired to notify listeners (e.g., popup spawner)
    public event Action<IReadOnlyList<GroupTickAward>, int> TickAwardsCalculated;  // (awards list, total award) - fired when groups tick

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        ResolveGroupController();
    }

    private void Start()
    {
        ResolveGroupController();
        groupOutlineController?.ForceRefreshNow();
    }

    private void Update()
    {
        if (!scoringEnabled)
        {
            return;
        }

        if (pauseWhenTimeScaleZero && Mathf.Approximately(Time.timeScale, 0f))
        {
            return;
        }

        ApplyTickScore(Time.deltaTime);
    }

    private void ResolveGroupController()
    {
        if (groupOutlineController == null)
        {
            groupOutlineController = GroupOutlineController.Instance != null
                ? GroupOutlineController.Instance
                : FindObjectOfType<GroupOutlineController>();
        }
    }

    private void ApplyTickScore(float deltaTime)
    {
        // Ensure we have a reference to the group controller
        ResolveGroupController();

        // Bail out if no controller or no groups
        if (groupOutlineController == null)
        {
            groupTickTimers.Clear();
            LastTickAward = 0;
            ActiveGroupCount = 0;
            ActiveHighlightedItemCount = 0;
            return;
        }

        IReadOnlyList<GroupOutlineController.HighlightedGroupSnapshot> groups = groupOutlineController.CurrentHighlightedGroups;

        // Bail out if no highlighted groups exist
        if (groups == null || groups.Count == 0)
        {
            groupTickTimers.Clear();
            LastTickAward = 0;
            ActiveGroupCount = 0;
            ActiveHighlightedItemCount = 0;
            return;
        }

        // Accumulate scores for this frame
        int tickAward = 0;
        int highlightedItemCount = 0;
        int validGroupCount = 0;
        List<GroupTickAward> tickAwards = new List<GroupTickAward>();  // All groups that ticked this frame
        HashSet<string> activeGroupKeys = new HashSet<string>();      // Track which groups are still highlighted (used to clean up stale timers)

        // === PROCESS EACH HIGHLIGHTED GROUP ===
        for (int i = 0; i < groups.Count; i++)
        {
            GroupOutlineController.HighlightedGroupSnapshot group = groups[i];
            if (group == null || group.Count < 3)
            {
                continue;  // Skip single items or null groups
            }

            // Get item level from first item in group
            if (!TryGetGroupScoreLevel(group, out int level))
            {
                continue;
            }

            float speedMultiplier = 1f + (group.Count * groupSizeSpeedMultiplier);  // Bigger groups tick faster, not for more score
            float levelTickInterval = Mathf.Max(0.05f, baseTickInterval / speedMultiplier);
            string groupKey = GetGroupKey(group);  // Create stable unique ID for this group
            activeGroupKeys.Add(groupKey);
            validGroupCount++;
            highlightedItemCount += group.Count;

            // Get or create timer for this group
            if (!groupTickTimers.TryGetValue(groupKey, out float timer))
            {
                timer = 0f;
            }

            // Increment timer by deltaTime
            timer += deltaTime;

            // === CHECK IF GROUP SHOULD TICK ===
            // May tick multiple times in one frame if deltaTime is large (e.g., after lag spike)
            while (timer >= levelTickInterval)
            {
                timer -= levelTickInterval;  // Deduct one tick

                // Score per tile equals the level's base score directly (level 1 = +1, level 2 = +2, etc).
                // Group size no longer multiplies this amount; it only affects tick speed above.
                int levelBaseScore = GetBaseScoreForLevel(level);
                int groupAward = levelBaseScore;

                groupAward = Mathf.Max(0, groupAward);  // Clamp to non-negative

                // Check if adding this would exceed maxScorePerTick cap
                if (maxScorePerTick > 0)
                {
                    int remaining = Mathf.Max(0, maxScorePerTick - tickAward);
                    groupAward = Mathf.Min(groupAward, remaining);
                }

                // Skip if capped to zero
                if (groupAward <= 0)
                {
                    if (maxScorePerTick > 0 && tickAward >= maxScorePerTick)
                    {
                        break;  // Exit inner while loop; we've hit the cap
                    }
                    continue;  // Skip this tick but keep the timer
                }

                // Add to this frame's total
                tickAward += groupAward;

                // Determine popup spawn position (representative tile or fallback to item)
                Vector3 worldPosition = Vector3.zero;
                if (group.RepresentativeTile != null)
                {
                    worldPosition = group.RepresentativeTile.transform.position;
                }
                else if (group.RepresentativeItem != null)
                {
                    worldPosition = group.RepresentativeItem.transform.position;
                }

                // Create award record for spawner (will create one popup per tile if splitAwardAcrossTiles = true)
                tickAwards.Add(new GroupTickAward
                {
                    Tag = group.Tag,
                    Level = level,
                    Count = group.Count,
                    Award = groupAward,
                    WorldPosition = worldPosition,
                    RepresentativeItem = group.RepresentativeItem,
                    Tiles = group.Tiles != null ? new List<Tile>(group.Tiles) : new List<Tile>()
                });

                // Optional: log breakdown of calculation
                if (logTickBreakdown)
                {
                    Debug.Log($"[GridScoringSystem] Group tag={group.Tag} level={level} count={group.Count} base={levelBaseScore} award={groupAward} interval={levelTickInterval:F2}s");
                }

                // Exit if we've hit the score cap for this frame
                if (maxScorePerTick > 0 && tickAward >= maxScorePerTick)
                {
                    break;  // Exit inner while loop
                }
            }

            // Store updated timer for next frame
            groupTickTimers[groupKey] = timer;
        }

        // === CLEANUP: Remove timers for groups that are no longer highlighted ===
        // This prevents memory leak if groups constantly form and disappear
        if (groupTickTimers.Count > 0)
        {
            List<string> staleKeys = new List<string>();
            foreach (KeyValuePair<string, float> pair in groupTickTimers)
            {
                if (!activeGroupKeys.Contains(pair.Key))
                {
                    staleKeys.Add(pair.Key);  // Mark for removal
                }
            }

            for (int i = 0; i < staleKeys.Count; i++)
            {
                groupTickTimers.Remove(staleKeys[i]);
            }
        }

        // === UPDATE STATS ===
        ActiveGroupCount = validGroupCount;
        ActiveHighlightedItemCount = highlightedItemCount;
        LastTickAward = tickAward;

        // === FIRE EVENTS ===
        if (tickAwards.Count > 0)
        {
            TickAwardsCalculated?.Invoke(tickAwards, tickAward);  // Notify spawner to create popups; spawner adds score per popup
        }
    }

    private int GetBaseScoreForLevel(int level)
    {
        // Returns the base score for an item at this level (before group size multiplier)
        switch (Mathf.Clamp(level, 1, 4))
        {
            case 1:
                return level1BaseScore;  // 1 point
            case 2:
                return level2BaseScore;  // 2 points
            case 3:
                return level3BaseScore;  // 3 points
            case 4:
                return level4BaseScore;  // 4 points
            default:
                return level1BaseScore;
        }
    }

    private bool TryGetGroupScoreLevel(GroupOutlineController.HighlightedGroupSnapshot group, out int level)
    {
        level = 1;

        if (group.RepresentativeItem is Bowl bowl)
        {
            if (bowl.ItemInBowl == null)
            {
                return false;
            }

            Item itemInBowl = bowl.ItemInBowl.GetComponent<Item>();
            if (itemInBowl == null || !itemInBowl.IsCookedFood)
            {
                return false;
            }

            level = 4;
            return true;
        }

        if (group.RepresentativeItem == null)
        {
            return false;
        }

        level = group.RepresentativeItem.ScoreLevel;
        return true;
    }

    private string GetGroupKey(GroupOutlineController.HighlightedGroupSnapshot group)
    {
        // Creates a unique, stable ID for this group based on the tiles it contains.
        // Used to track per-group timers across frames (so each group ticks independently).
        // Why stable ID? If we just used object references, timers would break when groups reform.
        
        if (group == null)
        {
            return "group:null";
        }

        if (group.Tiles == null || group.Tiles.Count == 0)
        {
            // Fallback: use representative tile's instance ID
            int fallbackId = group.RepresentativeTile != null ? group.RepresentativeTile.GetInstanceID() : 0;
            return $"{group.Tag}:{fallbackId}";
        }

        // Create key from all tile IDs (sorted so order doesn't matter)
        int[] ids = new int[group.Tiles.Count];
        int idCount = 0;

        for (int i = 0; i < group.Tiles.Count; i++)
        {
            Tile tile = group.Tiles[i];
            if (tile == null)
            {
                continue;
            }

            ids[idCount] = tile.GetInstanceID();
            idCount++;
        }

        if (idCount == 0)
        {
            int fallbackId = group.RepresentativeTile != null ? group.RepresentativeTile.GetInstanceID() : 0;
            return $"{group.Tag}:{fallbackId}";
        }

        Array.Sort(ids, 0, idCount);  // Sort so same tiles always produce same key

        StringBuilder builder = new StringBuilder(group.Tag);
        builder.Append(':');
        for (int i = 0; i < idCount; i++)
        {
            builder.Append(ids[i]);
            builder.Append(',');
        }

        return builder.ToString();
    }
}
