using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Visual effects for the hunger bar. Put on the hunger bar object (or its parent).
/// - Punch when the dog is fed, stronger when it is part of a combo (same feel as ScoreTextCombo)
/// - Flashes the fill red when hunger is low
/// </summary>
public class HungerBarEffects : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DogFeeder dogFeeder;
    [Tooltip("Object that gets scaled by the punch. Defaults to this object.")]
    [SerializeField] private Transform punchTarget;
    [Tooltip("Defaults to the DogFeeder's HungerBar image.")]
    [SerializeField] private Image fillImage;

    [Header("Small Punch (normal feed)")]
    [SerializeField, Min(0f)] private float smallPunch = 0.12f;

    [Header("Big Punch (combo feed)")]
    [SerializeField, Min(0f)] private float bigPunch = 0.35f;
    [SerializeField, Min(0f)] private float bigPunchPerCombo = 0.05f;   // extra for each combo above x2
    [SerializeField, Min(0f)] private float maxBigPunch = 0.8f;
    [SerializeField, Min(0.1f)] private float punchRecoverSpeed = 8f;

    [Header("Low Hunger Flash")]
    [SerializeField, Range(0f, 1f)] private float lowHungerThreshold = 0.25f;
    [SerializeField, Min(0f)] private float flashSpeed = 6f;            // flashes per second
    [SerializeField] private Color flashColor = new Color32(0xED, 0x55, 0x65, 255);
    [SerializeField, Min(0f)] private float lowHungerPulseAmount = 0.06f;
    [SerializeField, Min(0f)] private float pulseFadeSpeed = 6f;

    [Header("Wrong Meal Flash")]
    [SerializeField, Min(0.05f)] private float wrongFlashSeconds = 0.6f;
    [SerializeField, Min(1)] private int wrongFlashCount = 3;
    private float wrongFlashRemaining;

    [Header("Extra Flash Target")]
    [Tooltip("Optional child object tinted with the same flash. Needs an Image, TMP text, other UI Graphic or SpriteRenderer.")]
    [SerializeField] private GameObject extraFlashTarget;

    private Graphic extraGraphic;
    private SpriteRenderer extraSprite;
    private Color extraBaseColor;
    private float pulseWeight;
    private float pulseTime;

    private Vector3 baseScale;
    private Color baseColor;
    private float punch;
    private bool flashing;

    private void Awake()
    {
        if (dogFeeder == null)
        {
            dogFeeder = FindObjectOfType<DogFeeder>();
        }

        if (fillImage == null && dogFeeder != null)
        {
            fillImage = dogFeeder.HungerBar;
        }

        if (punchTarget == null)
        {
            punchTarget = transform;
        }

        baseScale = punchTarget.localScale;
        if (fillImage != null)
        {
            baseColor = fillImage.color;
        }

        if (extraFlashTarget != null)
        {
            extraGraphic = extraFlashTarget.GetComponent<Graphic>();
            extraSprite = extraFlashTarget.GetComponent<SpriteRenderer>();
            extraBaseColor = extraGraphic != null ? extraGraphic.color
                : extraSprite != null ? extraSprite.color : Color.white;
        }
    }

    private void OnEnable()
    {
        if (dogFeeder != null)
        {
            dogFeeder.MealFed += OnMealFed;
            dogFeeder.WrongMeal += OnWrongMeal;
        }
    }

    private void OnDisable()
    {
        if (dogFeeder != null)
        {
            dogFeeder.MealFed -= OnMealFed;
            dogFeeder.WrongMeal -= OnWrongMeal;
        }

        wrongFlashRemaining = 0f;
        punchTarget.localScale = baseScale;
        punch = 0f;
        StopFlash();
    }

    private void OnMealFed(int comboAmount)
    {
        if (comboAmount >= 2)
        {
            float extra = (comboAmount - 2) * bigPunchPerCombo;
            punch = Mathf.Max(punch, Mathf.Min(bigPunch + extra, maxBigPunch));
        }
        else
        {
            punch = Mathf.Max(punch, smallPunch);
        }
    }

    private void Update()
    {
        punch *= Mathf.Exp(-punchRecoverSpeed * Time.deltaTime);

        UpdateFlash();

        float pulse = Mathf.Sin(pulseTime) * lowHungerPulseAmount * pulseWeight;
        punchTarget.localScale = baseScale * ((1f + pulse) * (1f + punch));
    }

    private void UpdateFlash()
    {
        if (fillImage == null || dogFeeder == null)
        {
            return;
        }

        float hunger = dogFeeder.hungerLevel;
        bool low = hunger > 0f && hunger <= lowHungerThreshold;

        pulseWeight = Mathf.MoveTowards(pulseWeight, low ? 1f : 0f, pulseFadeSpeed * Time.deltaTime);
        if (low)
        {
            pulseTime += flashSpeed * Mathf.PI * Time.deltaTime;
        }

        // Wrong meal: a solid red hit that fades out, flashing a few times.
        float wrong = 0f;
        if (wrongFlashRemaining > 0f)
        {
            wrongFlashRemaining = Mathf.Max(0f, wrongFlashRemaining - Time.deltaTime);
            float progress = 1f - wrongFlashRemaining / wrongFlashSeconds;
            wrong = Mathf.Abs(Mathf.Sin(progress * wrongFlashCount * Mathf.PI));
        }

        if (!low && wrongFlashRemaining <= 0f && wrong <= 0f)
        {
            StopFlash();
            return;
        }

        flashing = true;
        float t = low ? Mathf.PingPong(Time.time * flashSpeed, 1f) : 0f;
        t = Mathf.Max(t, wrong);
        fillImage.color = Color.Lerp(baseColor, flashColor, t);
        SetExtraColor(Color.Lerp(extraBaseColor, flashColor, t));
    }

    private void OnWrongMeal()
    {
        wrongFlashRemaining = wrongFlashSeconds;
    }

    private void StopFlash()
    {
        if (flashing && fillImage != null)
        {
            fillImage.color = baseColor;
            SetExtraColor(extraBaseColor);
        }

        flashing = false;
    }

    private void SetExtraColor(Color color)
    {
        if (extraGraphic != null)
        {
            extraGraphic.color = color;
        }
        else if (extraSprite != null)
        {
            extraSprite.color = color;
        }
    }
}
