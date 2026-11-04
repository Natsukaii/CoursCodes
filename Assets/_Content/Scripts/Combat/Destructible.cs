using System;
using UnityEngine;

namespace Arena.Combat
{
    /// <summary>
    /// Object destroyed when its health points reach zero: crate, barrel, turret...
    /// Raises OnDestroyed, spawns an optional prefab (debris, explosion with an instant DamageZone), then disappears.
    /// </summary>
    public class Destructible : MonoBehaviour, IDamageable
    {
        [System.Serializable]
        private class Settings
        {
            [Tooltip("Optional prefab spawned in place of the object: debris, explosion...")]
            public GameObject SpawnOnDestroyed;
        }

        #region FIELDS

        [SerializeField] private Settings _settings;
        [SerializeField] private Health _health;

        /// <summary>
        /// Raised once, when the object is destroyed by damage.
        /// </summary>
        public event Action<Destructible> OnDestroyed;

        public Health Health => _health;

        #endregion

        #region UNITY LIFECYCLES

        void Awake()
        {
            _health.Initialize();
            _health.OnDied += HandleDied;
        }

        void OnDestroy()
        {
            _health.OnDied -= HandleDied;
        }

        #endregion

        #region PUBLIC METHODS

        public void TakeDamage(DamageInfo damage)
        {
            _health.TakeDamage(damage);
        }

        #endregion

        #region PRIVATE METHODS

        private void HandleDied()
        {
            OnDestroyed?.Invoke(this);

            if (_settings.SpawnOnDestroyed)
                Instantiate(_settings.SpawnOnDestroyed, transform.position, transform.rotation);

            Destroy(gameObject);
        }

#if UNITY_EDITOR
        [ContextMenu("Debug/Take 25 Physical Damage")]
        private void DebugTakeDamage()
        {
            TakeDamage(new DamageInfo(25, DamageType.Physical, gameObject));
        }
#endif

        #endregion
    }
}
