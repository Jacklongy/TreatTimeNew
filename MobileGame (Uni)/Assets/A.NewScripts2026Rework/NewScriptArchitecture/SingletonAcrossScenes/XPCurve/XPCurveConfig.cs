using UnityEngine;

/// <summary>
/// Shared XP-per-level curve. Referenced by XPManager (gameplay) and MainMenuXPDisplay (menu)
/// so both scenes calculate level thresholds identically without duplicating the curve.
/// </summary>
[CreateAssetMenu(fileName = "XPCurveConfig", menuName = "TreatTime/XP Curve Config")]
public class XPCurveConfig : ScriptableObject
{
    [Header("XP Curve")]
    [SerializeField] private AnimationCurve xpCurve;

    /// <summary>
    /// Returns the total XP required to reach the level after the given level.
    /// </summary>
    public float GetXPForNextLevel(int level)
    {
        int nextLevel = level + 1;

        if (nextLevel <= 0)
        {
            return 0f; // level 0 requires no XP
        }

        if (xpCurve == null || xpCurve.length == 0)
        {
            return float.MaxValue;
        }

        return Mathf.Max(0f, xpCurve.Evaluate(nextLevel));
    }
}
