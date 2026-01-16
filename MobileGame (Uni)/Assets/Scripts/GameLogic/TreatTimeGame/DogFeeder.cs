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
    public GameObject TimeUpScreen;
    public TextMeshProUGUI CoinTotal;
    public TextMeshProUGUI MealsTotal;

    [Header("Pop Ups")]
    public GameObject WrongFoodPopUp;
    public GameObject ComboPopUp;
    public TextMeshPro ComboDisplay;
    public GameObject AdScreen;
    public GameObject Tutorial;

    [Header("Particles")]
    public GameObject Particles;
    public GameObject ParticlePoint;

    [Header("XP")]
    public float TotalXP;
    public int CurrentLevel;
    public float PreviousLvlXP;
    public int NextLevelXP;

    [Header("XP Interface")]
    public TextMeshProUGUI LevelTxt;
    public TextMeshProUGUI XPTxt;
    public Image XPfill;
    public AnimationCurve XPCurve;
    public GameObject XPBar;

    [Header("Animations")]
    public Animator Ads;
    public Animator Eating;

    [Header("Timers")]
    bool timerPause;
    public float hungerLevel = 0.5f; // Starts at 50% hunger
    public float hungerDecayRate = 0.15f; // How fast hunger depletes per second
    public float hungerGainPerMeal = 0.25f; // How much hunger increases per correct meal
    public float comboTimer;

    [Header("Stats this game")]
    public int mealCount;  
    public int coinCount;
    public float XPthisGame;

    [Header("Data being saved")]
    public int FinalCoins;
    public int FinalMealCount;
    public float FinalXP;
    public int hasPlayed;
    public int HighScore;

    // For showing results over time.
    // So we can watch the coins go up!
    float coinDisplay;
    float mealsDisplay;

    // Combo Multiplier
    int comboAmount;
    bool ComboStart = false;

    [Header("References")]
    public FoodCombo foodComboRef;
    public ItemSpawner itemSpawner;
    public GameOverseer overseer;
    public SoundManagerScript sound;
    public GameObject gameManager;
    Grab grab;


    [Header("Bools")]
    public bool GameEnded;
    bool AddXP = true;
    public bool countDownSound;
    bool ding1, ding2, ding3;
    bool hungerDepleted = false; // Prevent hunger end trigger from firing multiple times

    [Header("Ads")]
    public bool ad;
    public int ad_CoolDown;

  
    /// <summary>
    /// Save the data from this game! Talking to overseer which will then save the stats to the phone. 
    /// </summary>
    public void SaveData()
    {   // add the coins to the instance of player data! Which will then be added to the total coins in the display menu.
        if(FinalCoins > 0)
        {
            overseer.Coins += FinalCoins;
        }
  
        overseer.TotalXP = TotalXP;
        overseer.PreviousLvlXP = PreviousLvlXP;
        overseer.NextLevelXP = NextLevelXP;
        overseer.CurrentLevel = CurrentLevel;
        overseer.hasPlayed = hasPlayed;

        if (mealCount > HighScore)
        {
            overseer.HighScoreTimed = mealCount;

            // Highscore PopUp etc
        }
    }

    /// <summary>
    /// When scene is disabled save
    /// </summary>
    private void OnDisable()
    {
        SaveData();
        
        if (overseer != null)
        {
            overseer.CloudDataBackUp();
        }
    }

    /// <summary>
    /// When scene is enabled stop lobby music and play new music. 
    /// </summary>
    private void OnEnable()
    {
        FindObjectOfType<SoundManagerScript>().StopPlaying("LobbyMusic");
        FindObjectOfType<SoundManagerScript>().Play("Treat Time Music");
       
    }

    /// <summary>
    /// When the player quits save. 
    /// </summary>
    private void OnApplicationQuit()
    {
        SaveData();
        
        if (overseer != null)
        {
            overseer.CloudDataBackUp();
        }
    }

  
    /// <summary>
    /// if the player exits of screen, pause the timer. 
    /// </summary>
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            timerPause = true;
        }
        else
        {
            timerPause = false;
        }
    }

    /// <summary>
    /// Add the XP
    /// </summary>
    /// <param name="XPEarned"> From this game </param>
    public void AddExperience(float XPEarned)
    {
        TotalXP += XPEarned;
        // check if we level up. 
        CheckForLevelUp();
    }

    /// <summary>
    /// Checking for level up
    /// </summary>
    public void CheckForLevelUp()
    {
        // While loop so it counts for multiple levels. 
        while (TotalXP >= NextLevelXP)
        {
            CurrentLevel++;
            // update the level. 
            UpdateLevel();

            // this is where we can add effects and stuff
        }
    }

    /// <summary>
    /// Update level to match new level and exp. 
    /// </summary>
    void UpdateLevel()
    {
        PreviousLvlXP = (int)XPCurve.Evaluate(CurrentLevel);
        NextLevelXP = (int)XPCurve.Evaluate(CurrentLevel + 1);

        Debug.Log($"PreviousLvlXP: {PreviousLvlXP}, NextLevelXP: {NextLevelXP}");

        // Update the interface. 
        UpdateInterface();
    }

    /// <summary>
    /// Updating the interface to display correct level. 
    /// </summary>
    private void UpdateInterface()
    {
        float start = TotalXP - PreviousLvlXP;
        float end = NextLevelXP - PreviousLvlXP; 

        Debug.Log($"start: {start}, end: {end}");

        LevelTxt.text = CurrentLevel.ToString();
        XPTxt.text = start + "XP / " + end + "XP";

        XPfill.fillAmount = start / end;
    }

    /// <summary>
    /// When game starts we want to find important objects e.g the overseer, set all data to the current data. 
    /// and reset any variables from last run. 
    /// </summary>
    void Start()
    {

        grab = FindObjectOfType<Grab>();

        grab.NoPickUp = false;
   
        overseer = FindObjectOfType<GameOverseer>();

        TotalXP = overseer.TotalXP;
        PreviousLvlXP = overseer.PreviousLvlXP;
        NextLevelXP = overseer.NextLevelXP;
        CurrentLevel = overseer.CurrentLevel;
        HighScore = overseer.HighScoreTimed;
        hasPlayed = overseer.hasPlayed;

        // Initialize hunger to middle (50%)
        hungerLevel = 0.5f;

        comboTimer = 10f;
        comboAmount = 0;

        AddXP = true;
        GameEnded = false;
        countDownSound = true;
        ad = false;
        ding1 = false;
        ding2 = false;
        ding3 = false;
        hungerDepleted = false; // Reset hunger depletion flag

        // If the player has played dont show the tutorial. 
        if (hasPlayed == 0)
        {
            Tutorial.SetActive(true);

            timerPause = true;
        }
        else
        {
            timerPause = false;
        }

        // randomy pick a gameObject/ item and a bowl color. 
        // go to food wanted script and display the sprites that match with the current combo. 
        ChosenFood = Foods[Random.Range(0, Foods.Length)];
        ChosenBowl = bowls[Random.Range(0, Foods.Length)];

        display.GetComponent<FoodCombo>().DisplayWantedFood(ChosenFood, ChosenBowl);
    }

    /// <summary>
    /// Close tutorial screen. 
    /// </summary>
    public void CloseTutorial()
    {
        Tutorial.SetActive(false);

        timerPause = false;
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

    /// <summary>
    /// Ad Reward. So grant another minute and half. 
    /// </summary>
   public void AdReward(bool NoAd)
    {
        ad = NoAd;
    }

    /// <summary>
    /// Game has ended. So the player has now played. 
    /// </summary>
    public void GameHasEnded()
    {
        GameEnded = true;
        hasPlayed = 1;
       
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
        }

        // Show the current amount of meals. 
        MealCountDisplay.text = (mealCount.ToString());

        // Calculates the XP for the end of the game. 
        XPthisGame = mealCount * coinCount / 2;

        // Trigger the end when hunger reaches 0 (ONLY ONCE)
        if (hungerLevel <= 0 && !hungerDepleted)
        {
            hungerDepleted = true; // Prevent this from running multiple times
            grab.NoPickUp = true;

            // Depending on what the player chooses. 
            if (ad_CoolDown < 2)
            {
                Ads.SetTrigger("PopUp");
               
            }
            else if(ad_CoolDown >= 2)
            {
                ad = false;
                GameEnded = true;
            }
          
            timerPause = true;

            // if the player doesnt watch add. and the game has now ended. 
            // Display time up screen and stats from that game. 
            if (ad == false && GameEnded == true)
            {
                Grab grab = gameManager.GetComponent<Grab>();

                grab.enabled = false;

                MealsTotal.gameObject.SetActive(true);

                if (mealsDisplay <= mealCount)
                {
                  
                    mealsDisplay += 1 * Time.deltaTime * 30;
                }

                if (mealsDisplay >= mealCount)
                {
                    if(ding1 == false)
                    {
                        FindObjectOfType<SoundManagerScript>().Play("Merge");

                        ding1 = true;
                    }
                  
                    CoinTotal.gameObject.SetActive(true);

                    if (coinDisplay <= coinCount)
                    {
                        coinDisplay += 1 * Time.deltaTime * 30;
                    }


                    if (coinDisplay >= coinCount)
                    {
                        if(ding2 == false)
                        {
                            FindObjectOfType<SoundManagerScript>().Play("Ding");

                            ding2 = true;
                        }
                     
                        // Set the xp active
                        XPBar.SetActive(true);

                        // Final xp is = to the xp this game. So dont go over and keep adding xp. 
                        FinalXP = XPthisGame;

                        if (AddXP)
                        {

                            // we want to add exp from this game. 
                            AddExperience(XPthisGame);
                          
                            // we only want to add it until it = the correct amount. 
                            if (XPthisGame >= FinalXP)
                            {
                                if(ding3 == false)
                                {
                                    FindObjectOfType<SoundManagerScript>().Play("Merge");
                                    ding3 = true;
                                }
                               
                                AddXP = false;
                            }
                                
                        }
                    }

                }

                // in time up screen display stats and give coins and experience
                CoinTotal.text = string.Format("Coins: {0:0}", coinDisplay);
                MealsTotal.text = string.Format("Meals: {0:0}", mealsDisplay);

                // Final coin count. 
                FinalCoins = coinCount;

                TimeUpScreen.SetActive(true);
                timerPause = true;
            }
            // if the player watches ad. 
            else if(ad == true)
            {
                grab.NoPickUp = false;

                grab.enabled = true;

                Ads.SetTrigger("Hide");

                timerPause = false;

                // Restore hunger to 50% after watching ad
                hungerLevel = 0.5f;

                ad_CoolDown += 1;

                ad = false;

                countDownSound = true;
            }
        }
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

            // increase meal count. 
            mealCount++;

            // Increase hunger when fed correctly (capped at 100%)
            hungerLevel = Mathf.Min(hungerLevel + hungerGainPerMeal, 1f);

            // start the combo. 
            ComboStart = true;

            comboAmount += 1;

            if (comboAmount >= 2)
            {
                FindObjectOfType<SoundManagerScript>().Play("Ding");
            }

            comboTimer = 10f;

            coinCount += 2 * comboAmount;

            // reset shuffle count
            itemSpawner.shuffleCount = 5;

            // Next meal
            foodComboRef.NextMeal();

            // display the next meal
            DisplayNewFood();
           
        }
        else
        {
            coinCount -= 5;

            // Slight hunger decrease on wrong food (5% penalty)
            hungerLevel = Mathf.Max(hungerLevel - 0.05f, 0f);

            FindObjectOfType<SoundManagerScript>().Play("CantPlace");
            // play dog animation
            // coin pop up

            // pause timer
            Debug.Log("-5 coins, hunger decreased");
        }
    }

    
    public void QuitButton()
    {

        Quit.SetActive(true);

        timerPause = true;
        // pause timer
    }

    public void Resume()
    {
        Quit.SetActive(false);

        timerPause = false;
        // resume timer
    }

    


}
