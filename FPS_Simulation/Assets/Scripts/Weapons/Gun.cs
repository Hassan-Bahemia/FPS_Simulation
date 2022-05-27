using UnityEngine;

namespace Weapons
{
    public class Gun : MonoBehaviour
    {
        //References
        [Header("Gun References")] 
        [SerializeField] private WeaponStats m_weaponStats;
        [SerializeField] private UnityEngine.Camera m_fpsCam;
        [SerializeField] private Transform m_shootingPoint;
        
        //Input
        [Header("Keybinds")] 
        [SerializeField] private KeyCode m_reloadKey = KeyCode.R;
        [SerializeField] private KeyCode m_shootKey = KeyCode.Mouse0;
        
        //Bools
        [Header("Bools")] 
        [SerializeField] private bool allowInvoke = true;

        private void Awake()
        {
            //Make sure magazine is set to full amount
            m_weaponStats.m_bulletsLeft = m_weaponStats.m_magazineSize;
            m_weaponStats.m_readyToShoot = true;
        }

        private void Update()
        {
            MyInput();
        }

        private void MyInput()
        {
            //Check if allowed to hold down button and take corresponding input
            m_weaponStats.m_shooting = m_weaponStats.m_allowButtonHold ? Input.GetKey(m_shootKey) : Input.GetKeyDown(m_shootKey);
            
            //Reloading
            if (Input.GetKeyDown(m_reloadKey) && m_weaponStats.m_bulletsLeft < m_weaponStats.m_magazineSize && !m_weaponStats.m_reloading) {
                Reload();
            }
            //Reload Automatically when trying to shoot without ammo
            if (m_weaponStats.m_readyToShoot && m_weaponStats.m_shooting && !m_weaponStats.m_reloading && m_weaponStats.m_bulletsLeft <= 0) {
                Reload();
            }
            
            //Shooting
            if (m_weaponStats.m_readyToShoot && m_weaponStats.m_shooting && !m_weaponStats.m_reloading && m_weaponStats.m_bulletsLeft > 0) {
                //Set Bullets shot to 0
                m_weaponStats.m_bulletShot = 0;

                Shoot();
            }
        }

        private void Shoot()
        {
            m_weaponStats.m_readyToShoot = false;
            
            //Find the exact hit point using a raycast
            Ray ray = m_fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)); //Just a ray through the middle of your screen
            RaycastHit hit;
            
            //Check if ray hits something
            Vector3 targetPoint;
            if (Physics.Raycast(ray, out hit)) {
                targetPoint = hit.point;
            }
            else {
                targetPoint = ray.GetPoint(1000); //Just a point far away from the player
            }
            
            //Calculate Direction from shootingPoint to targetPoint
            Vector3 directionWithoutSpread = targetPoint - m_shootingPoint.position;
            
            //Calculate Spread
            float x = Random.Range(-m_weaponStats.m_spread, m_weaponStats.m_spread);
            float y = Random.Range(-m_weaponStats.m_spread, m_weaponStats.m_spread);
            
            //Calculate new direction with spread
            Vector3 directionWithSpread = directionWithoutSpread + new Vector3(x, y, 0); //Just add spread to last direction
            
            //Instantiate Bullet
            GameObject currentBullet = Instantiate(m_weaponStats.m_weaponBullet, m_shootingPoint.position, Quaternion.identity); //Store Instantiated Bullet
            //Rotate Bullet to Shoot Direction
            currentBullet.transform.forward = directionWithSpread.normalized;
            
            //Add Forces to Bullet
            currentBullet.GetComponent<Rigidbody>().AddForce(directionWithSpread.normalized * m_weaponStats.m_weaponShootForce, ForceMode.Impulse);
            currentBullet.GetComponent<Rigidbody>().AddForce(m_fpsCam.transform.up * m_weaponStats.m_weaponUpwardForce, ForceMode.Impulse);

            m_weaponStats.m_bulletsLeft--;
            m_weaponStats.m_bulletShot++;
            
            //Invoke resetShot function (if not already invoked), with timeBetweenShooting
            if (allowInvoke) {
                Invoke("ResetShot", m_weaponStats.m_timeBetweenShooting);
                allowInvoke = false;
            }
            
            //If more than one bulletsPerTap make sure to repeat shoot function
            if (m_weaponStats.m_bulletShot < m_weaponStats.m_bulletsPerTap && m_weaponStats.m_bulletsLeft > 0) {
                Invoke("Shoot", m_weaponStats.m_timeBetweenShots);
            }
        }

        private void ResetShot()
        {
            //Allow Shooting and Invoking again
            m_weaponStats.m_readyToShoot = true;
            allowInvoke = true;
        }

        private void Reload()
        {
            m_weaponStats.m_reloading = true;
            Invoke("ReloadFinished", m_weaponStats.m_reloadTime);
        }

        private void ReloadFinished()
        {
            m_weaponStats.m_bulletsLeft = m_weaponStats.m_magazineSize;
            m_weaponStats.m_reloading = false;
        }
    }
}
