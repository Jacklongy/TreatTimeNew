using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.CloudSave; // Names spaces for cloud save. 
using Unity.Services.Core;
using System.Threading.Tasks;

public class CloudData : MonoBehaviour
{
    public static CloudData instance;

    private bool isInitialized = false;
    private TaskCompletionSource<bool> initializationTask;

    /// <summary>
    /// Make sure there is only one instance of cloud save. 
    /// </summary>
    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        initializationTask = new TaskCompletionSource<bool>();
    }


    /// <summary>
    /// Initalize cloud saving
    /// </summary>
    public async void Start()
    {
        await InitializeAsync();
    }

    /// <summary>
    /// Properly initialize cloud services and wait for completion
    /// </summary>
    private async Task InitializeAsync()
    {
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized)
            {
                Debug.Log("Initializing Unity Cloud Services...");
                await UnityServices.InitializeAsync();
                Debug.Log("Cloud Services initialized successfully.");
            }
            else
            {
                Debug.Log("Cloud Services already initialized.");
            }

            isInitialized = true;
            initializationTask.TrySetResult(true);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to initialize Cloud Services: " + e.Message);
            isInitialized = false;
            initializationTask.TrySetException(e);
        }
    }

    /// <summary>
    /// Wait for cloud services to be ready before attempting to save/load
    /// </summary>
    private async Task EnsureInitialized()
    {
        if (isInitialized)
            return;

        try
        {
            await initializationTask.Task;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Cloud initialization failed: " + e.Message);
            throw;
        }
    }

    /// <summary>
    /// Saving the data using a key and the actual value, e.g an int. 
    /// Example: Coins - 5. 
    /// </summary>
    /// <param name="key"> Key to refer and find the data. Its name. e.g Coins </param>
    /// <param name="value"> The Value to hold. E.g Int. </param>
    public async void SaveData(string key, object value)
    {
        try
        {
            await EnsureInitialized();

            var data = new Dictionary<string, object> { { key, value } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
            Debug.Log($"Cloud data saved - {key}: {value}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save cloud data ({key}): " + e.Message);
        }
    }


    // When loading data i had the issue of not being able to assign the data loaded to an in game value. 
    // using Task as return type allows me to return the key value and not just the key name. 
    // it can be any data type also. you just specify it when you reference this function. 
    public async Task<T> LoadData<T>(string key)
    {
        try
        {
            await EnsureInitialized();

            var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });
            if (playerData.TryGetValue(key, out var keyName) && playerData != null)
            {
                Debug.Log($"Cloud data loaded - {key}: {keyName.Value.GetAs<T>()}");
                return keyName.Value.GetAs<T>();
            }

            Debug.LogWarning($"Cloud data key not found: {key}");
            return default;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load cloud data ({key}): " + e.Message);
            return default;
        }
    }

    /// <summary>
    /// Function to show all the keys if needed.
    /// </summary>
    public async void ListKeys()
    {
        try
        {
            await EnsureInitialized();

            var keys = await CloudSaveService.Instance.Data.Player.ListAllKeysAsync();
            for (int i = 0; i < keys.Count; i++)
            {
                Debug.Log(keys[i].Key);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to list cloud keys: " + e.Message);
        }
    }
}
