using UnityEngine;

/// <summary>
/// Owns the live PlayerProfileData instance for the current application session. 
/// </summary>
public class PlayerProfileHolder : MonoBehaviour
{
	public static PlayerProfileHolder Instance;

	[Header("Player Data")]
	public PlayerProfileData CurrentData;

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
			CurrentData = new PlayerProfileData();
		}
	}

	/// <summary>
	/// Replaces the current data after a save service has loaded a player profile.
	/// </summary>
	public void SetData(PlayerProfileData data)
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
		CurrentData = new PlayerProfileData();
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
		}
	}
}
