using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSpawner : MonoBehaviour
{
    // Item spawner will just replace the items when they have all been placed. 
    // add random items good and bad. 
    // add a suffle button

    public GameObject BowlSlot;
    public GameObject Bowl;
    public GameObject[] ItemSlots;
    public GameObject[] ItemsToSpawn;
    GameObject itemSpawned;
    public static int slotEmpty;
    public int shuffleCount;
    public TextMeshProUGUI ShuffleDisplay;
   

    private void Start()
    {
        slotEmpty = 0;

        shuffleCount = 5;

        IItemSlot bowlSlot = BowlSlot.GetComponent<IItemSlot>();

        bowlSlot.SpawnItem(Bowl);

        slotEmpty += 1;

        foreach (var item in ItemSlots)
        {
            IItemSlot slot = item.GetComponent<IItemSlot>();

            itemSpawned = ItemsToSpawn[Random.Range(0, ItemsToSpawn.Length)];

            slot.SpawnItem(itemSpawned);

            slotEmpty += 1;

        }
    }


    private void Update()
    {
    
        if(slotEmpty == 0)
        {
            SpawnCheck();
        }

        if (shuffleCount < 0)
        {
            shuffleCount = 0;
        }

        ShuffleDisplay.text = (shuffleCount.ToString());

    }

    public void SpawnCheck()
    {

        IItemSlot bowlSlot = BowlSlot.GetComponent<IItemSlot>();

        bowlSlot.SpawnItem(Bowl);

        slotEmpty += 1;

        foreach (var item in ItemSlots)
        {
            IItemSlot slot = item.GetComponent<IItemSlot>();

            itemSpawned = ItemsToSpawn[Random.Range(0, ItemsToSpawn.Length)];

            slot.SpawnItem(itemSpawned);

            slotEmpty += 1;

        }
    }

    public void Shuffle()
    {
        shuffleCount -= 1;

        FindObjectOfType<SoundManagerScript>().Play("Shuffle");


        if (shuffleCount >= 0)
        {
            foreach (var item in ItemSlots)
            {
                IItemSlot slot = item.GetComponent<IItemSlot>();

                if (slot.CheckIfFull())
                {
                    slot.ShuffleItems();
                }
            }

            foreach (var item in ItemSlots)
            {
                IItemSlot slot = item.GetComponent<IItemSlot>();

                itemSpawned = ItemsToSpawn[Random.Range(0, ItemsToSpawn.Length)];

                slot.SpawnItem(itemSpawned);

                slotEmpty += 1;

            }
        }

      
    }

}
