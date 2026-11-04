namespace Arena.Combat
{
    /// <summary>
    /// Anything that can receive damage: player, destructible object.
    /// </summary>
    public interface IDamageable
    {
        void TakeDamage(DamageInfo damage);
    }
}
