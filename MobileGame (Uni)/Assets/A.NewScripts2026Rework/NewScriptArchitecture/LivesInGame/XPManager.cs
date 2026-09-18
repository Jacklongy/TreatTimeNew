using UnityEngine;

/// <summary>
/// Calculates XP earned during the current run and applies level-up progression.
/// Holds run-start and run-end snapshots so a separate game over screen script
/// can display "before vs after" XP/level feedback without redoing any math.
/// </summary>
public class XPManager : MonoBehaviour
{
    #region Current State

    //Profile Progress After This Run
    public int ProfileLevelAfterRun { get; private set; }
    public float ProfileXPAfterRun { get; private set; }

    [Header("XP Curve")]
    [SerializeField] private XPCurveConfig xpCurveConfig;

    #endregion

    #region Run Snapshot (for Game Over feedback)

    /// <summary>Level the player had when this run started.</summary>
    public int LevelAtRunStart { get; private set; }

    /// <summary>XP the player had when this run started.</summary>
    public float XPAtRunStart { get; private set; }

    /// <summary>Total XP earned during this run so far.</summary>
    public float XPGainedThisRun { get; private set; }

    /// <summary>How many levels were gained during this run.</summary>
    public int LevelsGainedThisRun => ProfileLevelAfterRun - LevelAtRunStart;

    /// <summary>True if at least one level up happened this run.</summary>
    public bool DidLevelUpThisRun => LevelsGainedThisRun > 0;

    #endregion

    #region Setup

    /// <summary>
    /// Pulls the starting XP/level from the player's profile and records the run snapshot.
    /// Call this once, right after NewRunManager has loaded the profile into the run.
    /// </summary>
    public void InitializeFromProfile(int startingLevel, float startingXP)
    {
        ProfileLevelAfterRun = startingLevel;
        ProfileXPAfterRun = startingXP;

        LevelAtRunStart = startingLevel;
        XPAtRunStart = startingXP;
        XPGainedThisRun = 0f;
    }

    #endregion

    #region XP Calculation

    /// <summary>
    /// Converts a run score into XP and applies it. Call once at the end of a run.
    /// </summary>
    public void TakeScore(float scoreTaken)
    {
        if (scoreTaken <= 0f)
        {
            return;
        }

        AddXP(scoreTaken * 0.1f);
    }

    /// <summary>
    /// Adds XP to the current run and rolls over any level ups.
    /// </summary>
    public void AddXP(float xpGained)
    {
        if (xpGained <= 0f)
        {
            return;
        }

        ProfileXPAfterRun += xpGained;
        XPGainedThisRun += xpGained;

        // Safety cap: prevents a hard freeze if the XP curve stops increasing at higher levels.
        const int maxLevelUpsPerCall = 1000;
        int safetyCounter = 0;

        while (ProfileXPAfterRun >= GetXPForNextLevel(ProfileLevelAfterRun))
        {
            ProfileLevelAfterRun++;
            safetyCounter++;

            if (safetyCounter >= maxLevelUpsPerCall)
            {
                Debug.LogError("XPManager: stopped after " + maxLevelUpsPerCall + " level-ups in one call. " +
                    "XPCurveConfig's curve likely stops increasing at higher levels (check its last keyframes).");
                break;
            }
        }
    }

    /// <summary>
    /// Returns the XP threshold required to reach the level after the given level.
    /// </summary>
    public float GetXPForNextLevel(int level)
    {
        return xpCurveConfig != null ? xpCurveConfig.GetXPForNextLevel(level) : float.MaxValue;
    }

    #endregion
}
