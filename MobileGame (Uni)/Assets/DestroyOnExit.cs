using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnExit : StateMachineBehaviour
{

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject root = animator.transform.parent != null ? animator.transform.parent.gameObject : animator.gameObject;

        // Pooled effects (e.g. score popups) return to their pool instead of being destroyed.
        IPoolableEffect poolable = root.GetComponent<IPoolableEffect>();
        if (poolable != null)
        {
            poolable.ReturnToPool();
            return;
        }

        Destroy(root);
    }
}
