using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


/// <summary>
/// Make this an object that is always present in the scene. It will handle scene transitions and loading.
/// It will also handle the transition animation and the loading screen.
/// </summary>
public class SceneController : MonoBehaviour
{

    public static SceneController Instance;

    public Animator transition;

    public float transTime = 1f;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    } 
    
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
