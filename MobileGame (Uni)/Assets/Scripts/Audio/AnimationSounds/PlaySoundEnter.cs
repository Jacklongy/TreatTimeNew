using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundEnter : StateMachineBehaviour
{
    SoundManagerScript SoundManagerScript;
    [SerializeField] string soundName;

   [SerializeField, Range(0, 1)] float volume;

    private void Awake()
    {
        SoundManagerScript = FindObjectOfType<SoundManagerScript>();
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SoundManagerScript.Play(soundName);
    }
}
