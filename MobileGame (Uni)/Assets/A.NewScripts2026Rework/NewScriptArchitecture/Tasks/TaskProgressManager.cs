using System;
using System.Collections.Generic;
using UnityEngine;

public class TaskProgressManager : MonoBehaviour
{
    public static TaskProgressManager Instance { get; private set; }
    public static event Action Changed;
    public int DuplicateItemCoins => duplicateItemCoins;

    [SerializeField] private TaskCatalog catalog;
    [SerializeField] private ShopCatalog shopCatalog;
    [Min(1)] [SerializeField] private int duplicateItemCoins = 100;
    [Min(1)] [SerializeField] private float saveIntervalSeconds = 10f;

    private const int TasksPerPeriod = 3;
    private bool ready;
    private bool dirty;
    private bool selectionNeedsSave;
    private bool unavailableReported;
    private static bool missingReported;
    private float nextSaveAt;
    private float nextPeriodCheckAt;
    private PlayerProfileData initializedProfile;

    private PlayerProfileData Profile =>
        PlayerProfileHolder.Instance != null ? PlayerProfileHolder.Instance.CurrentData : null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public bool Initialize()
    {
        ready = false;
        if (Profile == null || catalog == null)
        {
            Debug.LogError("Task manager needs a loaded profile and a task catalog.", this);
            return false;
        }
        if (Profile.TaskStatistics == null) Profile.TaskStatistics = new TaskStatistics();
        if (Profile.TaskPeriods == null) Profile.TaskPeriods = new List<SavedTaskPeriod>();
        initializedProfile = Profile;
        ready = true;
        if (!EnsurePeriods())
        {
            ready = false;
            return false;
        }
        unavailableReported = false;
        missingReported = false;
        Changed?.Invoke();
        return true;
    }

    private static string PeriodKey(TaskTime period, DateTime now)
    {
        DateTime date = now.ToUniversalTime().Date;
        switch (period)
        {
            case TaskTime.daily: return date.ToString("yyyy-MM-dd");
            case TaskTime.weekly:
                return date.AddDays(-((int)date.DayOfWeek + 6) % 7).ToString("yyyy-MM-dd");
            case TaskTime.monthly: return date.ToString("yyyy-MM");
            default: throw new ArgumentOutOfRangeException(nameof(period));
        }
    }

    private bool EnsurePeriods()
    {
        if (!ready || Profile == null) return false;
        if (Profile != initializedProfile) return Initialize();
        DateTime now = DateTime.UtcNow;
        foreach (TaskTime period in new[] { TaskTime.daily, TaskTime.weekly, TaskTime.monthly })
        {
            string key = PeriodKey(period, now);
            SavedTaskPeriod existing = Profile.TaskPeriods.Find(p => p != null && p.Period == period);
            if (existing != null && existing.PeriodKey == key) continue;

            if (!catalog.TryGetTasks(period, out List<TaskDefinition> candidates) ||
                !catalog.TryGetBonuses(period, out List<TaskBonusOption> bonuses))
                return false;
            if (candidates.Count == 0)
            {
                Debug.LogError($"No {period} tasks are configured.", catalog);
                return false;
            }

            var selected = new SavedTaskPeriod { Period = period, PeriodKey = key };
            int count = Math.Min(TasksPerPeriod, candidates.Count);
            for (int i = 0; i < count; i++)
            {
                int index = UnityEngine.Random.Range(0, candidates.Count);
                selected.Tasks.Add(new SavedTaskProgress { TaskId = candidates[index].Id });
                candidates.RemoveAt(index);
            }
            if (count < TasksPerPeriod)
                Debug.LogWarning($"Only {count} {period} tasks are configured; the completion bonus requires three.", catalog);

            TaskBonusOption bonus = bonuses[UnityEngine.Random.Range(0, bonuses.Count)];
            selected.Bonus = new SavedTaskBonus
            {
                Type = bonus.type,
                Amount = bonus.amount,
                ItemId = bonus.item != null ? bonus.item.Id : null,
                ItemName = bonus.item != null ? bonus.item.DisplayName : null
            };
            if (existing != null) Profile.TaskPeriods.Remove(existing);
            Profile.TaskPeriods.Add(selected);
            dirty = true;
            selectionNeedsSave = true;
        }
        if (selectionNeedsSave && !SaveCheckpoint()) return false;
        return true;
    }

    public bool TryGetPeriod(TaskTime period, out SavedTaskPeriod saved)
    {
        saved = null;
        if (!EnsurePeriods()) return false;
        saved = Profile.TaskPeriods.Find(p => p != null && p.Period == period);
        return saved != null;
    }

