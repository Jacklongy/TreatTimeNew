using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
//using CandyCoded.HapticFeedback;

[System.Serializable] 
public class DogFeeder : MonoBehaviour
{
    [Header("Chosen Food and Bowl.")]
    public string[] Foods;
    public string[] bowls;
    public string ChosenFood;
    public string ChosenBowl;
    private string nextFood;
    private string nextBowl;

    [Header("Displays")]
    public GameObject display;
    public TextMeshProUGUI MealCountDisplay;
    public Image HungerBar; // Drag your hunger bar Image here

    [Header("Menus")]
    public GameObject Quit;
    public GameObject Win;

// this still needs implimenting.
    [Header("Pop Ups")]
    public GameObject WrongFoodPopUp;
   
    [Header("Combo Settings")]
    public GameObject ComboPopUp;
    public TextMeshProUGUI ComboDisplay;
    public Animator ComboAnimator;  

    [Header("Combo Look (per combo above x2)")]
    public float speedPerCombo = 0.25f;                  // animation speed added each combo
    public float maxSpeed = 3f;
    public float sizePerCombo = 0.1f;                    // scale added each combo
    public float maxSize = 1.6f;
    public Color[] comboColors = { Color.white, Color.yellow, new Color(1f, 0.6f, 0f), Color.red }; // element 0 = x2, 1 = x3 ... last one is reused above
    public int rainbowFromCombo = 8;                     // combo at which the text switches to rainbow
    public float rainbowSpeed = 2f;                      // rainbow cycles per second
    public Color[] rainbowColors =                       // palette the rainbow blends through, loops back to the first
    {
        new Color32(0xED, 0x55, 0x65, 255),  // grapefruit
        new Color32(0xFC, 0x6E, 0x51, 255),  // bittersweet
        new Color32(0xFF, 0xCE, 0x54, 255),  // sunflower
        new Color32(0xA0, 0xD4, 0x68, 255),  // grass
        new Color32(0x48, 0xCF, 0xAD, 255),  // mint
        new Color32(0x4F, 0xC1, 0xE9, 255),  // aqua
        new Color32(0x5D, 0x9C, 0xEC, 255),  // blue jeans
        new Color32(0xAC, 0x92, 0xEC, 255),  // lavender
        new Color32(0xEC, 0x87, 0xC0, 255),  // pink rose
    };
    public bool debugForceRainbow = false;               // debug: rainbow on for any combo of x2 or more

    [Header("Combo Timer Bar")]
    public GameObject comboBarRoot;                      // parent of the bar, shown only while a combo is active
    public Image comboBarFill;                           // Image type = Filled
    public Animator comboBarAnimator;                    // optional, sped up in the warning phase
    public float comboWindow = 10f;                      // must match the value comboTimer is reset to
    public float warningTime = 3f;                       // seconds left when the bar starts flashing
    public Color warningColor = new Color32(0xED, 0x55, 0x65, 255); // grapefruit, same as the rainbow's red
    public float warningFlashSpeed = 6f;                 // flashes per second
    public float warningAnimSpeed = 2.5f;                // bar animator speed in the warning phase

    private Vector3 comboStartScale;

    // i also need to adjust how these look.
    [Header("Particles")]
    public GameObject Particles;
    public GameObject ParticlePoint;

    [Header("Animations")]
    public Animator Eating;

    [Header("Timers")]
    bool timerPause;
    public float hungerLevel = 0.5f; // Starts at 50% hunger
    public float hungerDecayRate = 0.25f; // How fast hunger depletes per second
    [SerializeField, Min(0f)] private float hungerDecayIncreasePerMilestone = 0.01f;
    [SerializeField, Min(1)] private int milestonesPerDecayIncrease = 2;
    [Tooltip("Flat hunger added on top of the normal gain for every meal in a combo from the 2nd onwards.")]
    [SerializeField, Range(0f, 1f)] private float comboHungerBonus = 0.25f;
    private int milestonesReached;
    public float hungerGainPerMeal = 0.50f; // How much hunger increases per correct meal
    public float comboTimer;
    public float comboGraceTime = 0.3f; // extra time after the window ends, so late physics events still count
    private float startingHungerDecayRate;
    private bool subscribedToMilestones;
    [Header("Feeding Score")]
    [SerializeField, Min(1)] private int feedingBaseAward = 100;
    [SerializeField, Min(0)] private int coinsPerMeal = 5;

    // Combo Multiplier
    int comboAmount;

    bool ComboStart = false;

    [Header("References")]
    public FoodCombo foodComboRef;
    public SoundManagerScript sound;

    [Header("Bools")]
    public bool countDownSound;

