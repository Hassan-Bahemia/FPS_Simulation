using UnityEngine;

namespace Weapons
{
    public class GrapplingGun : MonoBehaviour
    {
        [SerializeField] private Vector3 m_grapplePoint;
        [SerializeField] private LayerMask m_whatIsGrappleable;
        [SerializeField] public Transform m_shootingPoint;
        [SerializeField] private Transform m_cam;
        [SerializeField] private Transform m_player;
        [SerializeField] private float m_maxDistance;
    
        [Header("Spring Settings")]
        [SerializeField] private SpringJoint m_joint;
        [SerializeField] private float m_jointSpring;
        [SerializeField] private float m_jointDamp;
        [SerializeField] private float m_jointMassScale;

        private void Update()
        {
            if (Input.GetMouseButtonDown(1)) {
                StartGrapple();
            }
            else if (Input.GetMouseButtonUp(1)) {
                StopGrapple();
            }
        }

        /// <Summary>
        /// Call Whenever we want to start a grapple
        /// </Summary>
        void StartGrapple()
        {
            RaycastHit hit;
            if (Physics.Raycast(m_cam.position, m_cam.forward, out hit, m_maxDistance, m_whatIsGrappleable)) {
                m_grapplePoint = hit.point;
                m_joint = m_player.gameObject.AddComponent<SpringJoint>();
                m_joint.autoConfigureConnectedAnchor = false;
                m_joint.connectedAnchor = m_grapplePoint;

                float distanceFromPoint = Vector3.Distance(m_player.position, m_grapplePoint);
            
                //The distance grapple will try to keep from grapple point
                m_joint.maxDistance = distanceFromPoint * 0.8f;
                m_joint.minDistance = distanceFromPoint * 0.25f;
            
                //Change these values to fit the game
                m_joint.spring = m_jointSpring;
                m_joint.damper = m_jointDamp;
                m_joint.massScale = m_jointMassScale;
            }
        }

        /// <Summary>
        /// Call whenever we want to stop a grapple
        /// </Summary>
        void StopGrapple()
        {
            Destroy(m_joint);
        }
    
        public bool IsGrappling() {
            return m_joint != null;
        }
    
        public Vector3 GetGrapplePoint() {
            return m_grapplePoint;
        }
    }
}
