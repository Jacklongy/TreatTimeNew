using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.CloudSave; // Names spaces for cloud save. 
using Unity.Services.Core;
using System.Threading.Tasks;

public class CloudData : MonoBehaviour
{
    public static CloudData instance;

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
    }


    /// <summary>
    /// Initalize cloud saving
    /// </summary>
    public async void Start()
    {
        await UnityServices.InitializeAsync();
    }

    /// <summary>
    /// Saving the data using a key and the actual value, e.g an int. 
    /// Example: Coins - 5. 
    /// </summary>
    /// <param name="key"> Key to refer and find the data. Its name. e.g Coins </param>
    /// <param name="test"> The Value to hold. E.g Int. </param>
    public async void SaveData(string key, object test)
    {
        var data = new Dictionary<string, object> { { key, test} };

        await CloudSaveService.Instance.Data.Player.SaveAsync(data);
    }


    // When loading data i had the issue of not being able to assign the data loaded to an in game value. 
    // using Task as return type allows me to return the key value and not just the key name. 
    // it can be any data type also. you just specify it when you reference this function. 
    public async Task<T> LoadData<T>(string key)
    {
        var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { key });
        if (playerData.TryGetValue(key, out var keyName) && playerData != null)
        {
            return keyName.Value.GetAs<T>();
        }
        return default;
    }

    /// <summary>
    /// Function to show all the keys if needed.
    /// </summary>
    public async void ListKeys()
    {
        var keys = await CloudSaveService.Instance.Data.Player.ListAllKeysAsync();
        for (int i = 0; i < keys.Count; i++)
        {
            Debug.Log(keys[i].Key);
        }
    }
}
