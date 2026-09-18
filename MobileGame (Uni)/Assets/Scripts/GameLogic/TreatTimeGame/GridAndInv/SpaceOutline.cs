using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceOutline : MonoBehaviour
{
    public GameObject borderTop;
    public GameObject borderBottom;
    public GameObject borderLeft;
    public GameObject borderRight;

    private void Awake()
    {
        HideAll();
    }

    public void HideAll()
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        SetActiveSafe(borderTop, false);
        SetActiveSafe(borderBottom, false);
        SetActiveSafe(borderLeft, false);
        SetActiveSafe(borderRight, false);
    }

    public void UpdateBorders(bool top, bool bottom, bool left, bool right)
    {
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        SetActiveSafe(borderTop, top);
        SetActiveSafe(borderBottom, bottom);
        SetActiveSafe(borderLeft, left);
        SetActiveSafe(borderRight, right);
    }

    public void ApplyColor(Color color)
    {
        SetColorSafe(borderTop, color);
        SetColorSafe(borderBottom, color);
        SetColorSafe(borderLeft, color);
        SetColorSafe(borderRight, color);
    }

    public void ApplySortingOrder(int sortingOrder)
    {
        SetSortingOrderSafe(borderTop, sortingOrder);
        SetSortingOrderSafe(borderBottom, sortingOrder);
        SetSortingOrderSafe(borderLeft, sortingOrder);
        SetSortingOrderSafe(borderRight, sortingOrder);
    }

    private void SetActiveSafe(GameObject target, bool active)
    {
        if (target != null)
        {
            target.SetActive(active);
        }
    }

    private void SetColorSafe(GameObject target, Color color)
    {
        if (target == null)
        {
            return;
        }

        SpriteRenderer borderRenderer = target.GetComponent<SpriteRenderer>();
        if (borderRenderer != null)
        {
            borderRenderer.color = color;
        }
    }

    private void SetSortingOrderSafe(GameObject target, int sortingOrder)
    {
        if (target == null)
        {
            return;
        }

        SpriteRenderer borderRenderer = target.GetComponent<SpriteRenderer>();
        if (borderRenderer != null)
        {
            borderRenderer.sortingOrder = sortingOrder;
        }
    }
}
