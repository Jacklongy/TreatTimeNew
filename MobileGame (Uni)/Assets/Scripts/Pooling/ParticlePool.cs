using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// One scene object that pools every particle prefab passed to Play(), keeping a separate pool per prefab.
/// </summary>
public class ParticlePool : MonoBehaviour
{
    public static ParticlePool Instance { get; private set; }

    private readonly Dictionary<GameObject, SimplePool> pools = new Dictionary<GameObject, SimplePool>();

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Play(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            return;
        }

        if (!pools.TryGetValue(prefab, out SimplePool pool))
        {
            pool = new SimplePool(prefab, transform, typeof(PooledParticle));
            pools.Add(prefab, pool);
        }

        pool.Get(position, rotation);
    }
}
