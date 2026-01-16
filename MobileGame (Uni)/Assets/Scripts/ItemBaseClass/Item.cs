using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
//using CandyCoded.HapticFeedback;

/// <summary>
/// This class runs all items, All basic logic for all items is here. 
/// The items can take these functions, overide them and do something else, and do whats in the main function. 
/// very powerful and useful. 
/// </summary>
public abstract class Item : MonoBehaviour
{
    GameOverseer overseer;

    // Grid and Inventory
    public bool OnGrid = false;
    public GameObject mySlot;
    SoundManagerScript sound;
    public GameObject particlesDrop;
    public GameObject particlesMerge;
    public Animator Placed;

    private void Awake()
    {
        sound = FindObjectOfType<SoundManagerScript>();
        overseer = FindObjectOfType<GameOverseer>();
    }

    #region Grabbing and Merging
    public virtual void Grabbed()
    {

        FindObjectOfType<SoundManagerScript>().Play("Grabbed");

        if (overseer.Vibrations == true)
        {
            //HapticFeedback.LightFeedback();
            Debug.Log("Grabbed");
        }
           
    }

    public virtual void Merge(GameObject MergeMe)
    {

        // Particle effects
        Instantiate(particlesMerge, gameObject.transform.position, gameObject.transform.rotation);

        FindObjectOfType<SoundManagerScript>().Play("Merge");

        // make sure to include base for all items. 

        if (overseer.Vibrations == true)
        {
            Debug.Log("Vibrate");
            //HapticFeedback.MediumFeedback();
        }
        // you can impliment base functionality here 
        // but for specific tag use overide in item script. 
    }

    public virtual string GridTag()
    {
        return null;
    }

    public virtual void Dropped()
    {
        Instantiate(particlesDrop, gameObject.transform.position, gameObject.transform.rotation);

      
        if (overseer.Vibrations == true)
        {
            //Handheld.Vibrate();

            Debug.Log("Vibrate");
            //HapticFeedback.MediumFeedback();
        }
      
        // this makes sure we hit the item instead of the tile first
        gameObject.transform.position = new Vector3(transform.position.x, transform.position.y, -0.5f);

        if (!OnGrid && mySlot != null)
        {
            IItemSlot slot = mySlot.GetComponent<IItemSlot>();

            slot.ReturnToSlot(false);

            OnGrid = true;

            gameObject.layer = 7;
        }

        // removes slot because its been put on map
        mySlot = null;

        if (OnGrid && mySlot == null)
        {
            Debug.Log("no slot and im on the grid");
        }

        // sounds
        sound.Play("Place");

    }

    public bool GridCheck()
    {
        if (OnGrid)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void MergeFromInvCheck()
    {
        if (OnGrid == false)
        {
            //Debug.Log("Merged from inv");

            // DEPRECATED: slotEmpty no longer used - unified grid system in place
            //ItemSpawner.slotEmpty -= 1;
        }
    }
    #endregion

    #region Item Slots
    public void ReturnToMySlot()
    {
        IItemSlot slot = mySlot.GetComponent<IItemSlot>();
        ResetAnim();
        slot.ReturnToSlot(true);
    }

    public void MySlot(GameObject slot)
    {
        mySlot = slot;

       // Debug.Log("My Slot is " + slot.name);
    }
    #endregion

    #region Animations

    public virtual void ResetAnim()
    {
       
        Debug.Log("no Anim");
    }
    #endregion
}
