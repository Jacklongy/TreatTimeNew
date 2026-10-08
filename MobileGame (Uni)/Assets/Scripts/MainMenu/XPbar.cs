using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Scrolls a tiling texture on a RawImage by offsetting its UV rect, so it loops seamlessly.
/// </summary>
[RequireComponent(typeof(RawImage))]
public class XPbar : MonoBehaviour
{
    [Tooltip("Tiles scrolled per second. Positive moves right.")]
    [SerializeField] private float scrollSpeed = 0.2f;

    private RawImage rawImage;

    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
    }

    private void Update()
    {
        Rect uv = rawImage.uvRect;
        // Moving the UV window left makes the image appear to move right.
        uv.x = Mathf.Repeat(uv.x - scrollSpeed * Time.deltaTime, 1f);
        rawImage.uvRect = uv;
    }
}
