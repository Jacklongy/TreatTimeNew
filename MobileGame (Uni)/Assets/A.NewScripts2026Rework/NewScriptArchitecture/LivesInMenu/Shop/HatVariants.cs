using UnityEngine;

/// <summary>Switches between the standard and dog-specific hat visuals.</summary>
public class HatVariants : MonoBehaviour
{
    [SerializeField] private GameObject normalHat;
    [SerializeField] private GameObject dogHat;

    public void Apply(string slotId)
    {
        if (normalHat == null || !normalHat.transform.IsChildOf(transform) || normalHat == gameObject)
        {
            Debug.LogError("HatVariants needs a normal hat child object.", this);
            return;
        }

        if (dogHat != null && (!dogHat.transform.IsChildOf(transform) || dogHat == gameObject ||
            dogHat.transform.IsChildOf(normalHat.transform) || normalHat.transform.IsChildOf(dogHat.transform)))
        {
            Debug.LogError("HatVariants needs separate normal and dog hat child objects.", this);
            return;
        }

        bool useDogHat = slotId == "dog" && dogHat != null;
        normalHat.SetActive(!useDogHat);
        if (dogHat != null) dogHat.SetActive(useDogHat);
    }
}
