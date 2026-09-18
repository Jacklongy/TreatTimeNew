using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FpsCap : MonoBehaviour
{
     [SerializeField] private bool capAt60 = true;

    private void Awake()
    {
        Application.targetFrameRate = capAt60 ? 60 : 30;
    }
}
