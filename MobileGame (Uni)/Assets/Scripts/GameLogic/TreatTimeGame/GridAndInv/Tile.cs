using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool IsFull;
    public GameObject ObjectContainer;
    [SerializeField] private GameObject lockedVisual;
    public bool IsLocked { get; private set; }
    [SerializeField] private Animator tileAnimator;
    private static readonly int LockedHash = Animator.StringToHash("Locked");
    private static readonly int UnlockedHash = Animator.StringToHash("Unlocked");
    [Header("Debug")]
    [Tooltip("Tick in play mode to fire the unlock animation on this tile. Resets itself.")]
    [SerializeField] private bool debugPlayUnlockAnimation;
    [Tooltip("Seconds the locked visual stays visible after unlocking so the unlock animation can be seen.")]
    [SerializeField, Min(0f)] private float unlockVisualHideDelay = 2f;
    private Coroutine hideVisualRoutine;
    private SpaceOutline spaceOutline;
    private bool hasWarnedMissingOutline;

    private void Awake()
    {
        if (tileAnimator == null)
        {
            tileAnimator = GetComponent<Animator>();
        }

        SetLocked(false);
        EnsureSpaceOutline();
    }

    private void Update()
    {
        if (debugPlayUnlockAnimation)
        {
            debugPlayUnlockAnimation = false;
            if (tileAnimator != null)
            {
                tileAnimator.SetBool(LockedHash, false);
                tileAnimator.SetTrigger(UnlockedHash);
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        CheckIfFull();
        HideOutline();
    }

    public void TakeObject(GameObject Item)
    {
        if (Item == null || IsLocked)
        {
            return;
        }

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

    /// <summary>
    /// Sets whether this tile is available for gameplay and updates its assigned lock marker.
    /// </summary>
    public void SetLocked(bool locked)
    {
        bool wasLocked = IsLocked;
        IsLocked = locked;
        if (tileAnimator != null)
        {
            tileAnimator.SetBool(LockedHash, locked);
            // Only fire on a real locked -> unlocked transition, not the initial Awake setup.
            if (wasLocked && !locked)
            {
                tileAnimator.SetTrigger(UnlockedHash);
            }
        }

        if (hideVisualRoutine != null)
        {
            StopCoroutine(hideVisualRoutine);
            hideVisualRoutine = null;
        }

        if (lockedVisual == null || lockedVisual.activeSelf == locked)
        {
            return;
        }

        // Keep the lock visible while the unlock animation plays, otherwise it is hidden instantly.
        if (wasLocked && !locked && unlockVisualHideDelay > 0f && isActiveAndEnabled)
        {
            hideVisualRoutine = StartCoroutine(HideLockedVisualAfterDelay());
            return;
        }

        lockedVisual.SetActive(locked);
    }

    private IEnumerator HideLockedVisualAfterDelay()
    {
        yield return new WaitForSeconds(unlockVisualHideDelay);
        lockedVisual.SetActive(false);
        hideVisualRoutine = null;
    }

    public void ApplyOutline(bool top, bool bottom, bool left, bool right, Color color, int sortingOrder)
    {
        if (!EnsureSpaceOutline())
        {
            return;
        }

        spaceOutline.ApplyColor(color);
        SpriteRenderer tileRenderer = GetComponent<SpriteRenderer>();
        int tileOrder = tileRenderer != null ? tileRenderer.sortingOrder : 0;
        // Borders must always render above the tile; level keeps ordering between outline levels.
        spaceOutline.ApplySortingOrder(tileOrder + Mathf.Max(1, sortingOrder));
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
