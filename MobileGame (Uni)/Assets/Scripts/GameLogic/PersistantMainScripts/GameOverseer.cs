using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;

[System.Serializable]
public class GameOverseer : MonoBehaviour
{
    
    GameOverseer instance;
    public CloudData cloudData;
    public string DogName;

    //Headers are cool for organisation in the inspector. Ill be using these for code clean ups.
    [Header("XP")]
    public float TotalXP;
    public int CurrentLevel;
    public float PreviousLvlXP;
    public int NextLevelXP;
    public float PlayerRep;


    [Header("Currency")]
    public int Coins;
    public int Gems;

    [Header("HighScores")]
    public int HighScoreTimed;
    public int HighScoreUnlim;

    [Header("UI")]
    public GameObject WelcomeScreen;
    public TextMeshProUGUI CoinsLocal;
    public TextMeshProUGUI XPLocal;
    public TextMeshProUGUI CoinsCloud;
    public TextMeshProUGUI XPCloud;

    DataDisplayMenu display;


    [Header("XP Interface")]
    public TextMeshProUGUI LevelTxt;
    public TextMeshProUGUI XPTxt;
    public Image XPfill;
    public AnimationCurve XPCurve;

    // Settings
    [Header("Settings")]
    public bool Vibrations;
   
    public int hasPlayed;

  /// <summary>
  /// Make sure there is only one Overseer. 
  /// </summary>
    void Awake()
    {
        // Load the Data on awake. 
        LoadData();

        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// When we quit the game save. 
    /// </summary>
    private void OnApplicationQuit()
    {
        SaveSystem.Save(this);
    }

    /// <summary>
    /// When we pause save. 
    /// </summary>
    /// <param name="pauseStatus"></param>
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveSystem.Save(this);
        }
    }

    /// <summary>
    /// Save at the start, If the data shows the player is new and hasnt played before. 
    /// set default data. 
    /// </summary>
    private void Start()
    {
        SaveSystem.Save(this);

        // Displaying the data in UI 
        display = FindObjectOfType<DataDisplayMenu>();

        if (hasPlayed == 0)
        {
            WelcomeScreen.SetActive(true);

            Coins = 0;
            TotalXP = 0;
            PreviousLvlXP = 0;
            NextLevelXP = 30;
            CurrentLevel = 0;
            HighScoreTimed = 0;

            // Save again when data is set. 
            SaveSystem.Save(this);
        }
    }


    /// <summary>
    /// This Loads the data from the save system
    /// </summary>
    public void LoadData()
    {
        // Reference to the player data script.
        // Where data is copied from here and used to be saved in the save system. 
        PlayerData data = SaveSystem.LoadData();

        Coins = data.Coins;
        TotalXP = data.TotalXP;
        PreviousLvlXP = data.PreviousLvlXP;
        NextLevelXP = data.NextLevelXP;
        CurrentLevel = data.CurrentLevel;
        hasPlayed = data.hasPlayed;
        HighScoreTimed = data.HighScoreTimed;
    }


    /// <summary>
    /// We can use this function to save from other scripts if needed. 
    /// </summary>
    public void SaveTreatTimeData()
    {
        SaveSystem.Save(this);
    }

  
    /// <summary>
    /// Check if we are online for cloud save. 
    /// </summary>
    /// <returns></returns>
    public bool OnlineCheck()
    {
        switch (Application.internetReachability)
        {
            case NetworkReachability.NotReachable:

                return false;
                
            case NetworkReachability.ReachableViaCarrierDataNetwork:

                return true;
            case NetworkReachability.ReachableViaLocalAreaNetwork:

                return true;
               
            default:

                return false;
                
        }
    }

    /// <summary>
    /// Cloud data back up. Takes our saved data and saves it to cloud. 
    /// </summary>
    public void CloudDataBackUp()
    {
        if (OnlineCheck())
        {
            // uses a Key and value. 
            cloudData.SaveData("Coins", Coins);
            cloudData.SaveData("Level", CurrentLevel);
            cloudData.SaveData("TotalXP", TotalXP);
            cloudData.SaveData("PreviousLvlXP", PreviousLvlXP);
            cloudData.SaveData("NextLvlXP", NextLevelXP);
            cloudData.SaveData("HasPlayed", hasPlayed);
            cloudData.SaveData("HighScore", HighScoreTimed);
        }
        else
        {
            Debug.Log("Not online!!!");
        }
    }


    /// <summary>
    /// Replaces local data with cloud data. There is a button in settings, if data is lost on phone, 
    /// press button to load data from cloud.
    /// </summary>
    public async void UseCloud()
    {
        if(hasPlayed == 1)
        {
            Coins = await cloudData.LoadData<int>("Coins");
            TotalXP = await cloudData.LoadData<float>("TotalXP");
            PreviousLvlXP = await cloudData.LoadData<float>("PreviousLvlXP");
            NextLevelXP = await cloudData.LoadData<int>("NextLvlXP");
            CurrentLevel = await cloudData.LoadData<int>("Level");
            hasPlayed = await cloudData.LoadData<int>("HasPlayed");
            HighScoreTimed = await cloudData.LoadData<int>("HighScore");

            SaveSystem.Save(this);
            display.LoadData();
        }
    }

    /// <summary>
    /// Toggle the vibrations on and Off. I need to add this to saved data. 
    /// as it turns off when switching between scenes. 
    /// </summary>
    public void VibrationsToggle()
    {
        if(Vibrations != true)
        {
            Vibrations = true;
        }
        else
        {
            Vibrations = false;
        }
    }
}
