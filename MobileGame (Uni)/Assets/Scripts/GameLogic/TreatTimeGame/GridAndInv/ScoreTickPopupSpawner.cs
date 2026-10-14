using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Listens to GridScoringSystem tick events and spawns floating "+score" popups.
/// - Subscribes to TickAwardsCalculated event
/// - Takes popups from ScoreTickPopupPool (same GameObject by default)
/// - Adds score to NewScoreManager at the moment each popup spawns
/// </summary>
public class ScoreTickPopupSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridScoringSystem scoreSystem;
    [SerializeField] private ScoreTickPopupPool popupPool;

    [Header("Spawn Behavior")]
    [SerializeField] private bool spawnOnEachTile = true;
    [SerializeField, Min(0f)] private float perTileSpawnDelay = 0.2f;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.35f, 0f);

    [Header("Debug")]
    [SerializeField] private bool logSpawns;

    private WaitForSeconds tileDelay; // cached to avoid a GC alloc every staggered spawn

    private void Awake()
    {
        if (popupPool == null)
        {
            popupPool = GetComponent<ScoreTickPopupPool>();
        }

        if (perTileSpawnDelay > 0f)
        {
            tileDelay = new WaitForSeconds(perTileSpawnDelay);
        }
    }

    private void OnEnable()
    {
        if (scoreSystem == null)
        {
            scoreSystem = GridScoringSystem.Instance != null
                ? GridScoringSystem.Instance
                : FindObjectOfType<GridScoringSystem>();
        }

        if (scoreSystem != null)
        {
            scoreSystem.TickAwardsCalculated += OnTickAwardsCalculated;
        }
    }

    private void OnDisable()
    {
        if (scoreSystem != null)
        {
            scoreSystem.TickAwardsCalculated -= OnTickAwardsCalculated;
        }
    }

    private void OnTickAwardsCalculated(IReadOnlyList<GridScoringSystem.GroupTickAward> awards, int tickTotal)
    {
        if (popupPool == null || awards == null || awards.Count == 0)
        {
            return;
        }

        for (int i = 0; i < awards.Count; i++)
        {
            GridScoringSystem.GroupTickAward award = awards[i];
            if (award == null || award.Award <= 0)
            {
                continue;
            }

            if (spawnOnEachTile && award.Tiles != null && award.Tiles.Count > 0)
            {
                StartCoroutine(SpawnPerTile(award));
            }
            else
            {
                SpawnSingle(award.Award, award.Level, award.WorldPosition);
            }
        }

        if (logSpawns)
        {
            Debug.Log($"[ScoreTickPopupSpawner] Spawned popups for tick total={tickTotal}, groups={awards.Count}");
        }
    }

    private IEnumerator SpawnPerTile(GridScoringSystem.GroupTickAward award)
    {
        // award.Award is already validated > 0 by the caller and is flat per tile,
        // so no need to re-check it inside the loop.
        List<Tile> tiles = award.Tiles;
        int tileCount = tiles.Count;

        for (int i = 0; i < tileCount; i++)
        {
            Tile tile = tiles[i];
            if (tile == null)
            {
                continue;
            }

            SpawnSingle(award.Award, award.Level, tile.transform.position);

            if (tileDelay != null && i < tileCount - 1)
            {
                yield return tileDelay;
            }
        }
    }

    private void SpawnSingle(int amount, int level, Vector3 origin)
    {
        Vector3 spawnPosition = origin + worldOffset;

        ScoreTickPopup popup = popupPool.Get(spawnPosition, Quaternion.identity);
        popup.Play(amount, level);

        if (NewScoreManager.Instance != null)
        {
            NewScoreManager.Instance.AddScore(amount);
        }
    }
}