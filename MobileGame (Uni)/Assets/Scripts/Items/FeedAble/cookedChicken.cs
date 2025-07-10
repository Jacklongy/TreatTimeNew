using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookedChicken : Item, IBowlable
{
    public GameObject coin;
    public string gridTag = "FoodArea";

    void Start()
    {
        OnGrid = true;
        gameObject.layer = 7;
    }

    public override void Grabbed()
    {
        base.Grabbed();
    }


    public override string GridTag()
    {
        return gridTag;
    }

    public override void Merge(GameObject MergeMe)
    {
        base.Merge(MergeMe);

        if (MergeMe.CompareTag(this.tag))
        {
            Debug.Log("i merged");
            Instantiate(coin, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -0.5f), Quaternion.identity);

            // particle effects

            Destroy(gameObject);
        }

        // Turn to coal 
    }
}
