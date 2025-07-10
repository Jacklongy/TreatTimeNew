using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigFish : Item
{
    #region Animations
    public Animator Panic;
    #endregion


    public GameObject Cooked;
    public string gridTag = "FoodArea";

    void Start()
    {
        OnGrid = true;
        gameObject.layer = 7;
    }

    public override void Grabbed()
    {
        Panic.SetBool("Grabbed", true);
        base.Grabbed();
    }

    public override string GridTag()
    {
        return gridTag;
    }

    public override void Merge(GameObject mergeMe)
    {
        base.Merge(mergeMe);

        if (mergeMe.CompareTag(this.tag))
        {
            Debug.Log("i merged");
            Instantiate(Cooked, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -0.5f), Quaternion.identity);

            // play somesort of animation

            Destroy(gameObject);
            // grid location, spawn chicken. 
        }
    }

    public override void ResetAnim()
    {
        Panic.SetBool("Grabbed", false);
    }
}
