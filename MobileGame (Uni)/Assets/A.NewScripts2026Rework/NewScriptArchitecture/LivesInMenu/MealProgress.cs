using System;
using UnityEngine;

/// <summary>Persistent, sequential ten-minute meal cooldowns.</summary>
public static class MealProgress
{
    public const int MaxBones = 3;
    public const long SecondsPerBone = 600;

    private static bool trackedRun;

    public static bool Refresh(PlayerProfileData data, long now)
    {
        if (data.FilledMealBones < 0 || data.FilledMealBones > MaxBones ||
            (data.FilledMealBones > 0 && data.NextMealBoneEmptiesAt <= 0))
        {
            throw new InvalidOperationException("Invalid saved meal progress.");
        }

        bool changed = false;
        while (data.FilledMealBones > 0 && now >= data.NextMealBoneEmptiesAt)
        {
            data.FilledMealBones--;
            data.NextMealBoneEmptiesAt = data.FilledMealBones > 0
                ? data.NextMealBoneEmptiesAt + SecondsPerBone
                : 0;
            changed = true;
        }
        return changed;
    }

    public static bool TryBeginRun(PlayerProfileData data, long now)
    {
        Refresh(data, now);
        if (data.FilledMealBones >= MaxBones) return false;
        trackedRun = true;
        return true;
    }

    public static bool CompleteRun(PlayerProfileData data, long now)
    {
        if (!trackedRun) return false;
        trackedRun = false;
        Refresh(data, now);
        if (data.FilledMealBones >= MaxBones)
        {
            Debug.LogError("Cannot add a meal bone: all bones are already full.");
            return false;
        }

        if (data.FilledMealBones == 0) data.NextMealBoneEmptiesAt = now + SecondsPerBone;
        data.FilledMealBones++;
        return true;
    }

    public static void ClearBones(PlayerProfileData data)
    {
        data.FilledMealBones = 0;
        data.NextMealBoneEmptiesAt = 0;
    }

    public static long UtcNowSeconds => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}
