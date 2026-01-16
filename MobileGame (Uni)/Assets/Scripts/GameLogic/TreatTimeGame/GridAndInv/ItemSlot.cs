// DEPRECATED: ItemSlot is no longer used.
// The new unified grid system uses Tiles and GridManager for all item placement.
// Items are spawned directly onto grid tiles via ItemButton clicks.
// Keeping this file for reference only.

/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSlot : MonoBehaviour, IItemSlot
{
    public bool IsFull;
   public Transform ObjectContainer;
    

    // Start is called before the first frame update
    void Start()
    {
        CheckIfFull();
    }

    public bool CheckIfFull()
    {
        if (ObjectContainer == null)
        {
            IsFull = false;

        }
        else
        {
            Debug.Log("I contain " + ObjectContainer);
        }

        return IsFull;
    }

    public void TakeItem(GameObject item)
    {
        Item ItemRef = item.GetComponent<Item>();

        ItemRef.MySlot(this.gameObject);

        ObjectContainer = item.transform;

        ObjectContainer.transform.position = gameObject.transform.position;
        IsFull = true;
    }

    public void RemoveItem()
    {
       
        ObjectContainer = null;
        IsFull = false;

        if (ObjectContainer == null && IsFull == false)
        {
            ItemSpawner.slotEmpty -= 1;
        }

        //Debug.Log("I Removed " + ObjectContainer);
    }

    public void ShuffleItems()
    {
        //Debug.Log("Item lost");

        ItemSpawner.slotEmpty -= 1;

        Destroy(ObjectContainer.gameObject);

        ObjectContainer = null;
        IsFull = false;

        Debug.Log("I Removed " + ObjectContainer);
    }

    public void SpawnItem(GameObject item)
    {
        Item ItemRef = item.GetComponent<Item>();

        ItemRef.MySlot(gameObject);

        ObjectContainer = item.transform;

        ObjectContainer = Instantiate(ObjectContainer, gameObject.transform.position, Quaternion.identity);
        
        IsFull = true;

    }

    public void ReturnToSlot(bool NoSpace)
    {
        if(NoSpace == true)
        {
            // Returns its item back to the slots position if there is no space.
            ObjectContainer.transform.position = gameObject.transform.position;

        } else
        {
            RemoveItem();
        }
    }
}
*/
