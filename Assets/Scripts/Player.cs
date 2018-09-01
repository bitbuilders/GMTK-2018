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
    [SerializeField] LayerMask m_groundMask = 0;
    [SerializeField] LayerMask m_platformMask = 0;
    [SerializeField] Transform m_head = null;
    [SerializeField] Transform m_feet = null;

    public bool OnGround { get; private set; }

    Rigidbody2D m_rigidbody2D;
    CapsuleCollider2D m_collider;
    Vector3 m_velocity;

    void Start()
    {
        m_rigidbody2D = GetComponent<Rigidbody2D>();
        m_collider = GetComponent<CapsuleCollider2D>();
    }

    void Update()
    {
        Collider2D hit = Physics2D.OverlapCircle(m_feet.position, 0.15f, m_groundMask);
        OnGround = (hit != null) ? true : false;

        Vector3 v = Vector3.zero;
        v.x = Input.GetAxis("Horizontal");
        v *= Time.deltaTime * m_acceleration;

        m_velocity += v;
        m_velocity.x = Mathf.Clamp(m_velocity.x, -m_maxSpeed, m_maxSpeed);

        Vector3 dir = Vector3.right * m_velocity.x;
        CheckHitWall(Mathf.Abs(m_velocity.x));
        transform.position += m_velocity;

        if (Mathf.Abs(v.x) == 0.0f && Mathf.Abs(m_velocity.x) > 0.001f)
        {
            float damp = m_friction * Time.deltaTime;
            float s1 = (m_velocity.x > 0.0f) ? -1.0f : 1.0f;
            damp *= s1;
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
            m_rigidbody2D.AddForce(Vector3.up * m_jumpForce, ForceMode2D.Impulse);
        }
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
        float size = m_collider.size.x * 0.5f;
        RaycastHit2D rayHR = Physics2D.Raycast(m_head.position, Vector2.right, distance + size, m_groundMask);
        RaycastHit2D rayHL = Physics2D.Raycast(m_head.position, Vector2.left,  distance + size, m_groundMask);
        RaycastHit2D rayFR = Physics2D.Raycast(m_feet.position, Vector2.right, distance + size, m_groundMask);
        RaycastHit2D rayFL = Physics2D.Raycast(m_feet.position, Vector2.left,  distance + size, m_groundMask);
        
        if (rayHR.distance != 0.0f || rayHL.distance != 0.0f || rayFR.distance != 0.0f || rayFL.distance != 0.0f)
            m_velocity = Vector3.zero;

        if (OnGround)
        {
            RaycastHit2D rayHR2 = Physics2D.Raycast(m_head.position, Vector2.right, distance + size, m_platformMask);
            RaycastHit2D rayHL2 = Physics2D.Raycast(m_head.position, Vector2.left, distance + size, m_platformMask);
            RaycastHit2D rayFR2 = Physics2D.Raycast(m_feet.position, Vector2.right, distance + size, m_platformMask);
            RaycastHit2D rayFL2 = Physics2D.Raycast(m_feet.position, Vector2.left, distance + size, m_platformMask);

            if (rayHR2.distance != 0.0f || rayHL2.distance != 0.0f || rayFR2.distance != 0.0f || rayFL2.distance != 0.0f)
                m_velocity = Vector3.zero;
        }
    }
}
