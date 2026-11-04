using UnityEngine;

namespace Arena.Combat
{
    /// <summary>
    /// Everything a hit carries: amount, type and source. Passed to IDamageable.TakeDamage.
    /// </summary>
    public readonly struct DamageInfo
    {
        public readonly int Amount;

        public readonly DamageType Type;

        /// <summary>
        /// Object that dealt the damage: shooter, damage zone... Can be null.
        /// </summary>
        public readonly GameObject Source;

        public DamageInfo(int amount, DamageType type, GameObject source = null)
        {
            Amount = amount;
            Type = type;
            Source = source;
        }
    }
}
