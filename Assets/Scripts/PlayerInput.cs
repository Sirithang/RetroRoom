using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public bool controlled { get { return m_controlled; } set { m_controlled = value; } }

    public PhysicObject physicObject { get { return m_po; } }

    public float jumpTakeoffSpeed = 8;
    public float touchJumpTimeout = 0.2f;


    protected bool m_controlled = true;

    protected PhysicObject m_po;

    protected int m_usableLayer;
    protected int m_enemyLayers;
    protected int m_pickupLayer;

    protected int m_hurtHash = Animator.StringToHash("hurt");

    protected bool m_onLadder = false;

    protected SpriteRenderer m_renderer;
    protected Animator m_animator = null;

    protected bool m_invincible = false;
    protected float m_blinkingTime = 0.0f;
    protected Color m_transparent = new Color(0, 0, 0, 0);

    protected float m_pushingTimer = 0.0f;
    protected bool m_pushing = false;

    protected bool m_previousGrounded = false;

    protected RaycastHit2D[] m_hitCache = new RaycastHit2D[16];

    void OnEnable()
    {
        m_renderer = GetComponentInChildren<SpriteRenderer>();
        m_renderer.color = Color.white;

        m_animator = GetComponent<Animator>();
        m_po = GetComponent<PhysicObject>();

        PixelCamera2D.Instance.onScreenTransition += ScreenTransition;

        if(m_po != null)
        {
            m_po.inputCallback += HandleInput;
            m_po.collisionCallback += ReactCollision;
        }
    }

    void ScreenTransition(PixelCamera2D.ScreenTransitionState state)
    {
        if (state == PixelCamera2D.ScreenTransitionState.START)
            m_po.simulated = false;
        else
            m_po.simulated = true;
    }

    void OnDisable()
    {
        if (m_po != null)
        {
            m_po.inputCallback -= HandleInput;
            m_po.collisionCallback -= ReactCollision;
        }

        PixelCamera2D.Instance.onScreenTransition -= ScreenTransition;
    }

    void Start()
    {
        m_usableLayer = LayerMask.NameToLayer("Usable");
        m_enemyLayers = LayerMask.NameToLayer("Enemy");
        m_pickupLayer = LayerMask.NameToLayer("Pickup");
    }

    void Update()
    {
        if (m_pushingTimer <= 0.0f) m_pushing = false;
        else m_pushingTimer -= Time.deltaTime;

        
    }

    public void HandleInput()
    {
        Vector2 velocity = m_po.velocity;
        Vector2 move = Vector2.zero;

        //m_pushing = false;

        if (m_controlled)
        {
            
            move.x = Input.GetAxis("Horizontal");
            move.y = Input.GetAxis("Vertical");

            if (m_po.grounded)
            {

                if (move.y > 0.001f)
                {

                }

                if (m_onLadder && Mathf.Abs(move.x) > 0.01f)
                {
                  
                }
            }

            if (Input.GetButtonDown("Jump") && (m_po.grounded || m_onLadder))
            {
                bool jump = true;

                if (jump)
                {

                    velocity.y = jumpTakeoffSpeed;
                    m_po.velocity = velocity;
                }
            }
            else if(Input.GetButtonUp("Jump"))
            {
                if (m_po.velocity.y > 0)
                    m_po.velocity = m_po.velocity * 0.5f;
            }


            if (!m_onLadder)
                move.y = 0;
            else
                move.x = 0;

            bool needChange = m_renderer.flipX ? (move.x > 0.01f) : (move.x < -0.01f);
            if (needChange)
                m_renderer.flipX = !m_renderer.flipX;
        }

        m_po.moveInput = move;

        m_animator.SetBool("grounded", m_po.grounded);

        float velocityParam = Mathf.Abs(move.x);

        m_animator.SetFloat("velocityX",velocityParam);
        m_animator.SetFloat("velocityY", move.y);
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
       
    }

    IEnumerator Blinking()
    {
        if (m_blinkingTime > 0)
            yield break;

        m_invincible = true;
        m_blinkingTime = 3.0f;
        int count = 0;

        while (m_blinkingTime > 0)
        {
            yield return null;
            m_blinkingTime -= Time.deltaTime;

            count += 1;
            if (count > 10)
            {
                m_renderer.color = m_renderer.color == Color.white ? m_transparent : Color.white;
                count = 0;
            }
        }

        m_renderer.color = Color.white;
        m_invincible = false;
    }

    public void Hurt(int amount)
    {
        if (!m_invincible)
        {
            m_animator.SetTrigger(m_hurtHash);
            StartCoroutine(Blinking());
        }
    }

    public void ReactCollision(ref List<RaycastHit2D> contacts)
    {
        List<GameObject> handledObject = new List<GameObject>();

        for(int i = 0; i < contacts.Count; ++i)
        {
            if(contacts[i].collider != null && contacts[i].collider.gameObject.name != "taggedToDelete")
            {
                int layer = contacts[i].collider.gameObject.layer;

                if (layer == m_enemyLayers)
                {
                    contacts.RemoveAt(i);
                    i--;
                } 
                else
                {
                    PhysicObject obj = contacts[i].collider.GetComponent<PhysicObject>();

                    if (obj != null)
                    { 
                        if(Mathf.Abs(contacts[i].normal.x) > 0.8f)
                        {//pushing 
                            m_pushing = true;
                            m_pushingTimer = 0.2f;
                            obj.velocity = Vector2.Scale(m_po.velocity, Vector2.right);
                        }
                    }
                }
            }
        }
    }
}