    public bool TryGetDefinition(string id, out TaskDefinition definition)
    {
        definition = null;
        return ready && catalog.TryGetById(id, out definition);
    }

    public bool TryGetRewardItem(string id, out ShopItemData item)
    {
        item = shopCatalog != null ? shopCatalog.GetById(id) : null;
        return item != null;
    }

    public static void ReportMerge(string itemTag)
    {
        if (string.IsNullOrWhiteSpace(itemTag))
        {
            Debug.LogError("Cannot report a merge without an item tag.");
            return;
        }
        if (Instance != null) Instance.Report(TaskMetric.Merge, itemTag, 1);
        else ReportMissingManager();
    }

    public static void ReportMealFed()
    {
        if (Instance != null) Instance.Report(TaskMetric.Feed, null, 1);
        else ReportMissingManager();
    }

    public static void ReportScore(int score)
    {
        if (score <= 0)
        {
            Debug.LogError("Cannot report a score that is zero or negative.");
            return;
        }
        if (Instance != null) Instance.Report(TaskMetric.Score, null, score);
        else ReportMissingManager();
    }

    private static void ReportMissingManager()
    {
        if (missingReported) return;
        missingReported = true;
        Debug.LogError("Task manager is missing; task progress will not be recorded.");
    }

    private void Report(TaskMetric metric, string tag, int amount)
    {
        if (!EnsurePeriods())
        {
            if (!unavailableReported)
            {
                unavailableReported = true;
                Debug.LogError("Task progress cannot be recorded until the task system is initialized.", this);
            }
            return;
        }
        unavailableReported = false;

        TaskStatistics stats = Profile.TaskStatistics;
        if (metric == TaskMetric.Merge)
        {
            if (stats.MergeCounts == null) stats.MergeCounts = new List<TaskMergeCount>();
            TaskMergeCount entry = stats.MergeCounts.Find(m => m != null && m.ItemTag == tag);
            if (entry == null)
            {
                entry = new TaskMergeCount { ItemTag = tag };
                stats.MergeCounts.Add(entry);
            }
            if (entry.Count == long.MaxValue) { Debug.LogError("Merge counter overflow.", this); return; }
            entry.Count++;
        }
        else if (metric == TaskMetric.Feed)
        {
            if (stats.MealsFed == long.MaxValue) { Debug.LogError("Meal counter overflow.", this); return; }
            stats.MealsFed++;
        }
        else
        {
            if (stats.ScoreEarned > long.MaxValue - amount) { Debug.LogError("Score counter overflow.", this); return; }
            stats.ScoreEarned += amount;
        }

        foreach (SavedTaskPeriod period in Profile.TaskPeriods)
        {
            foreach (SavedTaskProgress progress in period.Tasks)
            {
                if (!catalog.TryGetById(progress.TaskId, out TaskDefinition definition)) continue;
                if (progress.Claimed || progress.Progress >= definition.Goal || definition.Metric != metric ||
                    (metric == TaskMetric.Merge && definition.MergeTag != tag)) continue;
                progress.Progress = (int)Math.Min(definition.Goal, (long)progress.Progress + amount);
            }
        }
        dirty = true;
        Changed?.Invoke();
    }

    public void StartRun()
    {
        if (!EnsurePeriods()) return;
        foreach (SavedTaskPeriod period in Profile.TaskPeriods)
        {
            foreach (SavedTaskProgress progress in period.Tasks)
            {
                if (catalog.TryGetById(progress.TaskId, out TaskDefinition definition) &&
                    definition.SingleRun && !progress.Claimed && progress.Progress < definition.Goal &&
                    progress.Progress != 0)
                {
                    progress.Progress = 0;
                    dirty = true;
                }
            }
        }
        Changed?.Invoke();
    }

    public bool CanClaimBonus(SavedTaskPeriod period)
    {
        if (period == null || period.Bonus == null || period.Bonus.Claimed ||
            period.Tasks == null || period.Tasks.Count != TasksPerPeriod) return false;
        foreach (SavedTaskProgress task in period.Tasks)
            if (task == null || !task.Claimed) return false;
        return true;
    }

