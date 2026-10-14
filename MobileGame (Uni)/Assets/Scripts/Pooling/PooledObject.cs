using UnityEngine;

/// <summary>
/// Put on (or inherit from) anything that lives in a pool. Call ReturnToPool() when finished with it.
/// Falls back to Destroy if the object was never created by a pool.
/// </summary>
public class PooledObject : MonoBehaviour
{
    private SimplePool owner;
    private bool isWaitingInPool;

    public void Bind(SimplePool pool)
    {
        owner = pool;
    }

    // Called by the pool each time it hands the object out.
    public void MarkSpawned()
    {
        isWaitingInPool = false;
        OnSpawned();
    }

    public void ReturnToPool()
    {
        if (isWaitingInPool)
        {
            return;
        }

        if (owner == null)
        {
            Destroy(gameObject);
            return;
        }

        isWaitingInPool = true;
        OnReturned();
        owner.Release(this);
    }

    /// <summary>Reset per-use state here.</summary>
    protected virtual void OnSpawned() { }

    /// <summary>Stop anything still running here.</summary>
    protected virtual void OnReturned() { }
}
