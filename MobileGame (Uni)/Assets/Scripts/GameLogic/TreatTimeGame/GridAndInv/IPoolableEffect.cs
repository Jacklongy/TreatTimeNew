/// <summary>
/// Implemented by pooled visual effects that should be returned to their pool
/// instead of destroyed when their animation finishes (see DestroyOnExit).
/// </summary>
public interface IPoolableEffect
{
    void ReturnToPool();
}
