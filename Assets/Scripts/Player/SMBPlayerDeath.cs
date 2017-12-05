using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMBPlayerDeath : SceneLinkedSMB<PlayerData>
{
    public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnSLStateEnter(animator, stateInfo, layerIndex);
    }

    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnSLStateNoTransitionUpdate(animator, stateInfo, layerIndex);

        if (_MonoBehaviour.playerController.physicObject.grounded)
            animator.SetTrigger("hurt");
    }

    public override void OnSLStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnSLStateExit(animator, stateInfo, layerIndex);

        _MonoBehaviour.SetInvincible(false);
        _MonoBehaviour.playerController.controlled = true;
    }
}
