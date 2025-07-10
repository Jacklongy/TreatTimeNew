using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigPig : Item
{
    #region Animations
    public Animator LeftAwake;
    public Animator RightAwake;
    public Animator LeftLeg;
    public Animator RightLeg;
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
        LeftLeg.SetBool("Grabbed", true);
        RightLeg.SetBool("Grabbed", true);
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
            Debug.Log("i merged Big Pig");
            Instantiate(Cooked, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -0.5f), Quaternion.identity);

            Destroy(gameObject);
            
        }
    }

    public override void ResetAnim()
    {
        #region Reset anim
        LeftAwake.SetBool("Grabbed", false);
        RightAwake.SetBool("Grabbed", false);
        LeftLeg.SetBool("Grabbed", false);
        RightLeg.SetBool("Grabbed", false);
        Panic.SetBool("Grabbed", false);
        #endregion
    }
}
