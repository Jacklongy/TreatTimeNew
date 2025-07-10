using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMerge
{
    public void ImergeWith(GameObject mergeMe);

    public void Grabbed();

    public void Dropped();

    public bool GridCheck();

    public void MySlot(GameObject slot);

    public void ReturnToSlot();

    public void MergeFromInvCheck();

    public void ResetAnim();
}
