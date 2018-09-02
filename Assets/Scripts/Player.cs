using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] [Range(0.0f, 10.0f)] float m_acceleration = 1.0f;
    [SerializeField] [Range(0.0f, 10.0f)] float m_maxSpeed = 1.0f;
    [SerializeField] [Range(0.0f, 10.0f)] float m_friction = 10.0f;
    [SerializeField] [Range(0.0f, 100.0f)] float m_jumpForce = 3.0f;
    [SerializeField] [Range(0.0f, 10.0f)] float m_jumpResistance = 3.0f;
    [SerializeField] [Range(0.0f, 10.0f)] float m_fallSpeed = 3.0f;
    [SerializeField] [Range(0.0f, 1.0f)] float m_airControl = 0.5f;
    [SerializeField] [Range(0.0f, 90.0f)] float m_backflipAngle = 60.0f;
    [SerializeField] LayerMask[] m_groundMasks;
    [SerializeField] LayerMask m_platformMask = 0;
    [SerializeField] Transform m_head = null;
    [SerializeField] Transform m_feet = null;
    [SerializeField] Transform m_groundTouch = null;

    public bool OnGround { get; private set; }
    public Vector3 Velocity { get { return m_velocity; } }
    public Rigidbody2D m_rigidbody2D;

    Animator m_animator;
    CapsuleCollider2D m_collider;
    Vector3 m_velocity;
    Vector3 m_collisionSizeStart;
    Vector3 m_collisionOffsetStart;
    Vector3 m_collisionSizeCrouch;
    Vector3 m_collisionOffsetCrouch;
    LayerMask m_groundMask;

    void Start()
    {
        m_rigidbody2D = GetComponent<Rigidbody2D>();
        m_collider = GetComponent<CapsuleCollider2D>();
        m_animator = GetComponentInChildren<Animator>();
        m_collisionSizeStart = m_collider.size;
        m_collisionOffsetStart = m_collider.offset;
        m_collisionSizeCrouch = new Vector3(m_collisionSizeStart.x, 0.25f);
        m_collisionOffsetCrouch = new Vector3(m_collisionOffsetStart.x, -0.125f);
        CreateGroundMask();
    }

    void Update()
    {
        Collider2D hit = Physics2D.OverlapCircle(m_groundTouch.position, 0.05f, m_groundMask);
        OnGround = (hit != null && m_rigidbody2D.velocity.y < 0.001f) ? true : false;
        bool crouched = Input.GetButton("Crouch") && OnGround;

        if (crouched)
        {
            m_collider.size = m_collisionSizeCrouch;
            m_collider.offset = m_collisionOffsetCrouch;
        }
        else
        {
            m_collider.size = m_collisionSizeStart;
            m_collider.offset = m_collisionOffsetStart;
        }
        
        if (OnGround)
        {
            Vector3 v = Vector3.zero;
            if (!crouched)
            {
                v.x = Input.GetAxis("Horizontal");
                v *= Time.deltaTime * m_acceleration;

                m_velocity += v;
                m_velocity.x = Mathf.Clamp(m_velocity.x, -m_maxSpeed, m_maxSpeed);
                
                CheckHitWall(Mathf.Abs(m_velocity.x));
            }

            transform.position += m_velocity;

            bool inBacklfip = m_animator.GetCurrentAnimatorStateInfo(0).IsName("Backflip") || (m_animator.GetCurrentAnimatorStateInfo(0).IsName("Crouch") && !OnGround);
            Vector3 motion = (inBacklfip) ? -v : v;
            if (motion.x > 0.00001f)
                transform.localScale = new Vector3(1.0f, 1.0f);
            else if (motion.x < -0.00001f)
                transform.localScale = new Vector3(-1.0f, 1.0f);
            bool facingForward = (transform.localScale.x > 0.0f) ? true : false;

            if (Mathf.Abs(v.x) == 0.0f&& Mathf.Abs(m_velocity.x) > 0.001f)
            {
                float damp = m_friction * Time.deltaTime;
                float s1 = (m_velocity.x > 0.0f) ? -1.0f : 1.0f;
                float c = (crouched) ? 0.5f : 1.0f;
                damp *= s1 * c;
                m_velocity.x += damp;
                float s2 = (m_velocity.x > 0.0f) ? -1.0f : 1.0f;
                if (s1 != s2)
                {
                    m_velocity.x = 0.0f;
                }
            }
            else if (Mathf.Abs(m_velocity.x) <= 0.001f)
            {
                m_velocity.x = 0.0f;
            }


            if (Input.GetButtonDown("Jump"))
            {
                if (facingForward && crouched && Input.GetButton("Backward"))
                {
                    Vector3 force = Vector3.left * m_jumpForce;
                    force = Quaternion.AngleAxis(-m_backflipAngle, Vector3.forward) * force;
                    m_rigidbody2D.AddForce(force, ForceMode2D.Impulse);
                    //print("Backflip Left");
                    m_animator.SetTrigger("Backflip");
                }
                else if (!facingForward && crouched && Input.GetButton("Forward"))
                {
                    Vector3 force = Vector3.right * m_jumpForce;
                    force = Quaternion.AngleAxis(m_backflipAngle, Vector3.forward) * force;
                    m_rigidbody2D.AddForce(force, ForceMode2D.Impulse);
                    //print("Backflip Right");
                    m_animator.SetTrigger("Backflip");
                }
                else
                {
                    m_rigidbody2D.AddForce(Vector3.up * m_jumpForce, ForceMode2D.Impulse);
                    //print("Jump");
                    m_animator.SetTrigger("Jump");
                }
            }
        }
        else
        {
            Vector3 v = Vector3.zero;
            v.x = Input.GetAxis("Horizontal");
            v *= Time.deltaTime * m_acceleration * m_airControl;

            m_velocity += v;
            m_velocity.x = Mathf.Clamp(m_velocity.x, -m_maxSpeed, m_maxSpeed);
            
            CheckHitWall(Mathf.Abs(m_velocity.x));
            transform.position += m_velocity;

            bool inBacklfip = m_animator.GetCurrentAnimatorStateInfo(0).IsName("Backflip") || (m_animator.GetCurrentAnimatorStateInfo(0).IsName("Crouch") && !OnGround);
            Vector3 motion = (inBacklfip) ? -m_velocity : m_velocity;
            if (motion.x > 0.01f)
                transform.localScale = new Vector3(1.0f, 1.0f);
            else if (motion.x < -0.01f)
                transform.localScale = new Vector3(-1.0f, 1.0f);
        }

        m_animator.SetFloat("WalkSpeed", m_velocity.magnitude / m_maxSpeed);
        m_animator.SetFloat("yVelocity", m_rigidbody2D.velocity.y);
        m_animator.SetBool("Crouch", crouched);
        m_animator.SetBool("OnGround", OnGround);
    }

    private void FixedUpdate()
    {
        if (m_rigidbody2D.velocity.y >= 0.01f)
        {
            m_rigidbody2D.velocity += (Vector2.up * Physics2D.gravity) * (m_jumpResistance - 1.0f) * Time.deltaTime;
        }
        else if (m_rigidbody2D.velocity.y <= -0.01f)
        {
            m_rigidbody2D.velocity += (Vector2.up * Physics2D.gravity) * (m_fallSpeed - 1.0f) * Time.deltaTime;
        }
    }

    private void CheckHitWall(float distance)
    {
        LayerMask mask = m_groundMask & ~m_platformMask;
        float size = m_collider.size.x * 0.5f;
        RaycastHit2D rayHR = Physics2D.Raycast(m_head.position, Vector2.right, distance + size, mask);
        RaycastHit2D rayHL = Physics2D.Raycast(m_head.position, Vector2.left,  distance + size, mask);
        RaycastHit2D rayFR = Physics2D.Raycast(m_feet.position, Vector2.right, distance + size, mask);
        RaycastHit2D rayFL = Physics2D.Raycast(m_feet.position, Vector2.left, distance + size, mask);

        if (rayHR.distance != 0.0f || rayHL.distance != 0.0f || rayFR.distance != 0.0f || rayFL.distance != 0.0f)
        {
            if (rayHR.distance != 0.0f || rayFR.distance != 0.0f)
            {
                m_velocity = Vector3.left * 0.001f;
            }
            else if (rayHL.distance != 0.0f || rayFL.distance != 0.0f)
            {
                m_velocity = Vector3.right * 0.001f;
            }
        }

        // Same but for platforms
        //if (OnGround)
        //{
        //    RaycastHit2D rayHR2 = Physics2D.Raycast(m_head.position, Vector2.right, distance + size, m_platformMask);
        //    RaycastHit2D rayHL2 = Physics2D.Raycast(m_head.position, Vector2.left, distance + size, m_platformMask);
        //    RaycastHit2D rayFR2 = Physics2D.Raycast(m_feet.position, Vector2.right, distance + size, m_platformMask);
        //    RaycastHit2D rayFL2 = Physics2D.Raycast(m_feet.position, Vector2.left, distance + size, m_platformMask);

        //    if (rayHR2.distance != 0.0f || rayHL2.distance != 0.0f || rayFR2.distance != 0.0f || rayFL2.distance != 0.0f)
        //        m_velocity = Vector3.zero;
        //}
    }

    private void CreateGroundMask()
    {
        foreach (LayerMask mask in m_groundMasks)
        {
            m_groundMask |= mask;
        }
    }
}
