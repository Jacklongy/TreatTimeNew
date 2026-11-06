using UnityEngine;

// Fade between home and the side pages, enabling interaction only when nearly fully visible.
[RequireComponent(typeof(CanvasGroup))]
public class ScrollReveal : MonoBehaviour
{
    [Tooltip("Show at home and fade out towards either side page.")]
    public bool visibleAtHome;

    CanvasGroup group;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
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

    void Apply(float scroll)
    {
        float amount = Mathf.Clamp01(Mathf.Abs(scroll));
        if (visibleAtHome)
            amount = 1f - amount;
        group.alpha = amount;
        group.interactable = amount > 0.9f;
        group.blocksRaycasts = amount > 0.9f;
    }
}
