using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

// Put on a full-screen transparent Image (the "DragCatcher"). Owns the scroll value:
// -1 = shop page, 0 = home, +1 = tasks page. Everything else just listens.
public class PageScroller : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static float Scroll { get; private set; }
    public static event Action<float> OnScroll;

    [Tooltip("Fraction of a screen width you must drag to move to the next page.")]
    public float swipeThreshold = 0.2f;
    public float snapDuration = 0.35f;

    int currentPage;
    int pageAtDragStart;
    Coroutine snap;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Scroll = 0f;
        OnScroll = null;
    }

    public int CurrentPage => currentPage;

    void OnEnable()
    {
        currentPage = Mathf.RoundToInt(Scroll);
    }

    public void OnBeginDrag(PointerEventData data)
    {
        StopSnap();
        pageAtDragStart = currentPage;
    }

    public void OnDrag(PointerEventData data)
    {
        // Finger moves right -> content moves right -> scroll goes towards the shop (-1).
        SetScroll(Scroll - data.delta.x / Screen.width);
    }

    public void OnEndDrag(PointerEventData data)
    {
        float moved = Scroll - pageAtDragStart;
        int target = pageAtDragStart;
        if (Mathf.Abs(moved) >= swipeThreshold)
            target = pageAtDragStart + (moved > 0 ? 1 : -1);

        GoToPage(target);
    }

    // Hook this to rock buttons: -1 shop, 0 home, 1 tasks.
    public void GoToPage(int page)
    {
        currentPage = Mathf.Clamp(page, -1, 1);
        StopSnap();
        snap = StartCoroutine(SnapTo(currentPage));
    }

    IEnumerator SnapTo(float target)
    {
        float start = Scroll;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.01f, snapDuration);
            SetScroll(Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t)));
            yield return null;
        }
        SetScroll(target);
        snap = null;
    }

    void StopSnap()
    {
        if (snap != null)
        {
            StopCoroutine(snap);
            snap = null;
        }
    }

    static void SetScroll(float value)
    {
        Scroll = Mathf.Clamp(value, -1f, 1f);
        OnScroll?.Invoke(Scroll);
    }
}
