using UnityEngine;

/// <summary>
/// Owns the live PlayerDataLocal instance for the current application session. 
/// </summary>
public class PlayerDataManager : MonoBehaviour
{
	public static PlayerDataManager Instance;

	[Header("Player Data")]
	public PlayerDataLocal CurrentData;

	private void Awake()
	{

		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);

		if (CurrentData == null)
		{
			CurrentData = new PlayerDataLocal();
		}
	}

	/// <summary>
	/// Replaces the current data after a save service has loaded a player profile.
	/// </summary>
	public void SetData(PlayerDataLocal data)
	{
		if (data == null)
		{
			Debug.LogWarning("Cannot assign null player data.");
			return;
		}

		CurrentData = data;
	}

	/// <summary>
	/// Restores a new default profile for testing or a new player.
	/// </summary>
	public void ResetData()
	{
		CurrentData = new PlayerDataLocal();
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}
}
