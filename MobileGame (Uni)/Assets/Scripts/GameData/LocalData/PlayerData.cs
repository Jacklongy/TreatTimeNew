using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

/// <summary>
/// Different types of data to be saved. 
/// This is a middle man for the save system and the Game Overseer. 
/// </summary>
[System.Serializable]
public class PlayerData
{
    public string DogName;

    public int Coins;
    public float TotalXP;
    public int CurrentLevel;
    public float PreviousLvlXP;
    public int NextLevelXP;
    public int Gems;
    public float PlayerRep;

    public int hasPlayed;
   
    public int HighScoreTimed;
    public int HighScoreUnlim;

    /// <summary>
    /// Sets the data = to the data from game overseer, which comunicates with the treat time game. 
    /// so all the updated data from the game gets sent here when we save. 
    /// </summary>
    /// <param name="gameOverseer"></param>
    public PlayerData(GameOverseer gameOverseer)
    {
        Coins = gameOverseer.Coins;
        TotalXP = gameOverseer.TotalXP;
        PreviousLvlXP = gameOverseer.PreviousLvlXP;
        NextLevelXP = gameOverseer.NextLevelXP;
        CurrentLevel = gameOverseer.CurrentLevel;
        hasPlayed = gameOverseer.hasPlayed;
        HighScoreTimed = gameOverseer.HighScoreTimed;
    }

}
