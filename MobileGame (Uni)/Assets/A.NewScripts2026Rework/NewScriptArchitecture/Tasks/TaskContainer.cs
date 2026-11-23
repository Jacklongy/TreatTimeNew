using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Displays the selected tasks and completion bonus for one period.</summary>
public class TaskContainer : MonoBehaviour
{
    [SerializeField] private TaskTime period;
    [SerializeField] private TaskObject taskPrefab;
    [SerializeField] private Transform taskParent;
    [SerializeField] private TMP_Text bonusText;
    [SerializeField] private Button bonusClaimButton;
    [Tooltip("Optional UI Image displaying the selected bonus.")]
    [SerializeField] private Image bonusImage;
    [SerializeField] private Sprite coinBonusSprite;
    [SerializeField] private Sprite gemBonusSprite;

    private readonly List<TaskObject> cards = new List<TaskObject>();
    private string missingIcon;

    private void OnEnable()
    {
        TaskProgressManager.Changed += Refresh;
        if (bonusClaimButton != null) bonusClaimButton.onClick.AddListener(ClaimBonus);
        Refresh();
    }

    private void OnDisable()
    {
        TaskProgressManager.Changed -= Refresh;
        if (bonusClaimButton != null) bonusClaimButton.onClick.RemoveListener(ClaimBonus);
    }

    public void Refresh()
    {
        TaskProgressManager manager = TaskProgressManager.Instance;
        if (manager == null || !manager.TryGetPeriod(period, out SavedTaskPeriod saved)) return;
        if (taskPrefab == null || taskParent == null || bonusText == null || bonusClaimButton == null)
        {
            Debug.LogError($"The {period} task section is missing UI references.", this);
            return;
        }

        while (cards.Count < saved.Tasks.Count)
            cards.Add(Instantiate(taskPrefab, taskParent));
        for (int i = 0; i < cards.Count; i++)
        {
            bool visible = i < saved.Tasks.Count;
            cards[i].gameObject.SetActive(visible);
            if (visible)
            {
                if (manager.TryGetDefinition(saved.Tasks[i].TaskId, out TaskDefinition definition))
                    cards[i].Bind(definition, saved.Tasks[i]);
                else
                    cards[i].gameObject.SetActive(false);
            }
        }

        SavedTaskBonus bonus = saved.Bonus;
        if (bonus == null)
        {
            Debug.LogError($"The saved {period} bonus is missing.", this);
            bonusText.text = "Bonus unavailable";
            bonusClaimButton.gameObject.SetActive(false);
            if (bonusImage != null)
            {
                bonusImage.sprite = null;
                bonusImage.gameObject.SetActive(false);
            }
            return;
        }
        string reward = bonus.Type == TaskRewardType.Item
            ? $"{bonus.ItemName} (if owned: {manager.DuplicateItemCoins:N0} coins)"
            : $"{bonus.Amount:N0} {bonus.Type}";
        bonusText.text = bonus.Claimed ? $"Bonus claimed: {reward}" : $"Complete all tasks for: {reward}";
        bonusClaimButton.gameObject.SetActive(manager.CanClaimBonus(saved));
        RefreshBonusImage(manager, bonus);
    }

    private void RefreshBonusImage(TaskProgressManager manager, SavedTaskBonus bonus)
    {
        if (bonusImage == null) return;

        Sprite sprite = null;
        string iconName;
        switch (bonus.Type)
        {
            case TaskRewardType.Coin:
                sprite = coinBonusSprite;
                iconName = "coin";
                break;
            case TaskRewardType.Gem:
                sprite = gemBonusSprite;
                iconName = "gem";
                break;
            case TaskRewardType.Item:
                iconName = $"item '{bonus.ItemId}'";
                if (manager.TryGetRewardItem(bonus.ItemId, out ShopItemData item))
                    sprite = item.Icon;
                break;
            default:
                iconName = $"invalid reward type '{bonus.Type}'";
                break;
        }

        bonusImage.sprite = sprite;
        bonusImage.gameObject.SetActive(sprite != null);
        if (sprite == null)
        {
            if (missingIcon != iconName)
                Debug.LogWarning($"No icon found for the {period} bonus {iconName}.", this);
            missingIcon = iconName;
        }
        else
            missingIcon = null;
    }

    public void ClaimBonus()
    {
        if (TaskProgressManager.Instance != null)
            TaskProgressManager.Instance.ClaimBonus(period);
    }
}
