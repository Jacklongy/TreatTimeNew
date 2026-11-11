using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PageAnimatationController : MonoBehaviour
{

    private Animator pageAnimator;

    // Start is called before the first frame update
    void Start()
    {
        pageAnimator = GetComponent<Animator>();
    }

    public void PageOpen()
    {
        gameObject.SetActive(true);
    }
    
    public void PageClose()
    {
         gameObject.SetActive(false);
    }
}
