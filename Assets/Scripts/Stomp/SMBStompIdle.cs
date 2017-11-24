using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SMBStompIdle : SceneLinkedSMB<StompController>
{
    public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnSLStateEnter(animator, stateInfo, layerIndex);

        _MonoBehaviour.Stop();
    }
}