    /// <summary>
    /// When game starts we want to find important objects e.g the overseer, set all data to the current data. 
    /// and reset any variables from last run. 
    /// </summary>
    void Start()
    {
        hungerDecayRate = startingHungerDecayRate;
        milestonesReached = 0;
        SubscribeToMilestones();

        comboTimer = comboWindow;
        comboAmount = 0;
        comboStartScale = ComboPopUp.transform.localScale;

        // randomy pick a gameObject/ item and a bowl color. 
        // go to food wanted script and display the sprites that match with the current combo. 
        ChosenFood = Foods[Random.Range(0, Foods.Length)];
        ChosenBowl = bowls[Random.Range(0, bowls.Length)];

        display.GetComponent<FoodCombo>().DisplayWantedFood(ChosenFood, ChosenBowl);

        RollNextCombo();
    }

    private void Awake()
    {
        startingHungerDecayRate = hungerDecayRate;
    }

    private void OnEnable()
    {
        SubscribeToMilestones();
    }

    private void OnDisable()
    {
        if (subscribedToMilestones && NewScoreManager.Instance != null)
        {
            NewScoreManager.Instance.MilestoneReached -= HandleMilestoneReached;
        }

        subscribedToMilestones = false;
    }

    private void SubscribeToMilestones()
    {
        if (!subscribedToMilestones && NewScoreManager.Instance != null)
        {
            NewScoreManager.Instance.MilestoneReached += HandleMilestoneReached;
            subscribedToMilestones = true;
        }
    }

    private void HandleMilestoneReached(int milestoneScore)
    {
        milestonesReached++;
        if (milestonesReached % milestonesPerDecayIncrease == 0)
        {
            hungerDecayRate += hungerDecayIncreasePerMilestone;
        }
    }

    /// <summary>
    /// Picks the combo after the current one and shows it in the preview.
    /// </summary>
    private void RollNextCombo()
    {
        nextFood = Foods[Random.Range(0, Foods.Length)];
        nextBowl = bowls[Random.Range(0, bowls.Length)];

        foodComboRef.DisplayNextFood(nextFood, nextBowl);
    }

    /// <summary>
    /// Update the hunger bar UI to reflect current hunger level
    /// </summary>
    private void UpdateHungerBar()
    {
        if (HungerBar != null)
        {
            HungerBar.fillAmount = hungerLevel;
        }
    }

    /// <summary>
    ///  display a new food the dog wants when the player succesfully makes a meal. 
    /// </summary>
    void DisplayNewFood()
    {
        ChosenFood = nextFood;
        ChosenBowl = nextBowl;

        display.GetComponent<FoodCombo>().DisplayWantedFood(ChosenFood, ChosenBowl);

        RollNextCombo();
    }

    void Update()
    {
        // Update hunger bar UI every frame
        UpdateHungerBar();

        // Manages the combo logic. 
        if (ComboStart == true)
        {
            comboTimer -= Time.deltaTime;

            if (comboTimer < comboWindow)
            {
                if (comboAmount >= 2)
                {
                    // Display pop up. 
                    ComboPopUp.SetActive(true);
                    ComboDisplay.text = ("X" + comboAmount);

                    if (comboAmount >= rainbowFromCombo || debugForceRainbow)
                    {
                        ComboDisplay.color = GetRainbowColor(rainbowSpeed);
                    }

                    UpdateComboBar();
                }
            }

            if (comboTimer <= -comboGraceTime)
            {
                ComboPopUp.SetActive(false);
                HideComboBar();

                FindObjectOfType<SoundManagerScript>().StopPlaying("Combo");

                ComboStart = false;

                comboTimer = comboWindow;
                comboAmount = 0;
            }
        }


        // Decrease hunger over time
        if (timerPause == false)
        {
            hungerLevel -= hungerDecayRate * Time.deltaTime;
            hungerLevel = Mathf.Clamp01(hungerLevel); // Keep between 0 and 1

            if (hungerLevel <= 0f)
            {
                timerPause = true; // Stop further decay/updates once the run has ended

                if (GameOverManager.Instance != null && NewScoreManager.Instance != null)
                {
                    GameOverManager.Instance.EndRun(NewScoreManager.Instance.CurrentScore);
                }
            }
        }
    }

    public void SetTimerPaused(bool paused)
    {
        timerPause = paused;
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        // if you colide with an object that has IFeed dog
        // Check what is in the bowl. 
        // if it matches your current food combo then show next one. 

        if (collision.gameObject.GetComponent<IFeedDog>() != null)
        {
            Debug.Log(collision);

            IFeedDog bowl = collision.gameObject.GetComponent<IFeedDog>();

            bowl.WhatsInBowl();
        }
        
    }

    // Read by other scripts (e.g. ScoreTextCombo). Active uses the same rule as the popup.
    // Raised after a correct meal, with the combo count including that meal.
    public event System.Action<int> MealFed;

    // Raised when the wrong food/bowl combo is served.
    public event System.Action WrongMeal;

    public int ComboAmount => comboAmount;
    public bool ComboActive => ComboStart && comboAmount >= 2;

