using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Everything the game over screen needs to present a finished run.
/// </summary>
public struct GameOverResults
{
    public int FinalScore;
    public bool IsNewHighScore;
    public int CoinsEarned;
    public int StartingLevel;
    public int FinalLevel;
    public float StartingXP;
    public float FinalXP;
    public XPManager XPSource;
}

/// <summary>
/// Plays the game over results as a sequence: score, then coins counting up, then the XP bar filling.
/// Runs on unscaled time because the game is paused (timeScale 0) while this screen is open.
/// A tap anywhere skips straight to the final state.
/// </summary>
public class GameOverScreenUiController : MonoBehaviour
{
    #region Inspector

    [Header("Score")]
    [SerializeField] private GameObject scoreRoot;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private Animator scoreAnimator;
    [SerializeField] private string scorePopTrigger = "Pop";
    [SerializeField] private string highScoreTrigger = "HighScore";
    [SerializeField] private float scoreAnimationSeconds = 0.15f;

    [Header("Coins")]
    [SerializeField] private GameObject coinsRoot;
    [SerializeField] private TMP_Text coinsEarnedText;
    [SerializeField] private Animator coinsAnimator;
    [SerializeField] private string coinsCompleteTrigger = "Complete";
    [SerializeField] private float coinCountSeconds = 1.25f;
    [SerializeField] private float coinCompleteAnimationSeconds = 0.5f;
    [SerializeField] private float coinTickScale = 1.15f;
    [SerializeField] private float coinTickSeconds = 0.08f;
    [SerializeField] private float coinTickMinInterval = 0.05f;

    [Header("XP Bar")]
    [SerializeField] private GameObject xpRoot;
    [SerializeField] private Image xpFillImage;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text xpProgressText;
    [SerializeField] private Animator xpAnimator;
    [SerializeField] private string levelUpTrigger = "LevelUp";
    [SerializeField] private float xpFullBarSeconds = 1.2f;
    [SerializeField] private float levelUpAnimationSeconds = 0.8f;
    [SerializeField] private int maxAnimatedLevelUps = 5;

    [Header("Buttons")]
    [SerializeField] private CanvasGroup buttonsGroup;


    [Header("Other UI Elements")]
    public ParticleSystem HighScoreParticles;


    #endregion

    #region State

    private GameOverResults results;
    private Coroutine sequenceRoutine;
    private bool sequenceRunning;
    private bool scoreShown;
    private float lastTickTime;

    #endregion

    #region Unity Callbacks

    private void Awake()
    {
        SetAnimatorToUnscaledTime(scoreAnimator);
        SetAnimatorToUnscaledTime(coinsAnimator);
        SetAnimatorToUnscaledTime(xpAnimator);
        HideAll();
    }

    private void Update()
    {
        if (sequenceRunning && Input.GetMouseButtonDown(0))
        {
            SkipToFinalState();
        }
    }

    #endregion

    #region Public API

    /// <summary>
    /// Hides everything and starts the results sequence for the completed run.
    /// </summary>
    public void ShowResults(GameOverResults runResults)
    {
        results = runResults;

        if (sequenceRoutine != null)
        {
            StopCoroutine(sequenceRoutine);
        }

        HideAll();
        sequenceRunning = true;
        sequenceRoutine = StartCoroutine(PlaySequence());
    }

    #endregion

    #region Sequence

    private IEnumerator PlaySequence()
    {
        yield return PlayScoreReveal();
        yield return CountUpCoins();
        yield return FillXPBar();

        FinishSequence();
    }

    private IEnumerator PlayScoreReveal()
    {
        ShowScore();
        yield return WaitUnscaled(scoreAnimationSeconds);
    }

    private IEnumerator CountUpCoins()
    {
        SetActive(coinsRoot, true);
        SetText(coinsEarnedText, FormatCoins(0));

        if (results.CoinsEarned <= 0)
        {
            yield break;
        }

        int shownValue = 0;
        float elapsed = 0f;

        while (elapsed < coinCountSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / coinCountSeconds);
            int value = Mathf.RoundToInt(Mathf.Lerp(0, results.CoinsEarned, progress));

            if (value != shownValue)
            {
                shownValue = value;
                SetText(coinsEarnedText, FormatCoins(value));
                PlayCoinTick();
            }

            yield return null;
        }

