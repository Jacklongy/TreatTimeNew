using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class coinPopUp : MonoBehaviour
{
    private Animator animator;

    private void OnBecameVisible()
    {
        animator = GetComponent<Animator>();

        animator.SetTrigger("spawned");
    }


}
