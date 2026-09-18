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
public class ScoreTickPopup : MonoBehaviour, IPoolableEffect
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

    private Vector3 startScale;
    private ScoreTickPopupSpawner owningSpawner;

    /// <summary>
    /// Assigns the spawner that owns this popup's pool. Call once, right after instantiation.
    /// </summary>
    public void SetOwner(ScoreTickPopupSpawner spawner)
    {
        owningSpawner = spawner;
    }

    /// <summary>
    /// Called by DestroyOnExit when the pop animation finishes, instead of destroying this object.
    /// </summary>
    public void ReturnToPool()
    {
        if (owningSpawner != null)
        {
            owningSpawner.ReleasePopup(this);
        }
        else
        {
            Destroy(gameObject);
        }
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

    public void Play(int amount, int level)
    {
        // Called by spawner: initialize popup with score amount and item level
        
        if (scoreText == null)
        {
            Destroy(gameObject);
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
