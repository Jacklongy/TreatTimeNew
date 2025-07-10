using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FoodCombo : MonoBehaviour
{
    // have string containers to say what the food combo has.
    public string foodNameUI;
    public string bowlNameUI;

    public Transform ShowFood;
    public Transform ShowBowl;

    // List of Sprites
    public GameObject[] FoodImages;
    public GameObject[] BowlImages;

    // A dictionary allows us to a asign a tag with an image.
    private Dictionary<string, GameObject> FoodImageTags;
    private Dictionary<string, GameObject> BowlImageTags;


    private void Awake()
    {

        FoodImageTags = new Dictionary<string, GameObject>();
        BowlImageTags = new Dictionary<string, GameObject>();
       

        // Add food tags and the images
        FoodImageTags.Add("CookedChicken", FoodImages[0]);
        FoodImageTags.Add("CookedFish", FoodImages[1]);
        FoodImageTags.Add("CookedPork", FoodImages[2]);
        FoodImageTags.Add("CookedSteak", FoodImages[3]);


        // add bowls and there images
        BowlImageTags.Add("Blue", BowlImages[0]);
        BowlImageTags.Add("Green", BowlImages[1]);
        BowlImageTags.Add("Purple", BowlImages[2]);
        BowlImageTags.Add("Red", BowlImages[3]);

        if (FoodImageTags.Count == 0 || BowlImageTags.Count == 0)
        {
            Debug.LogError("Dictionaries are empty. Check the Start method.");
            return;
        }
    }

    /// <summary>
    /// maybe for showing next meal. 
    /// </summary>
    public void NextMealPeak()
    {

    }

    public void NextMeal()
    {
        Debug.Log("Next meal");

        FoodImageTags.Clear();
        BowlImageTags.Clear();

        Transform childFood = ShowFood.GetChild(0);

        Destroy(childFood.gameObject);

        Transform childBowl = ShowBowl.GetChild(0);

        Destroy(childBowl.gameObject);


        FoodImageTags = new Dictionary<string, GameObject>();
        BowlImageTags = new Dictionary<string, GameObject>();


        // Add food tags and the images
        FoodImageTags.Add("CookedChicken", FoodImages[0]);
        FoodImageTags.Add("CookedFish", FoodImages[1]);
        FoodImageTags.Add("CookedPork", FoodImages[2]);
        FoodImageTags.Add("CookedSteak", FoodImages[3]);


        // add bowls and there images
        BowlImageTags.Add("Blue", BowlImages[0]);
        BowlImageTags.Add("Green", BowlImages[1]);
        BowlImageTags.Add("Purple", BowlImages[2]);
        BowlImageTags.Add("Red", BowlImages[3]);

        if (FoodImageTags.Count == 0 || BowlImageTags.Count == 0)
        {
            Debug.LogError("Dictionaries are empty. Check the Start method.");
            return;
        }
    }


    public void DisplayWantedFood(string food, string bowl)
    {

        //Debug.Log(FoodImageTags.Count);

        foodNameUI = food;
        bowlNameUI = bowl;

      
        foreach (var tag in FoodImageTags)
        {

            if (tag.Key == foodNameUI)
            {
                Instantiate(tag.Value, ShowFood.transform);
            }
        }

        foreach (var tag in BowlImageTags)
        {
            if (tag.Key == bowlNameUI)
            {
                Instantiate(tag.Value, ShowBowl.transform);
            }
        }
    }


}
