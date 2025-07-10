using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class DataDisplayMenu : MonoBehaviour
{

    // This is just used to show Data in main menu! 
  
    public int TotalCoinsDisplay;

    public TextMeshProUGUI coinUI; 

    public TextMeshProUGUI XPUI;
    public TextMeshProUGUI LvlUI;

    public TextMeshProUGUI HighScore;

    public Image XPBar;
 


    GameOverseer overseer;

    SoundManagerScript sound;

    private void Start()
    {
        sound = FindObjectOfType<SoundManagerScript>();

        sound.Play("LobbyMusic");

        sound.StopPlaying("Treat Time Music");

        overseer = FindObjectOfType<GameOverseer>();
        LoadData();
    }

    
    public void LoadData()
    {

        HighScore.text = ("Best Score: " + overseer.HighScoreTimed.ToString());
        coinUI.text = (overseer.Coins.ToString());

        float start = overseer.TotalXP - overseer.PreviousLvlXP;
        float end = overseer.NextLevelXP - overseer.PreviousLvlXP;


        LvlUI.text = overseer.CurrentLevel.ToString();
        XPUI.text = start + "XP / " + end + "XP";
        XPBar.fillAmount = start / end;

        Debug.Log("this is the display next level xp " + end);
    }

}
