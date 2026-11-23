using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif

public enum TaskMetric
{
    Merge,
    Feed,
    Score
}

/// <summary>Designer-authored task settings; player progress belongs in the profile.</summary>
[CreateAssetMenu(fileName = "NewTask", menuName = "Tasks/Task Definition")]
public class TaskDefinition : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique and permanent. Saved tasks refer to this ID.")]
    [SerializeField] private string id;
    [SerializeField] private string description;

    [Header("Goal")]
    [SerializeField] private TaskTime period;
    [SerializeField] private TaskMetric metric;
    [Tooltip("For merge tasks, use the Unity tag of the items being merged.")]
    [SerializeField] private string mergeTag;
    [Min(1)] [SerializeField] private int goal = 1;
    [Tooltip("The goal must be reached within one run.")]
    [SerializeField] private bool singleRun;

    [Header("Reward")]
    [SerializeField] private TaskRewardType rewardType;
    [Min(1)] [SerializeField] private int rewardAmount = 1;
    [Tooltip("The shop item awarded when the reward type is Item.")]
    [SerializeField] private ShopItemData rewardItem;

    public string Id => id;
    public string Description => description;
    public TaskTime Period => period;
    public TaskMetric Metric => metric;
    public string MergeTag => mergeTag;
    public int Goal => goal;
    public bool SingleRun => singleRun;
    public TaskRewardType RewardType => rewardType;
    public int RewardAmount => rewardAmount;
    public ShopItemData RewardItem => rewardItem;

    public bool TryValidate(out string error)
    {
        if (string.IsNullOrWhiteSpace(id))
            error = "Task ID is missing.";
        else if (string.IsNullOrWhiteSpace(description))
            error = "Task description is missing.";
        else if (!Enum.IsDefined(typeof(TaskTime), period))
            error = "Task period is invalid.";
        else if (!Enum.IsDefined(typeof(TaskMetric), metric))
            error = "Task metric is invalid.";
        else if (goal <= 0)
            error = "Task goal must be greater than zero.";
        else if (metric == TaskMetric.Merge && string.IsNullOrWhiteSpace(mergeTag))
            error = "Merge tasks need an item tag.";
        else if (!Enum.IsDefined(typeof(TaskRewardType), rewardType))
            error = "Task reward type is invalid.";
        else if (rewardType == TaskRewardType.Item && rewardItem == null)
            error = "Item rewards need a shop item.";
        else if (rewardType == TaskRewardType.Item && string.IsNullOrWhiteSpace(rewardItem.Id))
            error = "Item rewards need a shop item with an ID.";
        else if (rewardType != TaskRewardType.Item && rewardAmount <= 0)
            error = "Currency rewards must be greater than zero.";
        else
        {
#if UNITY_EDITOR
            if (metric == TaskMetric.Merge && mergeTag != "Untagged" &&
                Array.IndexOf(InternalEditorUtility.tags, mergeTag) < 0)
            {
                error = $"Merge tag '{mergeTag}' is not defined in Unity.";
                return false;
            }
#endif
            error = null;
            return true;
        }

        return false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(id))
            id = Guid.NewGuid().ToString("N");
    }
#endif
}
