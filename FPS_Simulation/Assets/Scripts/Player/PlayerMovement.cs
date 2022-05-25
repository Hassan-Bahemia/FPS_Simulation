using System;
using System.Collections;
using UnityEngine;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Player Settings")] 
        [SerializeField] private float m_MoveSpeed;
        [SerializeField] private float m_WalkSpeed;
        [SerializeField] private float m_SprintSpeed;
        [SerializeField] private float m_SlideSpeed;
        [SerializeField] private float m_WallRunSpeed;
        [SerializeField] private float m_ClimbSpeed;
        
        [Header("Speed Settings")] 
        [SerializeField] private float m_desiredMoveSpeed;
        [SerializeField] private float m_lastDesiredMoveSpeed;
        [SerializeField] private float m_speedIncreaseMultiplier;
        [SerializeField] private float m_slopeIncreaseMultiplier;
        [SerializeField] private float m_groundDrag;

        [Header("Ground Check")] 
        [SerializeField] private float m_playerHeight;
        [SerializeField] private LayerMask m_whatIsGround;

        [Header("Player Keybinds")] 
        [SerializeField] private KeyCode m_jumpKey = KeyCode.Space;
        [SerializeField] private KeyCode m_sprintKey = KeyCode.LeftShift;
        [SerializeField] private KeyCode m_crouchKey = KeyCode.C;
        
        [Header("Player Jump Settings")] 
        [SerializeField] private float m_jumpForce;
        [SerializeField] private float m_jumpCooldown;
        [SerializeField] private float m_airMultiplier;
        [SerializeField] private bool m_readyToJump;
        
        [Header("Player Crouch Settings")] 
        [SerializeField] private float m_CrouchSpeed;
        [SerializeField] private float m_crouchYScale;
        [SerializeField] private float m_startYScale;

        
        [Header("Slope Settings")] 
        [SerializeField] private float m_maxSlopeAngle;
        [SerializeField] private RaycastHit m_slopeHit;
        [SerializeField] private bool m_exitingSlope;

        [Header("Player Ref")] 
        [SerializeField] private Transform m_Orientation;
        [SerializeField] private float m_horizontalInput;
        [SerializeField] private float m_verticalInput;
        [SerializeField] private Vector3 m_moveDirection;
        [SerializeField] private Rigidbody m_RB;
        [SerializeField] private PlayerClimbing m_playerClimbing;
        
        [Header("Player Bools")]
        [SerializeField] public bool m_sliding;
        [SerializeField] public bool m_grounded;
        [SerializeField] public bool m_crouching;
        [SerializeField] public bool m_wallRunning;
        [SerializeField] public bool m_climbing;

        [Header("Player States")]
        public MovementStates m_State;
        public enum MovementStates { Walking, Sprinting, Wallrunning, Climbing, Crouching, Sliding, Air }

        private void Start()
        {
            m_RB = GetComponent<Rigidbody>();
            m_playerClimbing = GetComponent<PlayerClimbing>();
            m_RB.freezeRotation = true;
            m_Orientation = GetComponentInChildren<Transform>().gameObject.transform.Find("Orientation");

            m_readyToJump = true;

            m_startYScale = transform.localScale.y;
        }

        private void Update()
        {
            //Ground Check
            m_grounded = Physics.Raycast(transform.position, Vector3.down, m_playerHeight * 0.5f + 0.2f, m_whatIsGround);
            
            MyInput();
            SpeedControl();
            StateHandler();
            
            //Handle drag
            if (m_grounded) {
                m_RB.drag = m_groundDrag;
            } else {
                m_RB.drag = 0;
            }
        }

        private void FixedUpdate()
        {
            MovePlayer();
        }

        private void MyInput()
        {
            m_horizontalInput = Input.GetAxisRaw("Horizontal");
            m_verticalInput = Input.GetAxisRaw("Vertical");
            
            //When to Jump
            if (Input.GetKey(m_jumpKey) && m_readyToJump && m_grounded)
            {
                m_readyToJump = false;
                
                Jump();
                
                Invoke(nameof(ResetJump), m_jumpCooldown);
            }
            
            //Start Crouch
            if (Input.GetKeyDown(m_crouchKey) && m_horizontalInput == 0 && m_verticalInput == 0) {
                transform.localScale = new Vector3(transform.localScale.x, m_crouchYScale, transform.localScale.z);
                m_RB.AddForce(Vector3.down * 5f, ForceMode.Impulse);
                
                m_crouching = true;
            }
            
            //Stop Crouch
            if (Input.GetKeyUp(m_crouchKey)) {
                transform.localScale = new Vector3(transform.localScale.x, m_startYScale, transform.localScale.z);
                
                m_crouching = false;
            }
        }

        private void StateHandler()
        {
            //Mode - Climbing
            if (m_climbing) {
                m_State = MovementStates.Climbing;
                m_desiredMoveSpeed = m_ClimbSpeed;
            }
            //Mode - Wallrunning
            else if (m_wallRunning)
            {
                m_State = MovementStates.Wallrunning;
                m_desiredMoveSpeed = m_WallRunSpeed;
            }
            //Mode - Sliding
            else if (m_sliding) {
                m_State = MovementStates.Sliding;

                if (OnSlope() && m_RB.velocity.y < 0.1f) {
                    m_desiredMoveSpeed = m_SlideSpeed;
                }
                else {
                    m_desiredMoveSpeed = m_SprintSpeed;
                }
            }
            //Mode - Crouching
            else if (m_crouching) {
                m_State = MovementStates.Crouching;
                m_desiredMoveSpeed = m_CrouchSpeed;
            }
            //Mode - Sprinting
            else if (m_grounded && Input.GetKey(m_sprintKey)) {
                m_State = MovementStates.Sprinting;
                m_desiredMoveSpeed = m_SprintSpeed;
            }
            //Mode - Walking
            else if (m_grounded) {
                m_State = MovementStates.Walking;
                m_desiredMoveSpeed = m_WalkSpeed;
            }
            //Mode - Air
            else {
                m_State = MovementStates.Air;
            }
            
            //Check if desiredMoveSpeed has changed drastically
            if (Mathf.Abs(m_desiredMoveSpeed - m_lastDesiredMoveSpeed) > 4f && m_MoveSpeed != 0) {
                StopAllCoroutines();
                StartCoroutine(SmoothlyLerpMoveSpeed());
            }
            else {
                m_MoveSpeed = m_desiredMoveSpeed;
            }

            m_lastDesiredMoveSpeed = m_desiredMoveSpeed;
        }

        private IEnumerator SmoothlyLerpMoveSpeed()
        {
            //Smoothly lerp Movement Speed to desire value
            float time = 0;
            float difference = Mathf.Abs(m_desiredMoveSpeed - m_MoveSpeed);
            float startValue = m_MoveSpeed;

            while (time < difference) {
                m_MoveSpeed = Mathf.Lerp(startValue, m_desiredMoveSpeed, time / difference);

                if (OnSlope()) {
                    float slopeAngle = Vector3.Angle(Vector3.up, m_slopeHit.normal);
                    float slopeAngleIncrease = 1 + (slopeAngle / 90f);
                    
                    time += Time.deltaTime * m_speedIncreaseMultiplier * m_slopeIncreaseMultiplier * slopeAngleIncrease;
                }
                else {
                    time += Time.deltaTime * m_speedIncreaseMultiplier;
                }

                yield return null;
            }

            m_MoveSpeed = m_desiredMoveSpeed;
        }

        private void MovePlayer()
        {
            if (m_playerClimbing.m_exitingWall) return;
            
            //Calculate Movement Direction
            m_moveDirection = m_Orientation.forward * m_verticalInput + m_Orientation.right * m_horizontalInput;
            
            //On Slope
            if (OnSlope() && !m_exitingSlope) {
                m_RB.AddForce(20f * m_MoveSpeed * GetSlopeMoveDirection(m_moveDirection), ForceMode.Force);

                if (m_RB.velocity.y > 0) {
                    m_RB.AddForce(Vector3.down * 80f, ForceMode.Force);
                }
            }
            //On Ground
            if(m_grounded)
                m_RB.AddForce(10f * m_MoveSpeed * m_moveDirection.normalized, ForceMode.Force);
            //In Air
            else if(!m_grounded)
                m_RB.AddForce(m_airMultiplier * m_MoveSpeed * 10f * m_moveDirection.normalized, ForceMode.Force);

            if(!m_wallRunning) 
                m_RB.useGravity = !OnSlope();
        }

        private void SpeedControl()
        {
            //Limiting Speed on Slope
            if (OnSlope() && !m_exitingSlope) {
                if (m_RB.velocity.magnitude > m_MoveSpeed) {
                    m_RB.velocity = m_RB.velocity.normalized * m_MoveSpeed;
                }
            }
            //Limiting Speed on ground or air
            else
            {
                Vector3 flatVel = new Vector3(m_RB.velocity.x, 0f, m_RB.velocity.z);

                //Limit Velocity if needed
                if (flatVel.magnitude > m_MoveSpeed)
                {
                    Vector3 limitedVel = flatVel.normalized * m_MoveSpeed;
                    m_RB.velocity = new Vector3(limitedVel.x, m_RB.velocity.y, limitedVel.z);
                }
            }
        }

        private void Jump()
        {
            m_exitingSlope = true;
            
            //Reset Y Velocity
            m_RB.velocity = new Vector3(m_RB.velocity.x, 0f, m_RB.velocity.z);
            
            m_RB.AddForce(transform.up * m_jumpForce, ForceMode.Impulse);
        }

        private void ResetJump()
        {
            m_readyToJump = true;

            m_exitingSlope = false;
        }

        public bool OnSlope()
        {
            if (Physics.Raycast(transform.position, Vector3.down, out m_slopeHit, m_playerHeight * 0.5f + 0.3f)) {
                float angle = Vector3.Angle(Vector3.up, m_slopeHit.normal);
                return angle < m_maxSlopeAngle && angle != 0;
            }

            return false;
        }

        public Vector3 GetSlopeMoveDirection(Vector3 direction)
        {
            return Vector3.ProjectOnPlane(direction, m_slopeHit.normal).normalized;
        }
    }
}
