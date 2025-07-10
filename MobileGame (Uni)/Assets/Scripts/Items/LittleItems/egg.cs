using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Egg : Item
{
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
    #endregion

    // Old Code
    //public GameObject Chicken;
    //public GameObject mySlot;
    //public bool OnGrid = false;
    //public string mergeTag;
    //void Start()
    //{
    //    gameObject.layer = 3;
    //}

    //public void Dropped()
    //{

    //    // this makes sure we hit the item instead of the tile first
    //    gameObject.transform.position = new Vector3(transform.position.x, transform.position.y, -0.5f);


    //    if (!OnGrid)
    //    {
    //        IItemSlot slot = mySlot.GetComponent<IItemSlot>();

    //        slot.WatchItem(false);

    //        OnGrid = true;
    //    }

    //    // removes slot because its been put on map
    //    mySlot = null;


    //    if(OnGrid && mySlot == null)
    //    {
    //        Debug.Log("no slot and im on the grid");
    //    }

    //    return;
    //   // Debug.Log("Dropped");
    //}

    //public void Grabbed()
    //{

    //    return;
    //  //  Debug.Log("Grabbed");
    //}


    //public bool GridCheck()
    //{
    //    if (OnGrid)
    //    {
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}


    //public void ImergeWith(GameObject mergeMe)
    //{

    //    if (mergeMe.CompareTag(mergeTag))
    //    {
    //        Debug.Log("i merged");
    //        Instantiate(Chicken, new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, -0.5f), Quaternion.identity);

    //        // play somesort of animation

    //        Destroy(gameObject);
    //        // grid location, spawn chicken. 
    //    }
    //}

    //// takes a reference to the slot this object is in
    //public void MySlot(GameObject slot)
    //{
    //    mySlot = slot;

    //    Debug.Log("My Slot is " + slot.name);
    //}


    //// this will tell the the slot that there is no space and call the return fuction.
    //public void ReturnToSlot()
    //{
    //   IItemSlot slot = mySlot.GetComponent<IItemSlot>();

    //    slot.WatchItem(true);
    //}

    //public void MergeFromInvCheck()
    //{
    //    if(OnGrid == false)
    //    {
    //        Debug.Log("Merged from inv");

    //        ItemSpawner.slotEmpty -= 1;
    //    }
    //}

    //public void ResetAnim()
    //{
    //    Debug.Log("No Anim");

    //    return;
    //}
}
