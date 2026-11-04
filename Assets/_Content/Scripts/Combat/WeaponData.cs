using UnityEngine;

namespace Arena.Combat
{
    /// <summary>
    /// Settings of a weapon, shared by every launcher that uses it.
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponData_new", menuName = "Arena/Combat/Weapon Data")]
    public class WeaponData : ScriptableObject
    {
        #region FIELDS

        [Tooltip("Damage dealt by one projectile.")]
        [SerializeField, Min(0)] private int _damage = 10;

        [Tooltip("Type of the damage dealt by the projectiles.")]
        [SerializeField] private DamageType _damageType;

        [Tooltip("Time between two shots, in seconds.")]
        [SerializeField, Min(0.05f)] private float _fireInterval = 0.2f;

        [Tooltip("Ammo when the weapon is full.")]
        [SerializeField, Min(1)] private int _maxAmmo = 30;

        [Tooltip("Prefab fired by the weapon.")]
        [SerializeField] private Projectile _projectilePrefab;

        public int Damage => _damage;
        public DamageType DamageType => _damageType;
        public float FireInterval => _fireInterval;
        public int MaxAmmo => _maxAmmo;
        public Projectile ProjectilePrefab => _projectilePrefab;

        #endregion
    }
}
