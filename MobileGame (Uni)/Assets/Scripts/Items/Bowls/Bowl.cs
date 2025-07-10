using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bowl is an item but also uses interfaces to seperate itself from other items. 
/// </summary>
public class Bowl : Item , IBowl, IFeedDog
{
    public GameObject ItemInBowl;
    public string gridTag = "BowlArea";
    public string ItemInBowlTag;
    public GameObject NextBowl;
    DogFeeder feeder;
    Vector3 offset;
    public GameObject coin;
    public GameObject PlaceInBowlEffect;
   
    void Start()
    {
        if (CompareTag("Blue"))
        {
            gameObject.layer = 3;

        }
        else
        {
            OnGrid = true;
            gameObject.layer = 7;
        }

    }

    public override string GridTag()
    {
        return gridTag;
    }

    #region IBowl / IFeedDog
    public void TakeItem(GameObject Item)
    {

        Instantiate(PlaceInBowlEffect, gameObject.transform.position, gameObject.transform.rotation);

        FindObjectOfType<SoundManagerScript>().Play("PlaceInBowl");

        // so food pops out the top slightly
        offset = new Vector3(0, 0.2f, 0);

        // add item to bowl.
        if (Item.GetComponent<IBowlable>() != null && ItemInBowl == null)
        {
            ItemInBowl = Item;
            ItemInBowlTag = ItemInBowl.tag;

            ItemInBowl.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);

            ItemInBowl.transform.position = this.transform.position + offset;

            ItemInBowl.transform.SetParent(this.transform);

            Debug.Log("item took" + ItemInBowl);

        }
    }

    public void WhatsInBowl()
    {
        feeder = FindObjectOfType<DogFeeder>();
        // pass these values into the dog feeder function
        feeder.FeedDog(ItemInBowlTag, this.gameObject.tag);

        Destroy(gameObject);
    }



    #endregion

    #region Item
    public override void Merge(GameObject mergeMe)
    {

        base.Merge(mergeMe);

        if (mergeMe.CompareTag("Red"))
        {
            Debug.Log("i merged");
            Instantiate(coin, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -0.5f), Quaternion.identity);

            Destroy(gameObject);
        }

        // Only Merge if you dont have an item in already
        // Change into the next bowl
        if (NextBowl != null)

            if (ItemInBowl == null)
            {
              
                if (mergeMe.CompareTag(tag))
                {
                    Debug.Log("i merged");
                    Instantiate(NextBowl, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -0.5f), Quaternion.identity);

                    Destroy(gameObject);
                }

               
            }
            
    }

    public bool CheckFull()
    {
        if(ItemInBowl == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

   
    #endregion

}
