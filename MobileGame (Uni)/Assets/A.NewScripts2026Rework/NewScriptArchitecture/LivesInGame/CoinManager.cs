using UnityEngine;

/// <summary>
/// Tracks coins earned during the current run.
/// The persistent profile total is updated only when the run is saved.
/// </summary>
public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance { get; private set; }

    /// <summary>Total coins earned during this run.</summary>
    public int CoinsEarnedThisRun { get; private set; }

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
    /// Adds a coin reward earned during this run.
    /// </summary>
    public void AddCoins(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        CoinsEarnedThisRun += amount;
    }
}
