using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public bool controlled { get { return _controlled; } set { _controlled = value; } }

    public PhysicObject physicObject { get { return _po; } }

    public InputMapping inputKeyboardMapping;
    public InputMapping inputPadMapping;

    public float jumpTakeoffSpeed = 8;
    public float touchJumpTimeout = 0.2f;

    protected bool _controlled = true;

    protected PhysicObject _po;

    protected SpriteRenderer _renderer;
    protected Animator _animator = null;

    protected bool _previousGrounded = false;

    protected RaycastHit2D[] _hitCache = new RaycastHit2D[16];
    protected List<Collider2D> _currentPlatformEffectors = new List<Collider2D>();

    private readonly int JUMP_BUTTON_HASH = InputMapping.NameToHash("Jump");
    private readonly int ATTACK_BUTTON_HASH = InputMapping.NameToHash("Attack");

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

    public void HandleInput()
    {
        Vector2 velocity = _po.velocity;
        Vector2 move = Vector2.zero;

        //_pushing = false;

        if (_controlled)
        {
            KeyCode jumpKeyboardKeycode = inputKeyboardMapping.GetKeyCode(JUMP_BUTTON_HASH);
            KeyCode jumpPadKeycode = inputPadMapping.GetKeyCode(JUMP_BUTTON_HASH);

            KeyCode attackKeyboardKeycode = inputKeyboardMapping.GetKeyCode(ATTACK_BUTTON_HASH);
            KeyCode attackPadKeycode = inputPadMapping.GetKeyCode(ATTACK_BUTTON_HASH);

            move.x = Input.GetAxis("Horizontal");
            move.y = Input.GetAxis("Vertical");

            if (_po.grounded)
            {
                if (Input.GetKeyDown(jumpKeyboardKeycode) || Input.GetKeyDown(jumpPadKeycode))
                {
                    bool jump = true;

                    if (jump)
                    {

                        velocity.y = jumpTakeoffSpeed;
                        _po.velocity = velocity;
                    }
                }
                else if(Input.GetKeyDown(attackKeyboardKeycode) || Input.GetKeyDown(attackPadKeycode))
                {
                    _animator.SetTrigger("attacking");
                }
            }
            else if ((Input.GetKeyUp(jumpKeyboardKeycode) || Input.GetKeyUp(jumpPadKeycode)))
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
        
        //check for mapper input
        if (Input.GetKeyDown(KeyCode.Escape) && Time.timeScale > 0)
        {
            Time.timeScale = 0.0f;
            InputMapper.SetMappingData(inputKeyboardMapping, inputPadMapping);
            SceneManager.LoadSceneAsync("inputMapper", LoadSceneMode.Additive);
        }
    }
 
    public void ReactCollision(ref List<RaycastHit2D> contacts, PhysicObject.CollisionStage stage)
    {

        if (stage == PhysicObject.CollisionStage.Before)
        {
            PreCollisionTest(ref contacts);
        }
        else
        {
            for (int i = 0; i < contacts.Count; ++i)
            {
                RaycastHit2D hit = contacts[i];
                Vector2 cntNorm = contacts[i].normal;

                if (hit.collider.usedByEffector)
                {//rely on built in effector to avoid having to code a class that containt exactly the same parameters
                    PlatformEffector2D effector = hit.collider.GetComponent<PlatformEffector2D>();
                    if (effector != null && effector.useOneWay)
                    {
                        if (HandlePassThroughPlatform(hit, effector, stage))
                        {
                            contacts.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
        }
    }

    protected void PreCollisionTest(ref List<RaycastHit2D> contacts)
    {
        //we check for all the current paltformer effetor if we are still colliding them
        for (int i = 0; i < _currentPlatformEffectors.Count; ++i)
        {
            bool contained = false; 
            for (int k = 0; k < contacts.Count; ++k)
            {
                if (contacts[k] == _currentPlatformEffectors[i])
                {
                    contained = true;
                    break;
                }
            }
            
            if (!contained)
            {
                _currentPlatformEffectors.RemoveAt(i);
                i--;
            }
        }
    }

    //return true if need to be removed
    protected bool HandlePassThroughPlatform(RaycastHit2D hit, PlatformEffector2D effector, PhysicObject.CollisionStage stage)
    {
        Vector2 cntNorm = hit.normal;

        if (stage == PhysicObject.CollisionStage.XMove)
        {
            if (!_currentPlatformEffectors.Contains(hit.collider))
                _currentPlatformEffectors.Add(hit.collider);

            return true;
        }
        else
        {
            //only test if we are currently falling AND if we weren't going through that
            //effector the last frame already. Otherwise we ignore it (as that mean we started falling
            //before going entierly through it, we would get stuck in it if we weren't ignoring it)
            if (_po.velocity.y < 0)
            {
                if (!_currentPlatformEffectors.Contains(hit.collider))
                {//if the current list of effector contain that effector, we never passed it completly, 
                    //don't collide as we would be stuck INSIDE it
                    float dot = Vector2.Dot(cntNorm, Vector2.up);
                    if (dot < Mathf.Cos(Mathf.Deg2Rad * effector.surfaceArc * 0.5f))
                    {// ignore that collision, the normal is outside the angle define
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if(!_currentPlatformEffectors.Contains(hit.collider))
                    _currentPlatformEffectors.Add(hit.collider);

                return true;
            }
        }

        return false;
    }
}