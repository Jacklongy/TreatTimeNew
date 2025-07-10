using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chicken : Item
{
    #region Animations
    public Animator Leftflap;
    public Animator Rightflap;
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
        base.Grabbed();

        #region Start Animation
        Leftflap.SetBool("Grabbed", true);
        Rightflap.SetBool("Grabbed", true);
        LeftAwake.SetBool("Grabbed", true);
        RightAwake.SetBool("Grabbed", true);
        LeftLeg.SetBool("Grabbed", true);
        RightLeg.SetBool("Grabbed", true);
        Panic.SetBool("Grabbed", true);
        #endregion
    }

    public override string GridTag()
    {
        return gridTag;
    }

    public override void Dropped()
    {
        base.Dropped();
        FindObjectOfType<SoundManagerScript>().StopPlaying("ChickenFlap");
    }

    public override void Merge(GameObject mergeMe)
    {
        base.Merge(mergeMe);

        if (mergeMe.CompareTag(this.tag))
        {
            Debug.Log("i merged");
            Instantiate(Cooked, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -0.5f), Quaternion.identity);

            FindObjectOfType<SoundManagerScript>().StopPlaying("ChickenFlap");

            // play somesort of animation

            Destroy(gameObject);
            // grid location, spawn chicken. 
        }
    }

    public override void ResetAnim()
    {
        #region
        Leftflap.SetBool("Grabbed", false);
        Rightflap.SetBool("Grabbed", false);
        LeftAwake.SetBool("Grabbed", false);
        RightAwake.SetBool("Grabbed", false);
        LeftLeg.SetBool("Grabbed", false);
        RightLeg.SetBool("Grabbed", false);
        Panic.SetBool("Grabbed", false);
        #endregion
    }
}
