using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public Animator transition;

    public float transTime = 1f;

    private void Start()
    {
        gameObject.SetActive(true);
    }



    public void LoadScene(string name)
    {

        gameObject.SetActive(true);
        StartCoroutine(LoadLevel(name));
    }

   public IEnumerator LoadLevel(string name)
    {
        transition.SetTrigger("Start");

        yield return new WaitForSeconds(transTime);

        SceneManager.LoadScene(name);
    }
}
