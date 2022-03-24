
using System.Collections.Generic;
using UnityEngine;

public class CloseReload_AnimBehaviour : StateMachineBehaviour
{
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("Reloading", true);
        animator.SetFloat("LeftHandIKWeightOverride", 0);
    }

    public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.SetBool("Reloading", false);
        animator.SetFloat("LeftHandIKWeightOverride", 1);
    }
}
