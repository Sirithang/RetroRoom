using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicObject : MonoBehaviour
{ 
    //Any monobehaviour implementing that interface will get a callback when a physic object collide with it.
    public interface ICollisionReceiver
    {
        //careful, rememebr the hit received is from THE OTHER OBJECT perspective (normal will be other way you expect them)
        void ReceiveContact(PhysicObject origin, RaycastHit2D hit);
    }

    public delegate void InputDelegate();
    public delegate void CollisionDelegate(ref List<RaycastHit2D> contacts);

    public InputDelegate inputCallback;
    public CollisionDelegate collisionCallback;

    public float moveAcceleration = 50;
    public float airAccelerationFactor = 0.8f;
    public float maxSpeed = 7;
    public float gravityModifier = 1;

    public bool grounded { get { return _grounded; } }

    public Vector2 velocity { get { return _Velocity; } set { _Velocity = value; } }
    public Vector2 moveInput { get { return _moveInput; } set { _moveInput = value; } }
    public Rigidbody2D rigidbody2d {  get { return _Rigidbody; } }
    public Collider2D collider2d { get { return _collider; } }

    protected Rigidbody2D _Rigidbody = null;
    protected Collider2D _collider = null;
    protected SpriteRenderer _Renderer = null;

    protected Vector2 _Velocity = Vector2.zero;
    protected Vector2 _moveInput = Vector2.zero;
    protected bool _grounded = false;

    RaycastHit2D[] _hitBuffer = new RaycastHit2D[16];
    List<RaycastHit2D> _hitBufferList = new List<RaycastHit2D>(16);
    protected ContactFilter2D _filter = new ContactFilter2D();

    protected Vector2 targetPosition = Vector2.zero;
    protected Vector2 correction;
    protected Vector2 groundNormal = Vector2.up;

    protected int _usableLayer;
    protected int _pickupLayer;

    void OnEnable()
    {
        _Rigidbody = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
        _Renderer = GetComponent<SpriteRenderer>();
    }

    private void OnDisable()
    {

    }

    // Use this for initialization
    void Start()
    {
        _usableLayer = LayerMask.NameToLayer("Usable");
        _pickupLayer = LayerMask.NameToLayer("Pickup");

        _filter.useTriggers = false;
    }

    void Update()
    {
        _moveInput = Vector2.zero;

        if(inputCallback != null)
            inputCallback.Invoke();

        correction = Vector2.zero;
    }

    void PlayStep()
    {

    }

    public void AddForce(Vector2 force)
    {
        _Velocity += force;
    }

    public void SetVelocity(Vector2 velocity)
    {
        _Velocity = velocity;
    }

    public Vector2 GetVelocity()
    {
        return _Velocity;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        const float shell = 0.005f;

        Vector2 targetSpeed = _moveInput * maxSpeed;
        Vector2 speedDiff = targetSpeed - Vector2.Scale(_Velocity, new Vector2(1, 0));
        Vector2 acceleration = Vector2.up * LevelSettings.Instance.gravity * gravityModifier * Time.deltaTime;

        if (speedDiff.magnitude != 0.0)
        {
            acceleration += moveAcceleration * speedDiff * Time.deltaTime;

            if (!_grounded)
            {
                acceleration.x = airAccelerationFactor * acceleration.x;
            }
        }

        Vector2 dv = acceleration;

        if (Mathf.Abs(dv.x) > Mathf.Abs(speedDiff.x))
            dv.x = Mathf.Sign(dv.x) * Mathf.Abs(speedDiff.x);

        _Velocity = _Velocity + dv;

        Vector2 framedAcceleration = new Vector2(groundNormal.y, -groundNormal.x) * _Velocity.x * Time.deltaTime;

        _grounded = false;


        //do the X movement first
        Vector2 xMove = framedAcceleration;
        float dist = framedAcceleration.magnitude;
        if (dist > 0.001f)
        {
            int count = _Rigidbody.Cast(xMove.normalized, _filter, _hitBuffer, dist + shell);
            _hitBufferList.Clear();
            for (int i = 0; i < count; ++i)
                _hitBufferList.Add(_hitBuffer[i]);

            if (collisionCallback != null)
                collisionCallback.Invoke(ref _hitBufferList);

            for (int i = 0; i < _hitBufferList.Count; ++i)
            {
                ICollisionReceiver physObj = _hitBufferList[i].collider.GetComponent<ICollisionReceiver>();
                if (physObj != null)
                    physObj.ReceiveContact(this, _hitBufferList[i]);

                Vector2 cntNorm = _hitBufferList[i].normal;

                if (cntNorm.y > 0.7f)
                {
                    _grounded = true;
                }

                float proj = Vector2.Dot(_Velocity, cntNorm);

                if (proj < 0)
                    _Velocity = _Velocity - proj * cntNorm;

                float modifDist = _hitBufferList[i].distance - shell;
                dist = modifDist < dist ? modifDist : dist;
            }
            _Rigidbody.position = (_Rigidbody.position + xMove.normalized * dist);
        }


        //do the Y movement second
        Vector2 yMove = Vector2.up * _Velocity.y * Time.deltaTime;// new Vector2(0, dp.y);
        dist = yMove.magnitude; //Mathf.Abs(dp.y);
        if (dist > 0.001f)
        {
            int count = _Rigidbody.Cast(yMove.normalized, _filter, _hitBuffer, dist + shell);
            _hitBufferList.Clear();
            for (int i = 0; i < count; ++i)
                _hitBufferList.Add(_hitBuffer[i]);

            if (collisionCallback != null)
                collisionCallback.Invoke(ref _hitBufferList);

            for (int i = 0; i < _hitBufferList.Count; ++i)
            {
                ICollisionReceiver physObj = _hitBufferList[i].collider.GetComponent<ICollisionReceiver>();
                if (physObj != null)
                    physObj.ReceiveContact(this, _hitBufferList[i]);

                Vector2 cntNorm = _hitBufferList[i].normal;

                if (cntNorm.y > 0.7f)
                {
                    groundNormal = cntNorm;
                    _grounded = true;
                    cntNorm.x = 0;
                }

                float proj = Vector2.Dot(_Velocity, cntNorm);

                if (proj < 0)
                    _Velocity = _Velocity - proj * cntNorm;

                float modifDist = _hitBufferList[i].distance - shell;
                dist = modifDist < dist ? modifDist : dist;
            }
        }

        _Rigidbody.position = (_Rigidbody.position + yMove.normalized * dist);
    }

    void LadderMovement()
    {
        float speed = maxSpeed * 0.2f;

        Vector2 move = _moveInput;
        move.x = 0;

        Vector2 targetPosition = _Rigidbody.position + move * speed * Time.deltaTime;

        _Rigidbody.position = targetPosition;
    }

    public Vector2 GetPosition()
    {
        return _Rigidbody.position;
    }

    public void TeleportTo(Vector2 position, bool resetVelocity)
    {
        _Rigidbody.position = position;
        if (resetVelocity) _Velocity = Vector2.zero;
    }
}
