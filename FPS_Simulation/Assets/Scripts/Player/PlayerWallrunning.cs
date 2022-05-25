using System;
using UnityEngine;

namespace Player
{
    public class PlayerWallrunning : MonoBehaviour
    {
        [Header("Wallrun Settings")] 
        [SerializeField] private LayerMask m_whatIsWall;
        [SerializeField] private LayerMask m_whatIsGround;
        [SerializeField] private float m_wallRunForce;
        [SerializeField] private float m_wallClimbSpeed;
        [SerializeField] private float m_wallJumpUpForce;
        [SerializeField] private float m_wallJumpSideForce;
        [SerializeField] private float m_maxWallRunTime;
        [SerializeField] private float m_wallRunTimer;
    
        [Header("Player Input")]
        [SerializeField] private float m_horizontalInput;
        [SerializeField] private float m_verticalInput;
        [SerializeField] private KeyCode m_upwardsRunKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode m_downwardsRunKey = KeyCode.LeftControl;
        [SerializeField] private KeyCode m_jumpKey = KeyCode.Space;

        [Header("Detection")] 
        [SerializeField] private float m_wallCheckDistance;
        [SerializeField] private float m_minJumpHeight;
        [SerializeField] private RaycastHit m_leftWallHit;
        [SerializeField] private RaycastHit m_rightWallHit;

        [Header("Exiting State")] 
        [SerializeField] private float m_exitWallTime;
        [SerializeField] private float m_exitWallTimer;

        [Header("Gravity Settings")] 
        [SerializeField] private bool m_useGravity;
        [SerializeField] private float m_gravityCounterForce;

        [Header("Player Refs")]
        [SerializeField] private Transform m_Orientation;
        [SerializeField] private Rigidbody m_RB;
        [SerializeField] private PlayerMovement m_pm;
        [SerializeField] private PlayerCam m_pc;
        
        [Header("Player Bools")]
        [SerializeField] private bool m_wallLeft;
        [SerializeField] private bool m_wallRight;
        [SerializeField] private bool m_upwardsRunning;
        [SerializeField] private bool m_downwardsRunning;
        [SerializeField] private bool m_exitingWall;

        private void Start()
        {
            m_RB = GetComponent<Rigidbody>();
            m_pm = GetComponent<PlayerMovement>();
            m_Orientation = GetComponentInChildren<Transform>().gameObject.transform.Find("Orientation");
            m_pc = GameObject.Find("PlayerCam").GetComponent<PlayerCam>();
        }

        private void Update()
        {
            CheckForWall();
            StateMachine();
        }

        private void FixedUpdate()
        {
            if (m_pm.m_wallRunning) {
                WallRunningMovement();
            }
        }

        private void CheckForWall()
        {
            m_wallRight = Physics.Raycast(transform.position, m_Orientation.right, out m_rightWallHit, m_wallCheckDistance, m_whatIsWall);
            m_wallLeft = Physics.Raycast(transform.position, -m_Orientation.right, out m_leftWallHit, m_wallCheckDistance, m_whatIsWall);
        }

        private bool AboveGround()
        {
            return !Physics.Raycast(transform.position, Vector3.down, m_minJumpHeight, m_whatIsGround);
        }

        private void StateMachine()
        {
            //Getting Inputs
            m_horizontalInput = Input.GetAxisRaw("Horizontal");
            m_verticalInput = Input.GetAxisRaw("Vertical");

            m_upwardsRunning = Input.GetKey(m_upwardsRunKey);
            m_downwardsRunning = Input.GetKey(m_downwardsRunKey);
            
            //State 1 - Wallrunning
            if ((m_wallLeft || m_wallRight) && m_verticalInput > 0 && AboveGround() && !m_exitingWall) {
                //Start Wallrun!!
                if (!m_pm.m_wallRunning) {
                    StartWallRun();
                }
                //Wallrun Timer
                if (m_wallRunTimer > 0) {
                    m_wallRunTimer -= Time.deltaTime;
                }
                if (m_wallRunTimer <= 0 && m_pm.m_wallRunning) {
                    m_exitingWall = true;
                    m_exitWallTimer = m_exitWallTime;
                }
                //Wall Jump
                if (Input.GetKeyDown(m_jumpKey)) {
                    WallJump();
                }
            }
            //State 2 - Exiting
            else if (m_exitingWall) {
                if (m_pm.m_wallRunning) {
                    StopWallRun();
                }
                if (m_exitWallTimer > 0) {
                    m_exitWallTimer -= Time.deltaTime;
                }
                if(m_exitWallTimer <= 0) {
                    m_exitingWall = false;
                }
            }
            //State 3 - None
            else {
                if (m_pm.m_wallRunning) {
                    StopWallRun();
                }
            }
        }

        private void StartWallRun()
        {
            m_pm.m_wallRunning = true;
            
            m_wallRunTimer = m_maxWallRunTime;
            
            m_RB.velocity = new Vector3(m_RB.velocity.x, 0f, m_RB.velocity.z);
            
            //Apply Camera Effects
            m_pc.DoFov(90f);
            if(m_wallLeft) 
                m_pc.DoTilt(-5f);
            if(m_wallRight) 
                m_pc.DoTilt(5f);
        }
        
        private void WallRunningMovement()
        {
            m_RB.useGravity = m_useGravity;

            Vector3 wallNormal = m_wallRight ? m_rightWallHit.normal : m_leftWallHit.normal;

            Vector3 wallForward = Vector3.Cross(wallNormal, transform.up);

            if ((m_Orientation.forward - wallForward).magnitude > (m_Orientation.forward - -wallForward).magnitude) {
                wallForward = -wallForward;
            }
            
            //Forward Force
            m_RB.AddForce(wallForward * m_wallRunForce, ForceMode.Force);
            
            //Upwards / Downwards force
            if (m_upwardsRunning) {
                m_RB.velocity = new Vector3(m_RB.velocity.x, m_wallClimbSpeed, m_RB.velocity.z);
            }
            if (m_downwardsRunning) {
                m_RB.velocity = new Vector3(m_RB.velocity.x, -m_wallClimbSpeed, m_RB.velocity.z);
            }
            
            //Push to Wall Force
            if (!(m_wallLeft && m_horizontalInput > 0) && !(m_wallRight && m_horizontalInput < 0)) {
                m_RB.AddForce(-wallNormal * 100, ForceMode.Force);
            }
            
            //Weaken Gravity
            if(m_useGravity)
                m_RB.AddForce(transform.up * m_gravityCounterForce, ForceMode.Force);
        }
        
        private void StopWallRun()
        {
            m_pm.m_wallRunning = false;
            
            m_pc.DoFov(75f);
            m_pc.DoTilt(0f);
        }

        private void WallJump()
        {
            //Enter Exiting Wall State
            m_exitingWall = true;
            m_exitWallTimer = m_exitWallTime;
            
            Vector3 wallNormal = m_wallRight ? m_rightWallHit.normal : m_leftWallHit.normal;

            Vector3 forceToApply = transform.up * m_wallJumpUpForce + wallNormal * m_wallJumpSideForce;
            
            //Reset Y velocity and add force
            m_RB.velocity = new Vector3(m_RB.velocity.x, 0f, m_RB.velocity.z);
            m_RB.AddForce(forceToApply, ForceMode.Impulse);
        }
    }
}
