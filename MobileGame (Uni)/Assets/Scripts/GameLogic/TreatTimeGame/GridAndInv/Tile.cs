using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool IsFull;
    public GameObject ObjectContainer;
    [SerializeField] private SpaceOutline spaceOutline;
    private bool hasWarnedMissingOutline;

    private void Awake()
    {
        EnsureSpaceOutline();
    }

    // Start is called before the first frame update
    void Start()
    {
        CheckIfFull();
        HideOutline();
    }

    public void TakeObject(GameObject Item)
    {
        ObjectContainer = Item;
        ObjectContainer.transform.position = gameObject.transform.position;
        IsFull = true;

        Item Object = ObjectContainer.GetComponent<Item>();

        // Ensure item is marked as on-grid with correct layer
        if (Object != null)
        {
            Object.OnGrid = true;
            Object.ResetAnim();
        }

        Item.layer = 7;

        GroupOutlineController controller = GroupOutlineController.Instance != null
            ? GroupOutlineController.Instance
            : FindObjectOfType<GroupOutlineController>();
        controller?.RequestRefresh();

    }

    public void RemoveObject()
    {
        ObjectContainer = null;
        IsFull = false;

        HideOutline();
        GroupOutlineController controller = GroupOutlineController.Instance != null
            ? GroupOutlineController.Instance
            : FindObjectOfType<GroupOutlineController>();
        controller?.RequestRefresh();

        Debug.Log("I Removed " + ObjectContainer);
    }

   public void CheckIfFull()
    {
        if(ObjectContainer == null)
        {
            IsFull = false;
            return;
        }

        IsFull = true;
    }

    public void ApplyOutline(bool top, bool bottom, bool left, bool right, Color color, int sortingOrder)
    {
        if (!EnsureSpaceOutline())
        {
            return;
        }

        spaceOutline.ApplyColor(color);
        spaceOutline.ApplySortingOrder(sortingOrder);
        spaceOutline.UpdateBorders(top, bottom, left, right);
    }

    public void HideOutline()
    {
        if (!EnsureSpaceOutline())
        {
            return;
        }

        spaceOutline.HideAll();
    }

    private bool EnsureSpaceOutline()
    {
        if (spaceOutline == null)
        {
            spaceOutline = GetComponentInChildren<SpaceOutline>(true);
        }

        if (spaceOutline == null)
        {
            if (!hasWarnedMissingOutline)
            {
                Debug.LogWarning($"Tile '{name}' has no SpaceOutline component in children.");
                hasWarnedMissingOutline = true;
            }

            return false;
        }

        return true;
    }
    
}
