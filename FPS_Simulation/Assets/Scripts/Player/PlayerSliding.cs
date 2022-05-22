using System;
using UnityEngine;

namespace Player
{
    public class PlayerSliding : MonoBehaviour
    {
        [Header("Player Refs")] 
        [SerializeField] private Transform m_Orientation;
        [SerializeField] private Transform m_PlayerObj;
        [SerializeField] private Rigidbody m_RB;
        [SerializeField] private PlayerMovement m_pm;

        [Header("Sliding Settings")] 
        [SerializeField] private float m_maxSlideTime;
        [SerializeField] private float m_slideForce;
        [SerializeField] private float m_slideTimer;
        [SerializeField] private float m_slideYScale;
        [SerializeField] private float m_startYScale;
        [SerializeField] private bool m_sliding;

        [Header("Player Keybinds")] 
        [SerializeField] private KeyCode m_slideKey = KeyCode.LeftControl;
        [SerializeField] private float m_horizontalInput;
        [SerializeField] private float m_verticalInput;

        private void Start()
        {
            m_RB = GetComponent<Rigidbody>();
            m_pm = GetComponent<PlayerMovement>();
            m_Orientation = GetComponentInChildren<Transform>().gameObject.transform.Find("Orientation");
            m_PlayerObj = GetComponentInChildren<Transform>().gameObject.transform.Find("PlayerObj");

            m_startYScale = m_PlayerObj.localScale.y;
        }

        private void Update()
        {
            m_horizontalInput = Input.GetAxisRaw("Horizontal");
            m_verticalInput = Input.GetAxisRaw("Vertical");
            
            //Start Sliding
            if (Input.GetKeyDown(m_slideKey) && (m_horizontalInput != 0 || m_verticalInput != 0) && m_pm.m_State == PlayerMovement.MovementStates.Sprinting) {
                StartSlide();
            }
            
            //Stop Sliding
            if (Input.GetKeyUp(m_slideKey) && m_sliding) {
                StopSlide();
            }
        }

        private void FixedUpdate()
        {
            if (m_sliding) {
                SlidingMovement();
            }
        }

        private void StartSlide()
        {
            m_sliding = true;
            
            m_PlayerObj.localScale = new Vector3(m_PlayerObj.localScale.x, m_slideYScale, m_PlayerObj.localScale.z);
            m_RB.AddForce(Vector3.down * 5f, ForceMode.Impulse);

            m_slideTimer = m_maxSlideTime;
        }

        private void SlidingMovement()
        {
            Vector3 inputDirection = m_Orientation.forward * m_verticalInput + m_Orientation.right * m_horizontalInput;

            //Sliding Normal
            if (!m_pm.OnSlope() || m_RB.velocity.y > -0.1f)
            {
                m_RB.AddForce(inputDirection.normalized * m_slideForce, ForceMode.Force);
                m_slideTimer -= Time.deltaTime;
            }
            //Sliding down a slope
            else {
                m_RB.AddForce(m_pm.GetSlopeMoveDirection(inputDirection) * m_slideForce, ForceMode.Force);
            }

            if (m_slideTimer <= 0) {
                StopSlide();
            }
        }

        private void StopSlide()
        {
            m_sliding = false;
            
            m_PlayerObj.localScale = new Vector3(m_PlayerObj.localScale.x, m_startYScale, m_PlayerObj.localScale.z);
        }
    }
}
