using System;
using UnityEngine;

namespace Weapons
{
    public class CustomBullet : MonoBehaviour
    {
        //Assignables
        [Header("Assignables")]
        [SerializeField] private LayerMask m_whatIsEnemies;
        [SerializeField] private PhysicMaterial m_phsyXMaterial;
        [SerializeField] private Rigidbody m_rb;
        
        [Header("Stats")]
        [Range(0f, 1f)]
        [SerializeField] private float m_bulletBounciness;
        [SerializeField] private bool m_useGravity;

        [Header("Lifetime")] 
        [SerializeField] private int m_maxCollisions;
        [SerializeField] private float m_maxLifetime;
        [SerializeField] private int m_collisions;

        // Start is called before the first frame update
        void Start()
        {
            m_rb = GetComponent<Rigidbody>();
            Setup();
        }

        private void Update()
        {
            //When to Destroy
            if(m_collisions > m_maxCollisions) DestroyBullet();
            
            //Countdown Lifetime
            m_maxLifetime -= Time.deltaTime;
            if(m_maxLifetime <= 0) DestroyBullet();
        }

        private void DestroyBullet()
        {
            Invoke("Delay", 0.05f);
        }

        private void OnCollisionEnter(Collision other)
        {
            //Don't count collisions with other bullets
            if (other.collider.CompareTag("Bullet")) return;
            
            //Count up collisions
            m_collisions++;
        }

        private void Setup()
        {
            //Create a new Physics Material
            m_phsyXMaterial = new PhysicMaterial();
            m_phsyXMaterial.bounciness = m_bulletBounciness;
            m_phsyXMaterial.frictionCombine = PhysicMaterialCombine.Minimum;
            m_phsyXMaterial.bounceCombine = PhysicMaterialCombine.Maximum;
            //Assign material to collider
            GetComponent<SphereCollider>().material = m_phsyXMaterial;
            
            //Set gravity
            m_rb.useGravity = m_useGravity;
        }

        private void Delay()
        {
            Destroy(gameObject);
        }
    }
}
