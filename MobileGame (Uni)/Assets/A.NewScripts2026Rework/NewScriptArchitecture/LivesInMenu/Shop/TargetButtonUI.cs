using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A side-list button that chooses which target (egg, chicken, dog) is being dressed.
/// Keeps an IsSelected bool on its Animator on until a different target is chosen.
/// </summary>
[RequireComponent(typeof(Button))]
public class TargetButtonUI : MonoBehaviour
{
    [Tooltip("Must match the HatSlot slotId and the ShopPreviewController itemName, e.g. egg.")]
    [SerializeField] private string slotId;
    [SerializeField] private ShopManager shopManager;
    [Tooltip("Optional. Bool: IsSelected.")]
    [SerializeField] private Animator animator;

    private static readonly int IsSelectedHash = Animator.StringToHash("IsSelected");

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() => shopManager.SelectTarget(slotId));
    }

    private void OnEnable()
    {
        shopManager.OnTargetChanged += Refresh;
        Refresh(shopManager.SelectedSlotId);
    }

    private void OnDisable()
    {
        shopManager.OnTargetChanged -= Refresh;
    }

    private void Refresh(string selectedSlotId)
    {
        if (animator != null) animator.SetBool(IsSelectedHash, selectedSlotId == slotId);
    }
}
