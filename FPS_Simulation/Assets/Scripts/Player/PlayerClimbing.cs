using System;
using UnityEngine;

namespace Player
{
    public class PlayerClimbing : MonoBehaviour
    {
        [Header("Player Ref")] 
        [SerializeField] private Transform m_Orientation;
        [SerializeField] private Rigidbody m_RB;
        [SerializeField] private LayerMask m_whatIsWall;
        [SerializeField] private PlayerMovement m_pm;
        [SerializeField] private PlayerCam m_pc;
        
        [Header("Climbing Settings")] 
        [SerializeField] private float m_climbSpeed;
        [SerializeField] private float m_maxClimbTime;
        [SerializeField] private float m_climbTimer;
        
        [Header("Climbing Jumping")] 
        [SerializeField] private float m_climbJumpUpForce;
        [SerializeField] private float m_climbJumpBackForce;
        [SerializeField] private KeyCode m_jumpKey = KeyCode.Space;
        [SerializeField] private int m_climbJumps;
        [SerializeField] private int m_climbJumpsLeft;

        [Header("Player Bools")] 
        [SerializeField] private bool m_Climbing;
        [SerializeField] private bool m_wallFront;
        [SerializeField] public bool m_exitingWall;
        
        [Header("Detection")] 
        [SerializeField] private float m_detectionLength;
        [SerializeField] private float m_sphereCastRadius;
        [SerializeField] private float m_maxWallLookAngle;
        [SerializeField] private float m_wallLookAngle;
        [SerializeField] private RaycastHit m_frontWallHit;
        
        [Header("Wall References")]
        [SerializeField] private Transform m_lastWall;
        [SerializeField] private Vector3 m_lastWallNormal;
        [SerializeField] private float m_minWallNormalAngleChange;
        
        [Header("Wall Exiting")]
        [SerializeField] private float m_exitWallTime;
        [SerializeField] private float m_exitWallTimer;

        private void Start()
        {
            m_RB = GetComponent<Rigidbody>();
            m_pm = GetComponent<PlayerMovement>();
            m_pc = GameObject.Find("PlayerCam").GetComponent<PlayerCam>();
            m_Orientation = GetComponentInChildren<Transform>().gameObject.transform.Find("Orientation");
        }

        private void Update()
        {
            WallCheck();
            StateMachine();

            if (m_Climbing && !m_exitingWall) {
                ClimbingMovement();
            }
        }

        private void StateMachine()
        {
            //State 1 - Climbing
            if (m_wallFront && Input.GetKey(KeyCode.W) && m_wallLookAngle < m_maxWallLookAngle && !m_exitingWall) {
                if (!m_Climbing && m_climbTimer > 0) {
                    StartClimbing();
                }
                
                //Timer
                if (m_climbTimer > 0) {
                    m_climbTimer -= Time.deltaTime;
                }
                if (m_climbTimer < 0) {
                    StopClimbing();
                }
            }
            //State 2 - Exiting Wall Climbing
            else if (m_exitingWall) {
                if (m_Climbing) {
                    StopClimbing();
                }
                
                //Timer
                if (m_exitWallTimer > 0) {
                    m_exitWallTimer -= Time.deltaTime;
                }
                if (m_exitWallTimer < 0) {
                    m_exitingWall = false;
                }
            }
            //State 3 - None
            else {
                if (m_Climbing) {
                    StopClimbing();
                } 
            }

            if (m_wallFront && Input.GetKeyDown(m_jumpKey) && m_climbJumpsLeft > 0) {
                ClimbJump();
            }
        }

        private void WallCheck()
        {
            m_wallFront = Physics.SphereCast(transform.position, m_sphereCastRadius, m_Orientation.forward, out m_frontWallHit, m_detectionLength, m_whatIsWall);
            m_wallLookAngle = Vector3.Angle(m_Orientation.forward, -m_frontWallHit.normal);

            bool newWall = m_frontWallHit.transform != m_lastWall || Mathf.Abs(Vector3.Angle(m_lastWallNormal, m_frontWallHit.normal)) > m_minWallNormalAngleChange;

            if ((m_wallFront && newWall) || m_pm.m_grounded) {
                m_climbTimer = m_maxClimbTime;
                m_climbJumpsLeft = m_climbJumps;
            }
        }

        private void StartClimbing()
        {
            m_Climbing = true;
            m_pm.m_climbing = true;

            m_lastWall = m_frontWallHit.transform;
            m_lastWallNormal = m_frontWallHit.normal;
            
            //Apply Camera Effects
            m_pc.DoFov(90f);
        }

        private void ClimbingMovement()
        {
            m_RB.velocity = new Vector3(m_RB.velocity.x, m_climbSpeed, m_RB.velocity.z);
        }

        private void StopClimbing()
        {
            m_Climbing = false;
            m_pm.m_climbing = false;
            
            //Apply Camera Effects
            m_pc.DoFov(75f);
        }

        public void ClimbJump()
        {
            m_exitingWall = true;
            m_exitWallTimer = m_exitWallTime;
            
            Vector3 forceToApply = transform.up * m_climbJumpUpForce + m_frontWallHit.normal * m_climbJumpBackForce;
            
            m_RB.velocity = new Vector3(m_RB.velocity.x, 0f, m_RB.velocity.z);
            m_RB.AddForce(forceToApply, ForceMode.Impulse);

            m_climbJumpsLeft--;
        }
    }
}
