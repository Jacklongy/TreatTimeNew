using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitTileEvent : MonoBehaviour
{
    private void PlayDropEffect()
    {
       transform.parent.GetComponent<Item>().Dropped();
    }
}
