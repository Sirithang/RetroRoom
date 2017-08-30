using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicObject : MonoBehaviour
{ 
    public delegate void InputDelegate();
    public delegate void CollisionDelegate(ref List<RaycastHit2D> contacts);

    public InputDelegate inputCallback;
    public CollisionDelegate collisionCallback;

    public float moveAcceleration = 50;
    public float airAccelerationFactor = 0.8f;
    public float maxSpeed = 7;
    public float gravityModifier = 1;

    public bool twoAxis = false;

    public bool grounded { get { return m_grounded; } }

    public Vector2 velocity { get { return m_Velocity; } set { m_Velocity = value; } }
    public Vector2 moveInput { get { return m_moveInput; } set { m_moveInput = value; } }
    public Rigidbody2D rigidbody2d {  get { return m_Rigidbody; } }

    protected Rigidbody2D m_Rigidbody = null;
    protected Collider2D m_collider = null;
    protected SpriteRenderer m_Renderer = null;

    protected Vector2 m_Velocity = Vector2.zero;
    protected Vector2 m_moveInput = Vector2.zero;
    protected bool m_grounded = false;

    RaycastHit2D[] m_hitBuffer = new RaycastHit2D[16];
    List<RaycastHit2D> m_hitBufferList = new List<RaycastHit2D>(16);
    protected ContactFilter2D m_filter = new ContactFilter2D();

    protected bool m_onLadder = false;

    protected Collider2D[] m_colliderList = null;
    protected bool[] m_colliderEnabledSave = null;

    protected Vector2 targetPosition = Vector2.zero;
    protected Vector2 correction;
    protected Vector2 groundNormal = Vector2.up;

    protected int m_usableLayer;
    protected int m_pickupLayer;

    void OnEnable()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
        m_collider = GetComponent<Collider2D>();
        m_Renderer = GetComponent<SpriteRenderer>();

        if (m_colliderList != null)
        {
            for (int i = 0; i < m_colliderList.Length; ++i)
            {
                m_colliderList[i].enabled = m_colliderEnabledSave[i];
            }
        }

        m_colliderList = GetComponentsInChildren<Collider2D>();
        if(m_colliderList != null)
            m_colliderEnabledSave = new bool[m_colliderList.Length];

    }

    private void OnDisable()
    {
        if (m_colliderList != null)
        {
            for (int i = 0; i < m_colliderList.Length; ++i)
            {
                m_colliderEnabledSave[i] = m_colliderList[i].enabled;
                m_colliderList[i].enabled = false;
            }
        }
    }

    // Use this for initialization
    void Start()
    {
        m_usableLayer = LayerMask.NameToLayer("Usable");
        m_pickupLayer = LayerMask.NameToLayer("Pickup");

        m_filter.useTriggers = false;
    }

    void Update()
    {
        m_moveInput = Vector2.zero;

        if(inputCallback != null)
            inputCallback.Invoke();

        correction = Vector2.zero;
    }

    void PlayStep()
    {

    }

    public void AddForce(Vector2 force)
    {
        m_Velocity += force;
    }

    public void SetVelocity(Vector2 velocity)
    {
        m_Velocity = velocity;
    }

    public Vector2 GetVelocity()
    {
        return m_Velocity;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (m_onLadder)
        {
            LadderMovement();
        }

        const float shell = 0.01f;

        Vector2 targetSpeed = m_moveInput * maxSpeed;
        Vector2 speedDiff = targetSpeed - Vector2.Scale(m_Velocity, new Vector2(1, twoAxis ? 1 : 0));
        Vector2 acceleration = Vector2.up * -15 * gravityModifier;

        if (speedDiff.magnitude != 0.0)
        {
            acceleration += moveAcceleration * speedDiff;

            if (!m_grounded)
            {
                acceleration.x = airAccelerationFactor * acceleration.x;
            }
        }

        Vector2 dv = acceleration * Time.deltaTime;

        if (!Mathf.Approximately(m_Velocity.x, 0) && (m_Velocity.x * dv.x < 0) && (Mathf.Abs(dv.x) > Mathf.Abs(m_Velocity.x)))
            dv.x = -m_Velocity.x;


        m_Velocity = m_Velocity + dv;

        Vector2 framedAcceleration = new Vector2(groundNormal.y, -groundNormal.x) * m_Velocity.x;

        // === TEMP FIX  : DISABLE TRIGGER COLLIDER AS THEY ARE USED IN THE CASTING

        if (m_colliderList != null)
        {
            for (int i = 0; i < m_colliderList.Length; ++i)
            {
                if (m_colliderList[i].isTrigger)
                {
                    m_colliderEnabledSave[i] = m_colliderList[i].enabled;
                    m_colliderList[i].enabled = false;
                }
            }
        }
        // ===

        m_grounded = false;


        //do the X movement first
        Vector2 xMove = framedAcceleration;
        float dist = framedAcceleration.magnitude;
        if (dist > 0.001f)
        {
            int count = m_Rigidbody.Cast(xMove.normalized, m_filter, m_hitBuffer, dist + shell);
            m_hitBufferList.Clear();
            for (int i = 0; i < count; ++i)
                m_hitBufferList.Add(m_hitBuffer[i]);

            if (collisionCallback != null)
                collisionCallback.Invoke(ref m_hitBufferList);

            for (int i = 0; i < m_hitBufferList.Count; ++i)
            {
                Vector2 cntNorm = m_hitBufferList[i].normal;

                if (cntNorm.y > 0.7f)
                {
                    m_grounded = true;
                }

                float proj = Vector2.Dot(m_Velocity, cntNorm);

                if (proj < 0)
                    m_Velocity = m_Velocity - proj * cntNorm;

                float modifDist = m_hitBufferList[i].distance - shell;
                dist = modifDist < dist ? modifDist : dist;
            }
            m_Rigidbody.position = (m_Rigidbody.position + xMove.normalized * dist);
        }


        //do the Y movement second
        Vector2 yMove = Vector2.up * m_Velocity.y;// new Vector2(0, dp.y);
        dist = yMove.magnitude; //Mathf.Abs(dp.y);
        if (dist > 0.001f)
        {
            int count = m_Rigidbody.Cast(yMove.normalized, m_filter, m_hitBuffer, dist + shell);
            m_hitBufferList.Clear();
            for (int i = 0; i < count; ++i)
                m_hitBufferList.Add(m_hitBuffer[i]);

            if (collisionCallback != null)
                collisionCallback.Invoke(ref m_hitBufferList);

            for (int i = 0; i < m_hitBufferList.Count; ++i)
            {
                Vector2 cntNorm = m_hitBufferList[i].normal;

                if (cntNorm.y > 0.7f)
                {
                    groundNormal = cntNorm;
                    m_grounded = true;
                    cntNorm.x = 0;
                }

                float proj = Vector2.Dot(m_Velocity, cntNorm);

                if (proj < 0)
                    m_Velocity = m_Velocity - proj * cntNorm;

                float modifDist = m_hitBufferList[i].distance - shell;
                dist = modifDist < dist ? modifDist : dist;
            }
        }

        // === TEMP FIX  : DISABLE TRIGGER COLLIDER AS THEY ARE USED IN THE CASTING

        if (m_colliderList != null)
        {
            for (int i = 0; i < m_colliderList.Length; ++i)
            {
                if (m_colliderList[i].isTrigger)
                {
                    m_colliderList[i].enabled = m_colliderEnabledSave[i];
                }
            }
        }
        // ===

        m_Rigidbody.position = (m_Rigidbody.position + yMove.normalized * dist);
    }

    void LadderMovement()
    {
        float speed = maxSpeed * 0.2f;

        Vector2 move = m_moveInput;
        move.x = 0;

        Vector2 targetPosition = m_Rigidbody.position + move * speed * Time.deltaTime;

        m_Rigidbody.position = targetPosition;
    }

    public Vector2 GetPosition()
    {
        return m_Rigidbody.position;
    }

    public void TeleportTo(Vector2 position, bool resetVelocity)
    {
        m_Rigidbody.position = position;
        if (resetVelocity) m_Velocity = Vector2.zero;
    }
}
