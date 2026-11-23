using System;
using Unity.Services.LevelPlay;
using UnityEngine;

/// <summary>Initializes LevelPlay once and reports when it is ready.</summary>
public class Ads : MonoBehaviour
{
    [SerializeField] private string androidAppKey;
    [SerializeField] private string iosAppKey;
    [SerializeField] private bool enableTestSuite;

    public static Ads Instance { get; private set; }
    public bool IsReady { get; private set; }
    public event Action Initialized;

    private bool initializing;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            enabled = false;
            return;
        }

        if (transform.parent != null)
        {
            Debug.LogError("[LevelPlay] Put the Ads components on a separate root object.", this);
            enabled = false;
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LevelPlay.OnInitSuccess += OnInitialized;
        LevelPlay.OnInitFailed += OnInitializationFailed;
    }

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (Instance != this || initializing || IsReady) return;

#if UNITY_IOS && !UNITY_EDITOR
        string appKey = iosAppKey;
#else
        string appKey = androidAppKey;
#endif
        if (string.IsNullOrWhiteSpace(appKey))
        {
            Debug.LogError("[LevelPlay] Set the App Key for this platform.", this);
            return;
        }

        initializing = true;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (enableTestSuite) LevelPlay.SetMetaData("is_test_suite", "enable");
#endif
        LevelPlay.Init(appKey);
    }

    /// <summary>Open the LevelPlay integration suite in an editor or development build.</summary>
    public void LaunchTestSuite()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (!enableTestSuite || !IsReady)
        {
            Debug.LogWarning("[LevelPlay] Enable the test suite and wait for initialization first.", this);
            return;
        }
        LevelPlay.LaunchTestSuite();
#else
        Debug.LogWarning("[LevelPlay] The test suite is only available in development builds.", this);
#endif
    }

    private void OnInitialized(LevelPlayConfiguration configuration)
    {
        initializing = false;
        IsReady = true;
        Debug.Log("[LevelPlay] Initialization succeeded.");
        Initialized?.Invoke();
    }

    private void OnInitializationFailed(LevelPlayInitError error)
    {
        initializing = false;
        Debug.LogWarning($"[LevelPlay] Initialization failed: {error}", this);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !IsReady) Initialize();
    }

    private void OnDestroy()
    {
        if (Instance != this) return;
        LevelPlay.OnInitSuccess -= OnInitialized;
        LevelPlay.OnInitFailed -= OnInitializationFailed;
        Instance = null;
    }
}
