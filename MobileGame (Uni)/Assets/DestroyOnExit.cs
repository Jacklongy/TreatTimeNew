using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnExit : StateMachineBehaviour
{

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Pooled effects (e.g. score popups) return to their pool instead of being destroyed.
        // Searched from the animator upward because pooled instances live under a parent (the spawner).
        PooledObject pooled = animator.GetComponentInParent<PooledObject>();
        if (pooled != null)
        {
            pooled.ReturnToPool();
            return;
        }

        GameObject root = animator.transform.parent != null ? animator.transform.parent.gameObject : animator.gameObject;
        Destroy(root);
    }
}
