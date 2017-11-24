using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public bool controlled { get { return _controlled; } set { _controlled = value; } }

    public PhysicObject physicObject { get { return _po; } }

    public float jumpTakeoffSpeed = 8;
    public float touchJumpTimeout = 0.2f;

    protected bool _controlled = true;

    protected PhysicObject _po;

    protected SpriteRenderer _renderer;
    protected Animator _animator = null;

    protected bool _previousGrounded = false;

    protected RaycastHit2D[] _hitCache = new RaycastHit2D[16];

    void OnEnable()
    {
        _renderer = GetComponentInChildren<SpriteRenderer>();
        _renderer.color = Color.white;

        _animator = GetComponent<Animator>();
        _po = GetComponent<PhysicObject>();

        if(_po != null)
        {
            _po.inputCallback += HandleInput;
            _po.collisionCallback += ReactCollision;
        }
    }

    void OnDisable()
    {
        if (_po != null)
        {
            _po.inputCallback -= HandleInput;
            _po.collisionCallback -= ReactCollision;
        }
    }

    void Start()
    {

    }

    void Update()
    {

    }

    public void HandleInput()
    {
        Vector2 velocity = _po.velocity;
        Vector2 move = Vector2.zero;

        //_pushing = false;

        if (_controlled)
        {

            move.x = Input.GetAxis("Horizontal");
            move.y = Input.GetAxis("Vertical");

            if (Input.GetButtonDown("Jump") && (_po.grounded))
            {
                bool jump = true;

                if (jump)
                {

                    velocity.y = jumpTakeoffSpeed;
                    _po.velocity = velocity;
                }
            }
            else if (Input.GetButtonUp("Jump"))
            {
                if (_po.velocity.y > 0)
                    _po.velocity = _po.velocity * 0.5f;
            }


            move.y = 0;

            bool needChange = _renderer.flipX ? (move.x > 0.01f) : (move.x < -0.01f);
            if (needChange)
                _renderer.flipX = !_renderer.flipX;

            _po.moveInput = move;
        }

        _animator.SetBool("grounded", _po.grounded);

        float velocityParam = Mathf.Abs(move.x);

        _animator.SetFloat("velocityX",velocityParam);
        _animator.SetFloat("velocityY", move.y);
        _animator.SetFloat("verticalSpeed", _po.velocity.y);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
       
    }
 
    public void ReactCollision(ref List<RaycastHit2D> contacts)
    {


        List<GameObject> handledObject = new List<GameObject>();

        for(int i = 0; i < contacts.Count; ++i)
        {
            if(contacts[i].collider != null && contacts[i].collider.gameObject.name != "taggedToDelete")
            {
               
            }
        }
    }
}
