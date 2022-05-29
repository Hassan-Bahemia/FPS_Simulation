using UnityEngine;

namespace Weapons
{
    public class Spring {
        private float m_strength;
        private float m_damper;
        private float m_target;
        private float m_velocity;
        private float m_value;
 
        public void Update(float deltaTime) {
            var direction = m_target - m_value >= 0 ? 1f : -1f;
            var force = Mathf.Abs(m_target - m_value) * m_strength;
            m_velocity += (force * direction - m_velocity * m_damper) * deltaTime;
            m_value += m_velocity * deltaTime;
        }
 
        public void Reset() {
            m_velocity = 0f;
            m_value = 0f;
        }
        
        public void SetValue(float m_value) {
            this.m_value = m_value;
        }
        
        public void SetTarget(float m_target) {
            this.m_target = m_target;
        }
 
        public void SetDamper(float m_damper) {
            this.m_damper = m_damper;
        }
        
        public void SetStrength(float m_strength) {
            this.m_strength = m_strength;
        }
 
        public void SetVelocity(float m_velocity) {
            this.m_velocity = m_velocity;
        }
        
        public float Value => m_value;
    }
}