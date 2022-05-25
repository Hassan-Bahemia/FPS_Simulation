using UnityEngine;

namespace Weapons
{
    public class RotateGrappleGun : MonoBehaviour
    {
        [SerializeField] private GrapplingGun m_grapplingGun;
        [SerializeField] private Quaternion m_desiredRotation;
        [SerializeField] private float m_rotationSpeed;
        
        private void Awake()
        {
            m_grapplingGun = GetComponentInChildren<GrapplingGun>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!m_grapplingGun.IsGrappling()) {
                m_desiredRotation = transform.parent.rotation;
            }
            else {
                m_desiredRotation = Quaternion.LookRotation(m_grapplingGun.GetGrapplePoint() - transform.position);
            }
            
            transform.rotation = Quaternion.Lerp(transform.rotation, m_desiredRotation, Time.deltaTime * m_rotationSpeed);
        }
    }
}
