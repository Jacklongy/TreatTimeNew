using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Listens to GridScoringSystem tick events and spawns floating "+score" popups.
/// - Subscribes to TickAwardsCalculated event
/// - Draws popups from a pool per tile (or per group)
/// - Adds score to NewScoreManager at the moment each popup spawns
/// </summary>
public class ScoreTickPopupSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridScoringSystem scoreSystem;
    [SerializeField] private ScoreTickPopup popupPrefab;
    [SerializeField] private Transform popupParent;

    [Header("Spawn Behavior")]
    [SerializeField] private bool spawnOnEachTile = true;
    [SerializeField, Min(0f)] private float perTileSpawnDelay = 0.2f;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.35f, 0f);
    [SerializeField, Min(0f)] private float randomJitter = 0.06f;

    [Header("Pooling")]
    [SerializeField, Min(1)] private int poolDefaultCapacity = 20;
    [SerializeField, Min(1)] private int poolMaxSize = 200;

    [Header("Debug")]
    [SerializeField] private bool logSpawns;

    private ObjectPool<ScoreTickPopup> popupPool;
    private WaitForSeconds tileDelay; // cached to avoid a GC alloc every staggered spawn

    private void Awake()
    {
        popupPool = new ObjectPool<ScoreTickPopup>(
            createFunc: () =>
            {
                ScoreTickPopup popup = Instantiate(popupPrefab, popupParent);
                popup.SetOwner(this);
                return popup;
            },
            actionOnGet: popup => popup.gameObject.SetActive(true),
            actionOnRelease: popup => popup.gameObject.SetActive(false),
            actionOnDestroy: popup =>
            {
                if (popup != null) Destroy(popup.gameObject);
            },
            collectionCheck: false,
            defaultCapacity: poolDefaultCapacity,
            maxSize: poolMaxSize);

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

        if (popupParent == null)
        {
            popupParent = transform;
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
        if (popupPrefab == null || awards == null || awards.Count == 0)
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
        Vector2 jitter2D = Random.insideUnitCircle * randomJitter;
        Vector3 spawnPosition = origin + worldOffset + (Vector3)jitter2D;

        ScoreTickPopup popup = popupPool.Get();
        popup.transform.SetPositionAndRotation(spawnPosition, Quaternion.identity);
        popup.Play(amount, level);

        if (NewScoreManager.Instance != null)
        {
            NewScoreManager.Instance.AddScore(amount);
        }
    }

    /// <summary>
    /// Returns a finished popup to the pool. Called by ScoreTickPopup when its animation exits.
    /// </summary>
    public void ReleasePopup(ScoreTickPopup popup)
    {
        popupPool.Release(popup);
    }
}