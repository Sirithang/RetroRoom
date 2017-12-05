using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO : there is a lot of redundancy between that class aand player input, consolidate that?
public class PlayerData : MonoBehaviour
{
    public PlayerController playerController { get { return _playerController; } }

    public int maxLife;

    protected int _currentLife;
    protected Animator _animator;
    protected PhysicObject _physicObject;
    protected PlayerController _playerController;
    protected bool _invincible;

    readonly int HURT_HASH = Animator.StringToHash("hurt");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _physicObject = GetComponent<PhysicObject>();
        _playerController = GetComponent<PlayerController>();

        SceneLinkedSMB<PlayerData>.Initialise(_animator, this);
    }

    private void Start()
    {
        _currentLife = maxLife;
        _invincible = false;
    }

    public void Damaged(int amount, Vector2 direction)
    {
        if (_invincible)
            return;

        _invincible = true;

        _currentLife -= 1;
        _animator.SetTrigger(HURT_HASH);
        _playerController.controlled = false;
        _playerController.physicObject.moveInput = direction * 0.5f;

        Vector2 velocity = _playerController.physicObject.velocity;
        velocity.y = _playerController.jumpTakeoffSpeed;
        _playerController.physicObject.velocity = velocity;
    }

    public void SetInvincible(bool invincible)
    {
        _invincible = invincible;
    }
}