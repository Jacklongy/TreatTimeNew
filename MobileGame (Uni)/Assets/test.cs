using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class TestKeyInput : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("S key pressed.");
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("F key pressed.");
        }
    }
}