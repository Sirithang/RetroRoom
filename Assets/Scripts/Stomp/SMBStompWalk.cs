using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMBStompWalk : SceneLinkedSMB<StompController>
{
    public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnSLStateNoTransitionUpdate(animator, stateInfo, layerIndex);

        _MonoBehaviour.CheckForObstacle();
        _MonoBehaviour.CheckForGround();
        _MonoBehaviour.Move();
    }
}
