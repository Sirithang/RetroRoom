using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class gather together function that most if not all enemies could need
//simple action like looking for player, checking for obstacle etc...
public class EnemyController : MonoBehaviour
{
    public bool spriteFaceLeft;

    protected PhysicObject _po;
    protected SpriteRenderer _spriteRenderer;
    protected Animator _animator;

    protected Vector2 _currentInput;
    protected Vector2 _forward;

    protected bool _collidedInMovement;

    readonly int VELOCITYX_HASH = Animator.StringToHash("velocityX");
    readonly int DEATH_HASH = Animator.StringToHash("death");

    protected virtual void Awake()
    {
        _forward = spriteFaceLeft ? Vector2.left : Vector2.right;

        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _po = GetComponent<PhysicObject>();
    }

    protected virtual void OnEnable()
    {
        _po.inputCallback += HandleInput;
        _po.collisionCallback += HandleCollision;
    }

    protected virtual void OnDisable()
    {
        _po.inputCallback -= HandleInput;
        _po.collisionCallback -= HandleCollision;
    }

    protected void HandleInput()
    {
        _po.moveInput = _currentInput;

        _animator.SetFloat(VELOCITYX_HASH, _po.velocity.x);
    }

    protected void HandleCollision(ref List<RaycastHit2D> contacts)
    {
        foreach (var hit in contacts)
        {
            if(Vector2.Dot(_forward, hit.normal) < -0.9f)
            {
                _collidedInMovement = true;
            }
        }
    }

    // public function called by the state machine

    public void Move()
    {
        _currentInput = _forward;
    }

    public void Stop()
    {
        _currentInput = Vector2.zero;
    }

    //This will flip the enemy if it find 
    public void CheckForObstacle()
    {
        if (_collidedInMovement)
        {
            Flip();
        }

        _collidedInMovement = false;
    }

    //this will flip the enemy if they are facing a cliff
    public void CheckForGround()
    {
        float nextPosition = Mathf.Abs(_po.velocity.x * Time.deltaTime);

        Vector2 testPosition = new Vector2(_po.velocity.x > 0 ? _po.collider2d.bounds.max.x : _po.collider2d.bounds.min.x, _po.collider2d.bounds.min.y);

        if (!Physics2D.Raycast(testPosition, Vector2.down, 0.2f))
        {
            Flip();
        } 
    }

    public void Flip()
    {
        _spriteRenderer.flipX = !_spriteRenderer.flipX;
        _forward.x *= -1;
    }

    public void Die()
    {
        Destroy(_po.collider2d);
        Destroy(_po);

        _animator.SetTrigger(DEATH_HASH);
    }
}
