using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StompController : EnemyController, PhysicObject.ICollisionReceiver
{
    protected override void Awake()
    {
        base.Awake();

        SceneLinkedSMB<StompController>.Initialise(_animator, this);
    }

    public void ReceiveContact(PhysicObject obj, RaycastHit2D hit)
    {
        //hit from top, we DIE
        if(hit.normal.y > 0.8f)
        {
            Die();

            //TODO : find a better way of detecting player
            PlayerInput playerInput = obj.GetComponent<PlayerInput>();
            if(playerInput != null)
            {
                Vector2 velocity = obj.velocity;
                velocity.y = playerInput.jumpTakeoffSpeed * 0.5f;
                obj.SetVelocity(velocity);
            }
        }
        else if(Mathf.Abs(hit.normal.x) >0.9f)
        {
            PlayerData data = obj.GetComponent<PlayerData>();
            if(data != null)
                data.Damaged(1, hit.normal);
        }
    }
}
