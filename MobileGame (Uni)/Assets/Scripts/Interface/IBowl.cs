using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBowl
{
    void TakeItem(GameObject item);

    bool CheckFull();
}
