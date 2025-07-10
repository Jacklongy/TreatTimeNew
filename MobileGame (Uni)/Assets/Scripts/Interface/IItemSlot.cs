using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IItemSlot 
{
    void TakeItem(GameObject Item);

    void RemoveItem();

    bool CheckIfFull();

    public void ShuffleItems();


    void SpawnItem(GameObject Item);

    void ReturnToSlot(bool Item);
}
