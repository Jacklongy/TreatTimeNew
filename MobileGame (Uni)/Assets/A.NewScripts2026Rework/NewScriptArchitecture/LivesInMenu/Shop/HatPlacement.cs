using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>Optional local placement overrides for each target that can wear this hat.</summary>
public class HatPlacement : MonoBehaviour
{
    [Serializable]
    private class TargetPlacement
    {
        [Tooltip("Must match the target's HatSlot ID, e.g. dog or bigPig.")]
        public string slotId;
        public Vector3 localPosition;
        public Vector3 localEulerAngles;
        public Vector3 localScale = Vector3.one;
    }

    [SerializeField] private List<TargetPlacement> targetPlacements = new List<TargetPlacement>();

    public void Apply(string slotId)
    {
        if (string.IsNullOrEmpty(slotId)) return;

        foreach (TargetPlacement placement in targetPlacements)
        {
            if (placement == null || placement.slotId != slotId) continue;

            transform.localPosition = placement.localPosition;
            // Rotation and scale are applied on top of the prefab's own values so a prefab scaled to suit a tiny anchor isn't reset to 1.
            transform.localRotation *= Quaternion.Euler(placement.localEulerAngles);
            Vector3 scale = placement.localScale;
            // New inspector entries can serialize scale as 0, which would collapse the hat.
            if (scale.x == 0f) scale.x = 1f;
            if (scale.y == 0f) scale.y = 1f;
            if (scale.z == 0f) scale.z = 1f;
            transform.localScale = Vector3.Scale(transform.localScale, scale);
            return;
        }
    }
}
