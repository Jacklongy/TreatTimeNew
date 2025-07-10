using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigCow : Item
{

    #region Animations
    public Animator LeftAwake;
    public Animator RightAwake;
    public Animator Feet;
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

        #region Start Animation
        LeftAwake.SetBool("Grabbed", true);
        RightAwake.SetBool("Grabbed", true);
        Feet.SetBool("Grabbed", true);
        Panic.SetBool("Grabbed", true);
        #endregion

        base.Grabbed();

    }

    public override string GridTag()
    {
        return gridTag;
    }

    public override void Merge(GameObject mergeMe)
    {
        base.Merge(mergeMe);

        if (mergeMe.CompareTag(tag))
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
        #region Reset anim
        LeftAwake.SetBool("Grabbed", false);
        RightAwake.SetBool("Grabbed", false);
        Feet.SetBool("Grabbed", false);
        Panic.SetBool("Grabbed", false);
        #endregion
    }
}
