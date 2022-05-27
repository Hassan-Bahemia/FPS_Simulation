using UnityEngine;
using UnityEditor;

namespace Weapons
{
    static class WeaponStatsContextMenu
    {
        [MenuItem("Assets/Create/Weapon/Pistol", priority = 1)]
        [MenuItem("Assets/Create/Weapon/Shotgun", priority = 1)]
        static void Weapon()
        {
            var asset = ScriptableObject.CreateInstance<WeaponStats>();
            
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            path += "/New Weapon.asset";

            ProjectWindowUtil.CreateAsset(asset, path);
        }
    }
}
