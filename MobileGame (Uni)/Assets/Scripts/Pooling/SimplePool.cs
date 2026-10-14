using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A plain stack of switched-off copies of one prefab.
/// Get() reuses a copy if one is waiting, otherwise makes a new one. Release() switches it off and puts it back.
/// </summary>
public class SimplePool
{
    private readonly GameObject prefab;
    private readonly Transform parent;
    private readonly System.Type componentIfMissing;
    private readonly Stack<PooledObject> waiting = new Stack<PooledObject>();

    public SimplePool(GameObject prefab, Transform parent, System.Type componentIfMissing = null)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.componentIfMissing = componentIfMissing ?? typeof(PooledObject);
    }

    public PooledObject Get(Vector3 position, Quaternion rotation)
    {
        PooledObject obj = null;

        // Skip entries destroyed behind our back (e.g. scene unload).
        while (obj == null && waiting.Count > 0)
        {
            obj = waiting.Pop();
        }

        if (obj == null)
        {
            obj = Create();
        }

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.gameObject.SetActive(true);
        obj.MarkSpawned();
        return obj;
    }

    public void Release(PooledObject obj)
    {
        obj.gameObject.SetActive(false);
        waiting.Push(obj);
    }

    public void Prewarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Create().ReturnToPool();
        }
    }

    private PooledObject Create()
    {
        GameObject instance = Object.Instantiate(prefab, parent);

        PooledObject obj = instance.GetComponent<PooledObject>();
        if (obj == null)
        {
            obj = (PooledObject)instance.AddComponent(componentIfMissing);
        }

        obj.Bind(this);
        return obj;
    }
}
