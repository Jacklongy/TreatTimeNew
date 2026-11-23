using System;
using UnityEngine;

public class CloudScroller : MonoBehaviour
{
    [SerializeField, Tooltip("World units per second. Positive moves left; negative moves right.")]
    private float speed = 0.1f;

    [SerializeField, Tooltip("Orthographic camera viewing the clouds. Uses Main Camera if left empty.")]
    private Camera viewCamera;

    private CloudGroup[] groups;
    private float spacing;
    private bool warnedAboutCoverage;

    private sealed class CloudGroup
    {
        public Transform transform;
        public SpriteRenderer[] sprites;
    }

    private void Start()
    {
        if (viewCamera == null)
            viewCamera = Camera.main;

        if (viewCamera == null || !viewCamera.orthographic ||
            Vector3.Dot(viewCamera.transform.right, Vector3.right) < 0.999f)
        {
            Debug.LogError("[CloudScroller] Assign an orthographic camera whose right direction is world right.", this);
            enabled = false;
            return;
        }

        if (transform.childCount < 2)
        {
            Debug.LogError("[CloudScroller] Add at least two evenly spaced cloud groups as children.", this);
            enabled = false;
            return;
        }

        groups = new CloudGroup[transform.childCount];
        for (int i = 0; i < groups.Length; i++)
        {
            Transform group = transform.GetChild(i);
            groups[i] = new CloudGroup
            {
                transform = group,
                sprites = group.GetComponentsInChildren<SpriteRenderer>(true)
            };

            if (groups[i].sprites.Length == 0)
            {
                Debug.LogError($"[CloudScroller] Cloud group '{group.name}' has no SpriteRenderers.", group);
                enabled = false;
                return;
            }
        }

        Array.Sort(groups, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
        spacing = groups[1].transform.position.x - groups[0].transform.position.x;
        if (spacing <= 0.01f)
        {
            Debug.LogError("[CloudScroller] Cloud groups must have distinct, evenly spaced X positions.", this);
            enabled = false;
            return;
        }

        for (int i = 2; i < groups.Length; i++)
        {
            float gap = groups[i].transform.position.x - groups[i - 1].transform.position.x;
            if (Mathf.Abs(gap - spacing) > 0.01f)
            {
                Debug.LogError("[CloudScroller] Cloud groups must be evenly spaced along world X.", this);
                enabled = false;
                return;
            }
        }
    }

    private void LateUpdate()
    {
        float left = viewCamera.ViewportToWorldPoint(new Vector3(0f, 0f, 0f)).x;
        float right = viewCamera.ViewportToWorldPoint(new Vector3(1f, 0f, 0f)).x;
        float loopWidth = spacing * groups.Length;
        float maxStep = float.PositiveInfinity;

        foreach (CloudGroup group in groups)
        {
            if (!TryGetBounds(group, out Bounds bounds))
            {
                Debug.LogError($"[CloudScroller] Cloud group '{group.transform.name}' has no active SpriteRenderers.", group.transform);
                enabled = false;
                return;
            }

            if (loopWidth <= right - left + bounds.size.x)
            {
                if (!warnedAboutCoverage)
                    Debug.LogWarning("[CloudScroller] Not enough cloud groups to loop off-screen. Add more evenly spaced copies.", this);
                warnedAboutCoverage = true;
                return;
            }

            maxStep = Mathf.Min(maxStep, (loopWidth - (right - left) - bounds.size.x) * 0.5f);
        }
        warnedAboutCoverage = false;

        // Avoid skipping the off-screen wrap window after a long frame or app resume.
        float movement = Mathf.Clamp(-speed * Time.unscaledDeltaTime, -maxStep, maxStep);
        foreach (CloudGroup group in groups)
            group.transform.position += Vector3.right * movement;

        foreach (CloudGroup group in groups)
        {
            if (!TryGetBounds(group, out Bounds bounds))
                continue;

            float shift = 0f;
            if (speed > 0f && bounds.max.x < left && bounds.min.x + loopWidth > right)
                shift = loopWidth;
            else if (speed < 0f && bounds.min.x > right && bounds.max.x - loopWidth < left)
                shift = -loopWidth;

            if (shift != 0f)
                group.transform.position += Vector3.right * shift;
        }
    }

    private static bool TryGetBounds(CloudGroup group, out Bounds bounds)
    {
        bounds = default;
        bool found = false;
        foreach (SpriteRenderer sprite in group.sprites)
        {
            if (!sprite.enabled || !sprite.gameObject.activeInHierarchy)
                continue;

            if (!found)
            {
                bounds = sprite.bounds;
                found = true;
            }
            else
            {
                bounds.Encapsulate(sprite.bounds);
            }
        }
        return found;
    }
}