    // Same colour the popup text uses, including the rainbow.
    public Color GetCurrentComboColor()
    {
        if (comboAmount >= rainbowFromCombo || debugForceRainbow)
        {
            return GetRainbowColor(rainbowSpeed);
        }

        if (comboColors.Length == 0)
        {
            return Color.white;
        }

        return comboColors[Mathf.Clamp(comboAmount - 2, 0, comboColors.Length - 1)];
    }

    /// <summary>
    /// Shows the bar, fills it by the time left in the combo window and tints it with the combo colour.
    /// In the warning phase it flashes red and speeds up its animation.
    /// </summary>
    private void UpdateComboBar()
    {
        if (comboBarRoot == null || comboBarFill == null)
        {
            return;
        }

        comboBarRoot.SetActive(true);
        comboBarFill.fillAmount = Mathf.Clamp01(comboTimer / comboWindow);

        bool warning = comboTimer <= warningTime;
        Color comboColor = GetCurrentComboColor();

        comboBarFill.color = warning
            ? Color.Lerp(comboColor, warningColor, Mathf.PingPong(Time.time * warningFlashSpeed, 1f))
            : comboColor;

        if (comboBarAnimator != null)
        {
            comboBarAnimator.speed = warning ? warningAnimSpeed : 1f;
        }
    }

    private void HideComboBar()
    {
        if (comboBarAnimator != null)
        {
            comboBarAnimator.speed = 1f;
        }

        if (comboBarRoot != null)
        {
            comboBarRoot.SetActive(false);
        }
    }

    private Color GetRainbowColor(float cyclesPerSecond)
    {
        float position = Mathf.Repeat(Time.time * cyclesPerSecond, 1f) * rainbowColors.Length;
        int index = Mathf.FloorToInt(position);

        return Color.Lerp(rainbowColors[index], rainbowColors[(index + 1) % rainbowColors.Length], position - index);
    }

    private void UpdateComboLook()
    {
        int extra = comboAmount - 2;   // 0 at x2, 1 at x3, and so on

        // Speed
        ComboAnimator.speed = Mathf.Min(1f + extra * speedPerCombo, maxSpeed);

        // Size
        ComboPopUp.transform.localScale = comboStartScale * Mathf.Min(1f + extra * sizePerCombo, maxSize);

        // Colour: fixed per combo level; rainbow is handled in Update
        if (comboAmount < rainbowFromCombo && !debugForceRainbow && comboColors.Length > 0)
        {
            ComboDisplay.color = comboColors[Mathf.Min(extra, comboColors.Length - 1)];
        }
    }

    /// <summary>
    ///  // in here we want to compare if the items given are the same as items wanted. 
    /// </summary>
    /// <param name="ItemInBowl"></param>
    /// <param name="ColorOfBowl"></param>
    public void FeedDog(string ItemInBowl, string ColorOfBowl)
    {
      
        // show particls.
        Instantiate(Particles, ParticlePoint.transform.position, ParticlePoint.transform.rotation);

        // vibrate. 
        //HapticFeedback.HeavyFeedback();

        if (ChosenFood == ItemInBowl && ChosenBowl == ColorOfBowl)
        {
           // play animations. 
            Eating.SetTrigger("Eating");
            Eating.SetBool("Hovering", false);

            FindObjectOfType<SoundManagerScript>().Play("Dog");

            // Increase hunger when fed correctly (capped at 100%)
            hungerLevel = Mathf.Min(hungerLevel + hungerGainPerMeal, 1f);

            // start the combo. 
            ComboStart = true;

            comboAmount += 1;

            // Feeding as part of a combo (2nd meal onwards) gives a bonus to hunger.
            if (comboAmount >= 2)
            {
                hungerLevel = Mathf.Min(hungerLevel + comboHungerBonus, 1f);
            }
            if (comboAmount >= 2)
            {
                UpdateComboLook();

                FindObjectOfType<SoundManagerScript>().Play("Ding");
            }

            comboTimer = comboWindow;

            MealFed?.Invoke(comboAmount);

            // Score for this meal scales with the current combo tier and is sent immediately.
            if (NewScoreManager.Instance != null)
            {
                NewScoreManager.Instance.AddScore(feedingBaseAward * comboAmount, comboAmount >= 2);
            }

            // Flat coin reward per successful meal.
            if (CoinManager.Instance != null)
            {
                CoinManager.Instance.AddCoins(coinsPerMeal);
            }

            // Next meal
            foodComboRef.NextMeal();

            // display the next meal
            DisplayNewFood();
           
        }
        else
        {
            //SessionEconomyManager.Instance?.RegisterMealResult(false);

            // Slight hunger decrease on wrong food (5% penalty)
            hungerLevel = Mathf.Max(hungerLevel - 0.05f, 0f);

            WrongMeal?.Invoke();

            FindObjectOfType<SoundManagerScript>().Play("CantPlace");
            // play dog animation
            // coin pop up

            // pause timer
            Debug.Log("-5 coins, hunger decreased");
        }
    }
}
