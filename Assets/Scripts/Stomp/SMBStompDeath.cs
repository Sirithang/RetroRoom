using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMBStompDeath : SceneLinkedSMB<StompController>
{
    float countdown;
    readonly float deathTime = 2.0f;

    public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnSLStateEnter(animator, stateInfo, layerIndex);

        countdown = deathTime;
    }

    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnSLStateNoTransitionUpdate(animator, stateInfo, layerIndex);

        countdown -= Time.deltaTime;

        if (countdown <= 0)
            Destroy(animator.gameObject);
    }
}
