using UnityEngine;

namespace Weapons
{
    public class WeaponStats : ScriptableObject
    {
        //Bullet Prefab
        public GameObject m_weaponBullet;
        
        //Bullet Force
        public float m_weaponShootForce;
        public float m_weaponUpwardForce;
        
        //Weapon Stats (Float)
        public float m_timeBetweenShooting;
        public float m_spread;
        public float m_reloadTime;
        public float m_timeBetweenShots;

        //(Int)
        public int m_magazineSize;
        public int m_bulletsPerTap;
        public int m_bulletsLeft;
        public int m_bulletShot;
        
        //Bools
        public bool m_allowButtonHold;
        public bool m_shooting;
        public bool m_readyToShoot;
        public bool m_reloading;

    }
}
