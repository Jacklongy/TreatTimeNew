using UnityEngine;

/// <summary>
/// Pool for score popups. Add to the same GameObject as ScoreTickPopupSpawner and assign the prefab.
/// </summary>
public class ScoreTickPopupPool : MonoBehaviour
{
    [SerializeField] private ScoreTickPopup prefab;
    [SerializeField] private Transform parent;
    [SerializeField, Min(0)] private int prewarmCount = 10;

    private SimplePool pool;

    private void Awake()
    {
        pool = new SimplePool(prefab.gameObject, parent != null ? parent : transform);
        pool.Prewarm(prewarmCount);
    }

    public ScoreTickPopup Get(Vector3 position, Quaternion rotation)
    {
        return (ScoreTickPopup)pool.Get(position, rotation);
    }
}
