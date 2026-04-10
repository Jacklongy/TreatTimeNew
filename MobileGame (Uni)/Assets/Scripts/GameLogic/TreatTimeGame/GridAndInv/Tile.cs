using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool IsFull;
    public GameObject ObjectContainer;

    // Start is called before the first frame update
    void Start()
    {
        IsFull = false;
    }

    public void TakeObject(GameObject Item)
    {
        ObjectContainer = Item;
        ObjectContainer.transform.position = gameObject.transform.position;
        IsFull = true;

        Item Object = ObjectContainer.GetComponent<Item>();

        // Ensure item is marked as on-grid with correct layer
        Object.OnGrid = true;
        Item.layer = 7;

        Object.ResetAnim();

       // Debug.Log("Im full with" + ObjectContainer);

    }

    public void RemoveObject()
    {
        ObjectContainer = null;
        IsFull = false;

        Debug.Log("I Removed " + ObjectContainer);
    }

   public void CheckIfFull()
    {
        if(ObjectContainer == null)
        {
            IsFull = false;
        }
        else
        {

            IsFull = true;
            Debug.Log("I contain " + ObjectContainer);
        }
    }
    
}
