using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum TaskState
{
    Inactive,
    Active,
    Completed,
    Claimed
}

public enum TaskTime
{
   daily,
   weekly,
   monthly
}

public enum TaskType
{
    Merge,
    Feed,
    SingleRun,
    Score,
}

public enum TaskRewardType
{
    Coin,
    Gem,
    Item
}

public class TaskObject : MonoBehaviour
{
    public TextMeshProUGUI TaskGoalText;
    public TextMeshProUGUI TaskRewardText;
    public TextMeshProUGUI TaskProgressText;

    public TaskTime CurrentTaskTime;
    public TaskRewardType CurrentTaskRewardType;

    public float CurrentTaskGoal;
    public float CurrentTaskProgress;

// image fill for task progress
    public Image TaskProgressSlider;

// starts inactive by default, claim state is hidden until task is completed
    public GameObject ClaimState;
    public TaskState CurrentState { get; private set; } = TaskState.Inactive;

    [SerializeField] private Button claimButton;
    private TaskDefinition definition;

    private void Awake()
    {
        if (claimButton != null)
            claimButton.onClick.AddListener(Claim);
    }

    private void OnDestroy()
    {
        if (claimButton != null)
            claimButton.onClick.RemoveListener(Claim);
    }

    public void Bind(TaskDefinition task, SavedTaskProgress progress)
    {
        definition = task;
        CurrentTaskTime = task.Period;
        CurrentTaskRewardType = task.RewardType;
        CurrentTaskGoal = task.Goal;
        CurrentTaskProgress = progress.Progress;
        CurrentState = progress.Claimed ? TaskState.Claimed :
            progress.Progress >= task.Goal ? TaskState.Completed : TaskState.Active;

        if (TaskGoalText != null) TaskGoalText.text = task.Description;
        if (TaskProgressText != null) TaskProgressText.text = $"{progress.Progress:N0}/{task.Goal:N0}";
        if (TaskProgressSlider != null) TaskProgressSlider.fillAmount = Mathf.Clamp01((float)progress.Progress / task.Goal);
        if (TaskRewardText != null)
        {
            if (task.RewardType == TaskRewardType.Item)
            {
                string fallback = TaskProgressManager.Instance != null
                    ? $" (if owned: {TaskProgressManager.Instance.DuplicateItemCoins:N0} coins)" : "";
                TaskRewardText.text = task.RewardItem.DisplayName + fallback;
            }
            else
                TaskRewardText.text = $"{task.RewardAmount:N0} {task.RewardType}";
        }
        if (ClaimState != null) ClaimState.SetActive(CurrentState == TaskState.Completed);
        if (claimButton != null) claimButton.interactable = CurrentState == TaskState.Completed;
    }

    public void Claim()
    {
        if (definition != null && TaskProgressManager.Instance != null)
            TaskProgressManager.Instance.ClaimTask(definition.Period, definition.Id);
    }
}
