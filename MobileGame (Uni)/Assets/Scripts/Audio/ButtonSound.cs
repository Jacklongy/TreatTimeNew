using UnityEngine;

public class ButtonSound : MonoBehaviour
{
    [SerializeField, Tooltip("Exact name of a sound in the splash scene's AudioManager.")]
    private string soundName;

    public void PlaySound()
    {
        if (SoundManagerScript.instance == null)
        {
            Debug.LogError("[ButtonSound] SoundManagerScript is missing; cannot play button sound.", this);
            return;
        }

        SoundManagerScript.instance.Play(soundName);
    }
}
