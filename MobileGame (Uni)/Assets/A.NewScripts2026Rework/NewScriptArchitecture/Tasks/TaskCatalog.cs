using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TaskBonusOption
{
    public TaskRewardType type;
    [Min(1)] public int amount = 1;
    public ShopItemData item;
}

/// <summary>References the task assets available for selection and resolves their saved IDs.</summary>
[CreateAssetMenu(fileName = "TaskCatalog", menuName = "Tasks/Task Catalog")]
public class TaskCatalog : ScriptableObject
{
    [HideInInspector]
    [SerializeField] private List<TaskDefinition> tasks = new List<TaskDefinition>();

    [Header("Daily")]
    [SerializeField] private List<TaskDefinition> dailyTasks = new List<TaskDefinition>();
    [SerializeField] private List<TaskBonusOption> dailyBonuses = new List<TaskBonusOption>();

    [Header("Weekly")]
    [SerializeField] private List<TaskDefinition> weeklyTasks = new List<TaskDefinition>();
    [SerializeField] private List<TaskBonusOption> weeklyBonuses = new List<TaskBonusOption>();

    [Header("Monthly")]
    [SerializeField] private List<TaskDefinition> monthlyTasks = new List<TaskDefinition>();
    [SerializeField] private List<TaskBonusOption> monthlyBonuses = new List<TaskBonusOption>();

    public bool TryGetBonuses(TaskTime period, out List<TaskBonusOption> bonuses)
    {
        switch (period)
        {
            case TaskTime.daily: bonuses = dailyBonuses; break;
            case TaskTime.weekly: bonuses = weeklyBonuses; break;
            case TaskTime.monthly: bonuses = monthlyBonuses; break;
            default:
                bonuses = null;
                Debug.LogError($"Invalid task period: {period}.", this);
                return false;
        }

        if (bonuses == null || bonuses.Count == 0)
        {
            Debug.LogError($"Configure at least one {period} completion bonus.", this);
            return false;
        }

        foreach (TaskBonusOption bonus in bonuses)
        {
            if (bonus == null || !Enum.IsDefined(typeof(TaskRewardType), bonus.type) ||
                (bonus.type == TaskRewardType.Item && (bonus.item == null || string.IsNullOrWhiteSpace(bonus.item.Id))) ||
                (bonus.type != TaskRewardType.Item && bonus.amount <= 0))
            {
                Debug.LogError($"The {period} bonus list contains an invalid reward.", this);
                return false;
            }
        }

        return true;
    }

    public bool TryGetTasks(TaskTime period, out List<TaskDefinition> matchingTasks)
    {
        matchingTasks = new List<TaskDefinition>();
        if (!Enum.IsDefined(typeof(TaskTime), period))
        {
            Debug.LogError($"Invalid task period: {period}.", this);
            return false;
        }

        if (!ValidateCatalog())
            return false;

        foreach (TaskDefinition task in AllTasks())
        {
            if (task.Period == period)
                matchingTasks.Add(task);
        }

        return true;
    }

    public bool TryGetById(string id, out TaskDefinition task)
    {
        task = null;
        if (string.IsNullOrWhiteSpace(id))
        {
            Debug.LogError("Cannot look up a task without an ID.", this);
            return false;
        }

        if (!ValidateCatalog())
            return false;

        foreach (TaskDefinition candidate in AllTasks())
        {
            if (string.Equals(candidate.Id, id, StringComparison.Ordinal))
            {
                task = candidate;
                break;
            }
        }
        if (task == null)
        {
            Debug.LogError($"Task ID '{id}' is not in the catalog.", this);
            return false;
        }

        return true;
    }

    private bool ValidateCatalog()
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);
        bool valid = ValidateList(dailyTasks, TaskTime.daily, ids);
        valid = ValidateList(weeklyTasks, TaskTime.weekly, ids) && valid;
        valid = ValidateList(monthlyTasks, TaskTime.monthly, ids) && valid;
        valid = ValidateList(tasks, null, ids) && valid;
        return valid;
    }

    private bool ValidateList(List<TaskDefinition> entries, TaskTime? expectedPeriod, HashSet<string> ids)
    {
        if (entries == null)
        {
            Debug.LogError("Task catalog has a missing task list.", this);
            return false;
        }

        bool valid = true;
        foreach (TaskDefinition task in entries)
        {
            if (task == null)
            {
                Debug.LogError("Task catalog contains an empty task reference.", this);
                valid = false;
                continue;
            }

            if (!task.TryValidate(out string error))
            {
                Debug.LogError($"Task '{task.name}' is invalid: {error}", task);
                valid = false;
            }

            if (expectedPeriod.HasValue && task.Period != expectedPeriod.Value)
            {
                Debug.LogError($"Task '{task.name}' is in the {expectedPeriod} list but is configured as {task.Period}.", task);
                valid = false;
            }

            if (!string.IsNullOrWhiteSpace(task.Id) && !ids.Add(task.Id))
            {
                Debug.LogError($"Task catalog contains duplicate ID '{task.Id}'.", task);
                valid = false;
            }
        }

        return valid;
    }

    private IEnumerable<TaskDefinition> AllTasks()
    {
        foreach (TaskDefinition task in dailyTasks) yield return task;
        foreach (TaskDefinition task in weeklyTasks) yield return task;
        foreach (TaskDefinition task in monthlyTasks) yield return task;
        foreach (TaskDefinition task in tasks) yield return task;
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        if (tasks != null && tasks.Count > 0)
            Debug.LogWarning("This catalog contains legacy tasks. Use 'Move legacy tasks into period lists' from the component menu.", this);
    }

    [ContextMenu("Move legacy tasks into period lists")]
    private void MigrateLegacyTasks()
    {
        if (tasks == null || tasks.Count == 0) return;
        for (int i = tasks.Count - 1; i >= 0; i--)
        {
            TaskDefinition task = tasks[i];
            if (task == null)
            {
                Debug.LogError("Cannot migrate an empty task reference; remove it from the legacy list in Inspector Debug mode.", this);
                continue;
            }

            List<TaskDefinition> destination;
            switch (task.Period)
            {
                case TaskTime.daily: destination = dailyTasks; break;
                case TaskTime.weekly: destination = weeklyTasks; break;
                case TaskTime.monthly: destination = monthlyTasks; break;
                default:
                    Debug.LogError($"Cannot migrate task '{task.name}' with an invalid period.", task);
                    continue;
            }
            if (!destination.Contains(task)) destination.Add(task);
            tasks.RemoveAt(i);
        }
        UnityEditor.EditorUtility.SetDirty(this);
    }
#endif
}
