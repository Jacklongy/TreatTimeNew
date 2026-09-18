using UnityEngine;

/// <summary>
/// Owns ad-reward cooldown/state. Called by GameOverManager and rewarded-ad buttons.
/// </summary>
public class AdRewardManager : MonoBehaviour
{
    public static AdRewardManager Instance { get; private set; }

    [SerializeField, Min(0)] private int adCooldown;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    /// <summary>
    /// Grants the reward for a completed rewarded-ad view.
    /// </summary>
    public void GrantReward()
    {
        // TODO: hook up the actual reward (bonus coins, revive, etc).
    }
}
