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

    [Header("Displays")]
    public GameObject display;
    public TextMeshProUGUI MealCountDisplay;
    public Image HungerBar; // Drag your hunger bar Image here

    [Header("Menus")]
    public GameObject Quit;
    public GameObject Win;

    [Header("Pop Ups")]
    public GameObject WrongFoodPopUp;
    public GameObject ComboPopUp;
    public TextMeshPro ComboDisplay;

    [Header("Particles")]
    public GameObject Particles;
    public GameObject ParticlePoint;

    [Header("Animations")]
    public Animator Eating;

    [Header("Timers")]
    bool timerPause;
    public float hungerLevel = 0.5f; // Starts at 50% hunger
    public float hungerDecayRate = 0.25f; // How fast hunger depletes per second
    public float hungerGainPerMeal = 0.50f; // How much hunger increases per correct meal
    public float comboTimer;

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
        comboTimer = 10f;
        comboAmount = 0;

        // randomy pick a gameObject/ item and a bowl color. 
        // go to food wanted script and display the sprites that match with the current combo. 
        ChosenFood = Foods[Random.Range(0, Foods.Length)];
        ChosenBowl = bowls[Random.Range(0, Foods.Length)];

        display.GetComponent<FoodCombo>().DisplayWantedFood(ChosenFood, ChosenBowl);
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
        ChosenFood = Foods[Random.Range(0, Foods.Length)];
        ChosenBowl = bowls[Random.Range(0, Foods.Length)];

        display.GetComponent<FoodCombo>().DisplayWantedFood(ChosenFood, ChosenBowl);
    }

    void Update()
    {
        // Update hunger bar UI every frame
        UpdateHungerBar();

        // Manages the combo logic. 
        if (ComboStart == true)
        {
            comboTimer -= Time.deltaTime;

            if (comboTimer < 10f)
            {
                if (comboAmount >= 2)
                {
                    // Display pop up. 
                    ComboPopUp.SetActive(true);
                    ComboDisplay.text = ("X" + comboAmount);
                }

               
            }

            if (comboTimer <= 0f)
            {
                ComboPopUp.SetActive(false);

                FindObjectOfType<SoundManagerScript>().StopPlaying("Combo");

                ComboStart = false;

                comboTimer = 10f;
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

            if (comboAmount >= 2)
            {
                FindObjectOfType<SoundManagerScript>().Play("Ding");

                // Add bonus to the hungerbar for achieving a combo
                hungerLevel = Mathf.Min(hungerLevel + hungerGainPerMeal, 1f);
            }

            comboTimer = 10f;

            // Score for this meal scales with the current combo tier and is sent immediately.
            if (NewScoreManager.Instance != null)
            {
                NewScoreManager.Instance.AddScore(feedingBaseAward * comboAmount);
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

            FindObjectOfType<SoundManagerScript>().Play("CantPlace");
            // play dog animation
            // coin pop up

            // pause timer
            Debug.Log("-5 coins, hunger decreased");
        }
    }
}