    public bool ClaimTask(TaskTime period, string id)
    {
        if (!TryGetPeriod(period, out SavedTaskPeriod saved) ||
            !catalog.TryGetById(id, out TaskDefinition definition)) return false;
        SavedTaskProgress task = saved.Tasks.Find(t => t.TaskId == id);
        if (task == null || task.Claimed || task.Progress < definition.Goal) return false;
        if (definition.RewardType == TaskRewardType.Item &&
            (shopCatalog == null || shopCatalog.GetById(definition.RewardItem.Id) == null))
        {
            Debug.LogError($"Task reward item '{definition.RewardItem.Id}' is missing from the shop catalog.", this);
            return false;
        }
        return ClaimReward(definition.RewardType, definition.RewardAmount, definition.RewardItem,
            () => task.Claimed = true, () => task.Claimed = false);
    }

    public bool ClaimBonus(TaskTime period)
    {
        if (!TryGetPeriod(period, out SavedTaskPeriod saved) || !CanClaimBonus(saved)) return false;
        ShopItemData item = null;
        if (saved.Bonus.Type == TaskRewardType.Item)
        {
            if (!TryGetRewardItem(saved.Bonus.ItemId, out item))
            {
                Debug.LogError($"Bonus item '{saved.Bonus.ItemId}' is missing from the shop catalog.", this);
                return false;
            }
        }
        return ClaimReward(saved.Bonus.Type, saved.Bonus.Amount, item,
            () => saved.Bonus.Claimed = true, () => saved.Bonus.Claimed = false);
    }

    private bool ClaimReward(TaskRewardType type, int amount, ShopItemData item, Action mark, Action undo)
    {
        PlayerProfileData profile = Profile;
        if (!Enum.IsDefined(typeof(TaskRewardType), type))
        {
            Debug.LogError("Cannot claim an invalid reward type.", this);
            return false;
        }
        int oldCoins = profile.Coins;
        int oldGems = profile.Gems;
        bool addedItem = false;
        if (type == TaskRewardType.Item)
        {
            if (item == null || string.IsNullOrWhiteSpace(item.Id) || profile.PurchasedItemIds == null)
            {
                Debug.LogError("Cannot claim an invalid item reward or inventory.", this);
                return false;
            }
            if (PlayerInventory.IsOwned(item))
            {
                type = TaskRewardType.Coin;
                amount = duplicateItemCoins;
            }
            else
            {
                PlayerInventory.AddOwned(item);
                addedItem = true;
            }
        }

        if (type == TaskRewardType.Coin || type == TaskRewardType.Gem)
        {
            int current = type == TaskRewardType.Coin ? profile.Coins : profile.Gems;
            if (amount <= 0 || current > int.MaxValue - amount)
            {
                Debug.LogError("Cannot claim an invalid or overflowing currency reward.", this);
                return false;
            }
            if (type == TaskRewardType.Coin) profile.Coins += amount;
            else profile.Gems += amount;
        }
        mark();
        dirty = true;
        if (!SaveCheckpoint())
        {
            undo();
            profile.Coins = oldCoins;
            profile.Gems = oldGems;
            if (addedItem)
            {
                profile.PurchasedItemIds.Remove(item.Id);
                PlayerInventory.NotifyOwnershipChanged();
            }
            Changed?.Invoke();
            return false;
        }
        Changed?.Invoke();
        return true;
    }

    public bool SaveCheckpoint()
    {
        if (!dirty) return true;
        if (LocalSaveStorage.Instance == null || !LocalSaveStorage.Instance.Save())
        {
            Debug.LogError("Could not save task progress locally.", this);
            return false;
        }
        dirty = false;
        selectionNeedsSave = false;
        nextSaveAt = Time.realtimeSinceStartup + Mathf.Max(1f, saveIntervalSeconds);
        _ = ProfileSyncService.SyncSavedProfileAsync();
        return true;
    }

    private void Update()
    {
        if (ready && Time.realtimeSinceStartup >= nextPeriodCheckAt)
        {
            nextPeriodCheckAt = Time.realtimeSinceStartup + 1f;
            bool expired = Profile != initializedProfile;
            if (!expired && Profile != null && Profile.TaskPeriods != null)
            {
                DateTime now = DateTime.UtcNow;
                foreach (SavedTaskPeriod period in Profile.TaskPeriods)
                    if (period != null && period.PeriodKey != PeriodKey(period.Period, now))
                        expired = true;
            }
            if (expired && EnsurePeriods()) Changed?.Invoke();
        }
        if (dirty && Time.realtimeSinceStartup >= nextSaveAt) SaveCheckpoint();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused) SaveCheckpoint();
    }

    private void OnApplicationQuit()
    {
        SaveCheckpoint();
    }
}