        SetText(coinsEarnedText, FormatCoins(results.CoinsEarned));
        Trigger(coinsAnimator, coinsCompleteTrigger);
        yield return WaitUnscaled(coinCompleteAnimationSeconds);
    }

    private IEnumerator FillXPBar()
    {
        SetActive(xpRoot, true);

        int level = results.StartingLevel;
        float fill = GetFillForXP(level, results.StartingXP);
        SetXPDisplay(level, fill);

        int levelUpsAnimated = 0;

        while (level < results.FinalLevel && levelUpsAnimated < maxAnimatedLevelUps)
        {
            yield return AnimateFill(level, fill, 1f);

            level++;
            levelUpsAnimated++;
            fill = 0f;
            Trigger(xpAnimator, levelUpTrigger);
            SetXPDisplay(level, fill);
            yield return WaitUnscaled(levelUpAnimationSeconds);
        }

        // Too many levels gained to animate each one: jump to the final level.
        if (level < results.FinalLevel)
        {
            level = results.FinalLevel;
            fill = 0f;
        }

        yield return AnimateFill(level, fill, GetFillForXP(level, results.FinalXP));
    }

    private IEnumerator AnimateFill(int level, float from, float to)
    {
        float duration = Mathf.Max(0.05f, Mathf.Abs(to - from) * xpFullBarSeconds);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetXPDisplay(level, Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
            yield return null;
        }

        SetXPDisplay(level, to);
    }

    private void FinishSequence()
    {
        sequenceRunning = false;
        sequenceRoutine = null;
        SetButtonsInteractable(true);
    }

    private void SkipToFinalState()
    {
        if (sequenceRoutine != null)
        {
            StopCoroutine(sequenceRoutine);
        }

        if (!scoreShown)
        {
            ShowScore();
        }

        SetActive(coinsRoot, true);
        SetText(coinsEarnedText, FormatCoins(results.CoinsEarned));

        if (coinsEarnedText != null)
        {
            LeanTween.cancel(coinsEarnedText.gameObject);
            coinsEarnedText.transform.localScale = Vector3.one;
        }

        SetActive(xpRoot, true);
        SetXPDisplay(results.FinalLevel, GetFillForXP(results.FinalLevel, results.FinalXP));

        FinishSequence();
    }

    #endregion

    #region Score

    private void ShowScore()
    {
        scoreShown = true;
        SetActive(scoreRoot, true);
        SetText(scoreText, results.FinalScore.ToString());

        // A new high score replaces the standard pop rather than playing alongside it.
        if (results.IsNewHighScore)
        {
            Trigger(scoreAnimator, highScoreTrigger);
            // TODO: play HighScoreParticles (confetti) here once the effect is made.
        }
        else
        {
            Trigger(scoreAnimator, scorePopTrigger);
        }
    }

    #endregion

    #region Coins

    private void PlayCoinTick()
    {
        if (coinsEarnedText == null || Time.unscaledTime - lastTickTime < coinTickMinInterval)
        {
            return;
        }

        lastTickTime = Time.unscaledTime;

        GameObject target = coinsEarnedText.gameObject;
        LeanTween.cancel(target);
        target.transform.localScale = Vector3.one * coinTickScale;
        LeanTween.scale(target, Vector3.one, coinTickSeconds).setIgnoreTimeScale(true);
    }

    private string FormatCoins(int value)
    {
        return $"+{value}";
    }

    #endregion

    #region XP Bar

    /// <summary>
    /// Returns how far through the given level the XP value is (0-1), using the same math as the main menu bar.
    /// </summary>
    private float GetFillForXP(int level, float xp)
    {
        GetLevelXPRange(level, out float levelStartXP, out float xpNeeded);
        return Mathf.Clamp01((xp - levelStartXP) / xpNeeded);
    }

    private void GetLevelXPRange(int level, out float levelStartXP, out float xpNeeded)
    {
        levelStartXP = results.XPSource.GetXPForNextLevel(level - 1);
        float levelEndXP = results.XPSource.GetXPForNextLevel(level);
        xpNeeded = Mathf.Max(1f, levelEndXP - levelStartXP);
    }

    private void SetXPDisplay(int level, float fill)
    {
        if (xpFillImage != null)
        {
            xpFillImage.fillAmount = fill;
        }

        GetLevelXPRange(level, out _, out float xpNeeded);

        SetText(levelText, level.ToString());
        SetText(xpProgressText, $"{fill * xpNeeded:N0}/{xpNeeded:N0}XP");
    }

    #endregion

    #region Helpers

    private void HideAll()
    {
        scoreShown = false;
        lastTickTime = 0f;

        SetActive(scoreRoot, false);
        SetActive(coinsRoot, false);
        SetActive(xpRoot, false);
        SetButtonsInteractable(false);
    }

    private void SetAnimatorToUnscaledTime(Animator animator)
    {
        if (animator != null)
        {
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (buttonsGroup != null)
        {
            buttonsGroup.interactable = interactable;
        }
    }

    private IEnumerator WaitUnscaled(float seconds)
    {
        float elapsed = 0f;

        while (elapsed < seconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private void Trigger(Animator animator, string triggerName)
    {
        if (animator != null && !string.IsNullOrEmpty(triggerName))
        {
            animator.SetTrigger(triggerName);
        }
    }

    private void SetActive(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }

    private void SetText(TMP_Text targetText, string value)
    {
        if (targetText == null)
        {
            return;
        }

        targetText.text = value;
    }

    #endregion
}