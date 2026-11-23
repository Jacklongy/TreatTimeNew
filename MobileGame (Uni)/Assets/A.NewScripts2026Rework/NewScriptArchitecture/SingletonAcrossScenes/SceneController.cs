using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


/// <summary>
/// Make this an object that is always present in the scene. It will handle scene transitions and loading.
/// It will also handle the transition animation and the loading screen.
/// </summary>
public class SceneController : MonoBehaviour
{

    public static SceneController Instance;

    public Animator transition;

    public float transTime = 1f;

    [SerializeField, Tooltip("Optional filled UI Image on the persistent transition canvas.")]
    private Image loadingBar;

    private bool isLoading;

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
        if (isLoading)
        {
            Debug.LogWarning("[SceneController] A scene transition is already in progress.");
            yield break;
        }

        if (transition == null)
        {
            Debug.LogError("[SceneController] Cannot load a scene without a transition animator.");
            yield break;
        }

        if (string.IsNullOrEmpty(name) || !Application.CanStreamedLevelBeLoaded(name))
        {
            Debug.LogError($"[SceneController] Scene '{name}' is not available in the build settings.");
            yield break;
        }

        isLoading = true;
        SetLoadingBar(0f);
        transition.SetBool("Loading", true);
        transition.SetTrigger("Start");

        AsyncOperation operation = SceneManager.LoadSceneAsync(name);
        if (operation == null)
        {
            Debug.LogError($"[SceneController] Could not start loading scene '{name}'.");
            transition.SetBool("Loading", false);
            isLoading = false;
            yield break;
        }

        operation.allowSceneActivation = false;
        float elapsed = 0f;
        float displayedProgress = 0f;
        while (operation.progress < 0.9f || elapsed < transTime)
        {
            elapsed += Time.deltaTime;

            // Unity reserves the last 10% for activation; ease the remaining work towards 90%.
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            float target = 0.9f * (1f - (1f - progress) * (1f - progress));
            displayedProgress = Mathf.Lerp(displayedProgress, target, 1f - Mathf.Exp(-2.5f * Time.deltaTime));
            SetLoadingBar(displayedProgress);
            yield return null;
        }

        operation.allowSceneActivation = true;
        while (!operation.isDone)
        {
            yield return null;
        }

        SetLoadingBar(1f);
        yield return null;
        transition.SetBool("Loading", false);
        isLoading = false;
    }

    private void SetLoadingBar(float progress)
    {
        if (loadingBar != null)
        {
            loadingBar.fillAmount = progress;
        }
    }
}
