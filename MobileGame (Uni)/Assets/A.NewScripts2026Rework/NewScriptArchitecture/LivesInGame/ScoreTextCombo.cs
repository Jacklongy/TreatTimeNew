using TMPro;
using UnityEngine;

/// <summary>
/// Visual effects for the top score text. Put on the same GameObject as the score TextMeshProUGUI.
/// - Small punch on any score add (grid ticks, feeding without a combo)
/// - Big punch when feeding the dog during a combo
/// - Slow pulse and combo-coloured text while a combo is active
/// </summary>
public class ScoreTextCombo : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DogFeeder dogFeeder;
    [SerializeField] private TMP_Text scoreText;

    [Header("Small Punch (any score add)")]
    [SerializeField, Min(0f)] private float smallPunch = 0.12f;

    [Header("Big Punch (combo feed)")]
    [SerializeField, Min(0f)] private float bigPunch = 0.35f;
    [SerializeField, Min(0f)] private float bigPunchPerCombo = 0.05f;   // extra for each combo above x2
    [SerializeField, Min(0f)] private float maxBigPunch = 0.8f;

    [Header("Combo Pulse")]
    [SerializeField, Min(0f)] private float pulseAmount = 0.05f;
    [SerializeField, Min(0f)] private float pulseSpeed = 4f;            // radians per second
    [SerializeField, Min(0f)] private float pulseSpeedPerCombo = 0.5f;  // extra for each combo above x2

    [Header("Settle")]
    [SerializeField, Min(0.1f)] private float punchRecoverSpeed = 8f;
    [SerializeField, Min(0.1f)] private float pulseFadeSpeed = 4f;

    private Vector3 baseScale;
    private Color baseColor;
    private float punch;
    private float pulseWeight;
    private float pulseTime;
    private bool wasComboActive;
    private bool atRest = true;

    private void Awake()
    {
        if (scoreText == null)
        {
            scoreText = GetComponent<TMP_Text>();
        }

        if (dogFeeder == null)
        {
            dogFeeder = FindObjectOfType<DogFeeder>();
        }

        baseScale = transform.localScale;
        baseColor = scoreText.color;
    }

    // Start rather than OnEnable so NewScoreManager.Instance already exists.
    private void Start()
    {
        if (NewScoreManager.Instance != null)
        {
            NewScoreManager.Instance.ScoreAdded += OnScoreAdded;
        }
    }

    private void OnDestroy()
    {
        if (NewScoreManager.Instance != null)
        {
            NewScoreManager.Instance.ScoreAdded -= OnScoreAdded;
        }
    }

    private void OnScoreAdded(int amount, bool comboFeed)
    {
        if (comboFeed && dogFeeder != null)
        {
            float extra = Mathf.Max(0, dogFeeder.ComboAmount - 2) * bigPunchPerCombo;
            punch = Mathf.Max(punch, Mathf.Min(bigPunch + extra, maxBigPunch));
        }
        else
        {
            punch = Mathf.Max(punch, smallPunch);
        }

        atRest = false;
    }

    private void Update()
    {
        bool comboActive = dogFeeder != null && dogFeeder.ComboActive;

        if (!comboActive && !wasComboActive && pulseWeight <= 0f && punch < 0.001f)
        {
            if (!atRest)
            {
                transform.localScale = baseScale;
                atRest = true;
            }

            return;
        }

        atRest = false;

        // Colour follows the combo (including the rainbow), then snaps back when it ends.
        if (comboActive)
        {
            scoreText.color = dogFeeder.GetCurrentComboColor();
        }
        else if (wasComboActive)
        {
            scoreText.color = baseColor;
        }

        wasComboActive = comboActive;

        pulseWeight = Mathf.MoveTowards(pulseWeight, comboActive ? 1f : 0f, pulseFadeSpeed * Time.deltaTime);

        float extraCombo = comboActive ? Mathf.Max(0, dogFeeder.ComboAmount - 2) * pulseSpeedPerCombo : 0f;
        pulseTime += (pulseSpeed + extraCombo) * Time.deltaTime;
        float pulse = Mathf.Sin(pulseTime) * pulseAmount * pulseWeight;

        punch *= Mathf.Exp(-punchRecoverSpeed * Time.deltaTime);

        transform.localScale = baseScale * ((1f + pulse) * (1f + punch));
    }
}
