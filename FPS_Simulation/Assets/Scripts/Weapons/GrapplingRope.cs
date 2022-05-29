using UnityEngine;

namespace Weapons
{
    public class GrapplingRope : MonoBehaviour
    {
    
        [Header("Rope Refs")]
        [SerializeField] private LineRenderer m_lr;
        [SerializeField] private Vector3 m_currentGrapplePosition;
        [SerializeField] private GrapplingGun m_grappleGun;
        [SerializeField] private Spring m_spring;
        
        [Header("Rope Settings")]
        [SerializeField] private int m_ropeQuality;
        [SerializeField] private float m_damper;
        [SerializeField] private float m_strength;
        [SerializeField] private float m_velocity;
        [SerializeField] private float m_waveCount;
        [SerializeField] private float m_waveHeight;
        [SerializeField] private AnimationCurve m_ropeAffectCurve;
        
        private void Awake()
        {
            m_lr = GetComponent<LineRenderer>();
            m_spring = new Spring();
            m_spring.SetTarget(0);
        }

        //Called after Update
        private void LateUpdate()
        {
            DrawRope();
        }
    
        void DrawRope()
        {
            //If not grappling, don't draw rope
            if (!m_grappleGun.IsGrappling()) {
                m_currentGrapplePosition = m_grappleGun.m_shootingPoint.position;
                m_spring.Reset();
                if (m_lr.positionCount > 0) {
                    m_lr.positionCount = 0;
                }
                
                return;
            }

            if (m_lr.positionCount == 0) {
                m_spring.SetVelocity(m_velocity);
                m_lr.positionCount = m_ropeQuality + 1;
            }
            
            m_spring.SetDamper(m_damper);
            m_spring.SetStrength(m_strength);
            m_spring.Update(Time.deltaTime);

            var grapplePoint = m_grappleGun.GetGrapplePoint();
            var gunTipPosition = m_grappleGun.m_shootingPoint.position;
            var up = Quaternion.LookRotation((grapplePoint - gunTipPosition).normalized) * Vector3.up;

            m_currentGrapplePosition = Vector3.Lerp(m_currentGrapplePosition, grapplePoint, Time.deltaTime * 12f);

            for (int i = 0; i < m_ropeQuality + 1; i++) {
                var delta = i / (float) m_ropeQuality;
                var offset = m_ropeAffectCurve.Evaluate(delta) * m_waveHeight * Mathf.Sin(delta * m_waveCount * Mathf.PI) * m_spring.Value * up;
                
                m_lr.SetPosition(i, Vector3.Lerp(gunTipPosition, m_currentGrapplePosition, delta) + offset);
            }
        }
    }
}
