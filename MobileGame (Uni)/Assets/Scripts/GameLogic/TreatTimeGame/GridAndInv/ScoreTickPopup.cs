using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Individual floating score popup.
/// - Displays "+{score}" as TextMeshPro world-space text
/// - Applies level-based styling (color, scale) before animation
/// - Triggers Animator to play animation (user controls animation curves in Unity)
/// - Does NOT handle animation logic (Animator and its exit time destroy the popup)
/// </summary>
public class ScoreTickPopup : PooledObject
{
    /// <summary>
    /// Visual style for a specific item level (color, scale, burst effect).
    /// One per level (1, 2, 3, 4) configured in Inspector on popup prefab.
    /// </summary>
    [System.Serializable]
    public class PopupLevelStyle
    {
        [Range(1, 4)] public int level = 1;              // Which level this style applies to
        public Color color = Color.white;                // Text color for this level
        [Min(0.2f)] public float scaleMultiplier = 1f;   // Popup scale multiplier (1.0 = normal, 1.5 = 50% bigger)
        public GameObject burstEffectPrefab;             // Optional particle effect to spawn with popup
    }

    [Header("References")]
    [SerializeField] private TMP_Text scoreText;        // TextMeshPro component showing "+score"
    [SerializeField] private Animator popupAnimator;    // Animator with Pop state machine
    [SerializeField] private string playTriggerName = "Play";  // Animator trigger to fire (default "Play")

    [Header("Styles")]
    [SerializeField] private List<PopupLevelStyle> levelStyles = new List<PopupLevelStyle>();  // One per level (1-4)

    [Header("Drift")]
    [SerializeField, Min(0f)] private float driftDistance = 0.6f;   // World units travelled
    [SerializeField, Range(0f, 90f)] private float driftAngleSpread = 25f; // Max degrees off straight up (lower = less random)
    [SerializeField, Min(0f)] private float driftTilt = 15f;        // Degrees rotated by the end, leaning toward the drift side
    [SerializeField, Min(0.05f)] private float driftDuration = 0.8f; // Match roughly to the pop animation length
    [SerializeField] private AnimationCurve driftCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Vector3 startScale;
    private Vector3 driftOrigin;
    private Vector3 driftOffset;
    private float driftTiltEnd;
    private float driftElapsed;
    private bool isDrifting;

    protected override void OnReturned()
    {
        isDrifting = false;
    }

    private void Awake()
    {
        if (scoreText == null)
        {
            scoreText = GetComponentInChildren<TMP_Text>();
        }

        if (popupAnimator == null)
        {
            popupAnimator = GetComponent<Animator>();
        }

        startScale = transform.localScale;
    }

    private void Update()
    {
        if (!isDrifting)
        {
            return;
        }

        driftElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(driftElapsed / driftDuration);
        float eased = driftCurve.Evaluate(t);
        transform.position = driftOrigin + driftOffset * eased;
        transform.rotation = Quaternion.Euler(0f, 0f, driftTiltEnd * eased);

        if (t >= 1f)
        {
            isDrifting = false;
        }
    }

    private void StartDrift()
    {
        driftOrigin = transform.position;
        float angle = Random.Range(-driftAngleSpread, driftAngleSpread);
        driftOffset = Quaternion.Euler(0f, 0f, angle) * Vector3.up * driftDistance;
        // Positive angle drifts left, so tilt follows the same sign.
        driftTiltEnd = driftAngleSpread > 0f ? driftTilt * (angle / driftAngleSpread) : 0f;
        transform.rotation = Quaternion.identity;
        driftElapsed = 0f;
        isDrifting = true;
    }

    public void Play(int amount, int level)
    {
        // Called by spawner: initialize popup with score amount and item level
        
        if (scoreText == null)
        {
            ReturnToPool();
            return;
        }

        // Look up level-based style (color, scale)
        PopupLevelStyle style = GetStyleForLevel(level);
        Color styleColor = style != null ? style.color : Color.white;  // Fallback to white if no style found
        float scaleMultiplier = style != null ? style.scaleMultiplier : 1f;

        // === SET TEXT ===
        scoreText.text = amount >= 0 ? $"+{amount}" : amount.ToString();  // "+5" or "-3"
        scoreText.color = styleColor;

        // === APPLY SCALE ===
        // Multiply by style multiplier (e.g., level 2 might be 1.2× bigger than level 1)
        transform.localScale = startScale * Mathf.Max(0.2f, scaleMultiplier);

        // === SPAWN BURST EFFECT (optional) ===
        if (style != null && style.burstEffectPrefab != null)
        {
            Instantiate(style.burstEffectPrefab, transform.position, Quaternion.identity);
        }

        StartDrift();

        // === TRIGGER ANIMATOR ===
        // Animator has Idle→Pop→Idle state machine
        // Transitions to Pop on "Play" trigger, runs animation, then exits and destroys popup
        if (popupAnimator != null && !string.IsNullOrEmpty(playTriggerName))
        {
            popupAnimator.SetTrigger(playTriggerName);
        }
    }

    private PopupLevelStyle GetStyleForLevel(int level)
    {
        // Search levelStyles list for matching level
        // Returns null if not found (fallback: use white color, 1.0 scale)
        int clampedLevel = Mathf.Clamp(level, 1, 4);
        for (int i = 0; i < levelStyles.Count; i++)
        {
            if (levelStyles[i] != null && levelStyles[i].level == clampedLevel)
            {
                return levelStyles[i];
            }
        }

        return null;
    }
}
