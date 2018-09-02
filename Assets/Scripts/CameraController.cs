using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Player m_target = null;
    [SerializeField] [Range(-20.0f, 20.0f)] float m_initialHeightOffset = 1.0f;
    [SerializeField] [Range(0.0f, 20.0f)] float m_cameraStiffness = 3.0f;
    [SerializeField] [Range(0.0f, 20.0f)] float m_followDistance = 3.0f;
    [SerializeField] Transform m_levelMIN = null;
    [SerializeField] Transform m_levelMAX = null;

    Camera m_camera;
    Vector3 m_targetPoint;
    Vector3 m_heightOffset;

    void Start()
    {
        m_camera = GetComponent<Camera>();
        m_heightOffset = Vector3.up * m_initialHeightOffset;
    }
    
    void Update()
    {

    }

    private void LateUpdate()
    {
        Vector3 dir = new Vector3(m_target.transform.position.x, m_target.transform.position.y) - new Vector3(m_targetPoint.x, m_targetPoint.y);
        if (m_target.OnGround || dir.magnitude >= m_followDistance)
        {
            Vector3 lead = m_target.Velocity * 10.0f + (Vector3.up * m_target.m_rigidbody2D.velocity.y / 4.0f);
            m_targetPoint = m_target.transform.position + lead;
            m_targetPoint.z = transform.position.z;
        }

        LimitTargetPoint();

        transform.position = Vector3.Lerp(transform.position, m_targetPoint + m_heightOffset, Time.deltaTime * m_cameraStiffness);
    }

    private void LimitTargetPoint()
    {
        float vertExtent = m_camera.orthographicSize;
        float horizExtent = m_camera.orthographicSize * Screen.width / Screen.height;

        float minY = m_levelMIN.position.y - m_heightOffset.y;
        float maxY = m_levelMAX.position.y - m_heightOffset.y;

        if (m_targetPoint.x - horizExtent < m_levelMIN.position.x)
        {
            m_targetPoint.x = m_levelMIN.position.x + horizExtent;
        }
        else if (m_targetPoint.x + horizExtent > m_levelMAX.position.x)
        {
            m_targetPoint.x = m_levelMAX.position.x - horizExtent;
        }

        if (m_targetPoint.y - vertExtent < minY)
        {
            m_targetPoint.y = minY + vertExtent;
        }
        else if (m_targetPoint.y + vertExtent > maxY)
        {
            m_targetPoint.y = maxY - vertExtent;
        }
    }
}
