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

        if (overseer != null && overseer.Vibrations == true)
        {
            Debug.Log("Vibrate");
            //HapticFeedback.MediumFeedback();
        }

        // this makes sure we hit the item instead of the tile first
        gameObject.transform.position = new Vector3(transform.position.x, transform.position.y, -0.5f);

        OnGrid = true;
        gameObject.layer = 7;

        // sounds
        if (sound != null)
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

    #region Item Slots (DEPRECATED - unified grid system)
    public void ReturnToMySlot()
    {
        // Old slot system removed - this should no longer be called
        Debug.LogWarning("ReturnToMySlot called but slot system is deprecated");
        ResetAnim();
    }

    public void MySlot(GameObject slot)
    {
        // Old slot system removed
        Debug.LogWarning("MySlot called but slot system is deprecated");
    }
    #endregion

    #region Animations

    public virtual void ResetAnim()
    {
       
        Debug.Log("no Anim");
    }
    #endregion
}
