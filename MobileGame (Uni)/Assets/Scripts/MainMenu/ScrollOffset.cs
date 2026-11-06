using UnityEngine;

// Put on anything that should slide with the page scroll (world layers, UI groups, rocks).
// speed 1 = moves one page width per page, <1 looks far away, >1 looks close to the camera.
// Position the object where it should be when scroll = 0; the offset is applied on top of that.
public class ScrollOffset : MonoBehaviour
{
    public float speed = 1f;

    [Tooltip("World objects only: leave 0 to use the camera's visible width.")]
    public float worldPageWidth;

    RectTransform rect;
    Canvas rootCanvas;
    Vector3 startPos;
    Camera cam;

    void Awake()
    {
        rect = transform as RectTransform;
        if (rect != null)
        {
            startPos = rect.anchoredPosition;
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null) rootCanvas = canvas.rootCanvas;
        }
        else
        {
            startPos = transform.localPosition;
            cam = Camera.main;
        }
    }

    void OnEnable()
    {
        PageScroller.OnScroll += Apply;
        Apply(PageScroller.Scroll);
    }

    void OnDisable()
    {
        PageScroller.OnScroll -= Apply;
    }

    float PageWidth()
    {
        if (rect != null)
            return rootCanvas != null ? ((RectTransform)rootCanvas.transform).rect.width : Screen.width;

        if (worldPageWidth > 0f) return worldPageWidth;
        if (cam == null) cam = Camera.main;
        return cam != null ? 2f * cam.orthographicSize * cam.aspect : 1f;
    }

    void Apply(float scroll)
    {
        float offset = -scroll * PageWidth() * speed;

        if (rect != null)
            rect.anchoredPosition = new Vector2(startPos.x + offset, startPos.y);
        else
            transform.localPosition = new Vector3(startPos.x + offset, startPos.y, startPos.z);
    }
}
