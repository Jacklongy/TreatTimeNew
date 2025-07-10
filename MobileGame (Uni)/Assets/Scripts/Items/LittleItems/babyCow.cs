using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BabyCow : Item
{
    #region Animations
    public Animator LeftAwake;
    public Animator RightAwake;
    public Animator Panic;
    public Animator Feet;
    #endregion


    #region Variables and Start Method
    // Merging
    public GameObject nextObject;
    public string gridTag = "FoodArea";

    private void Start()
    {
        // Set layer to inv so we can merge within the inventory.
        gameObject.layer = 3;
    }
    #endregion

    #region Grabbing and Merging
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

    public override void Merge(GameObject MergeMe)
    {
        base.Merge(MergeMe);

        if (MergeMe.CompareTag(this.tag))
        {
            Debug.Log("i merged");
            Instantiate(nextObject, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -0.5f), Quaternion.identity);

            Destroy(gameObject);
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
    #endregion
}
