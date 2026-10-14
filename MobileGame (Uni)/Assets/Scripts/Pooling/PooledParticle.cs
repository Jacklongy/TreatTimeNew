using UnityEngine;

/// <summary>
/// Added automatically by ParticlePool to pooled particle prefabs.
/// Returns itself to the pool once the particles have finished.
/// </summary>
public class PooledParticle : PooledObject
{
    private ParticleSystem[] systems;
    private float lifetime;
    private float timeLeft;
    private bool playing;

    private void Awake()
    {
        systems = GetComponentsInChildren<ParticleSystem>(true);

        for (int i = 0; i < systems.Length; i++)
        {
            ParticleSystem.MainModule main = systems[i].main;

            // The prefabs are set to Destroy when finished, which would kill the pooled copy.
            main.stopAction = ParticleSystemStopAction.None;
            lifetime = Mathf.Max(lifetime, main.duration + main.startLifetime.constantMax);
        }

        lifetime = Mathf.Max(lifetime, 0.1f);
    }

    protected override void OnSpawned()
    {
        for (int i = 0; i < systems.Length; i++)
        {
            systems[i].Clear();
            systems[i].Play();
        }

        timeLeft = lifetime;
        playing = true;
    }

    protected override void OnReturned()
    {
        playing = false;
    }

    private void Update()
    {
        if (!playing)
        {
            return;
        }

        timeLeft -= Time.deltaTime;
        if (timeLeft <= 0f)
        {
            ReturnToPool();
        }
    }
}
