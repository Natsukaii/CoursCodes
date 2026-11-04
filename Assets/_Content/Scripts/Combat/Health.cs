using System;
using UnityEngine;

namespace Arena.Combat
{
    /// <summary>
    /// Health points of an entity. Serialized inside the component that owns it (Player, Destructible),
    /// which calls Initialize in its Awake.
    /// </summary>
    [System.Serializable]
    public class Health
    {
        #region FIELDS

        [Tooltip("Health points when full.")]
        [Min(1)] public int Max = 100;

        [Tooltip("Current health points, visible for debugging.")]
        [SerializeField] private int _current;

        /// <summary>
        /// Raised with the current and max health points each time they change.
        /// </summary>
        public event Action<int, int> OnChange;

        /// <summary>
        /// Raised once, when the health points reach zero.
        /// </summary>
        public event Action OnDied;

        public int Current => _current;
        public bool IsDead => _current <= 0;

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Fills the health points. Called by the owner in its Awake.
        /// </summary>
        public void Initialize()
        {
            SetCurrent(Max);
        }

        /// <summary>
        /// Removes health points and raises OnDied the first time they reach zero.
        /// Returns the health points actually removed.
        /// </summary>
        public int TakeDamage(DamageInfo damage)
        {
            if (IsDead || damage.Amount <= 0)
                return 0;

            int previous = _current;
            SetCurrent(_current - damage.Amount);

            if (IsDead)
                OnDied?.Invoke();

            return previous - _current;
        }

        /// <summary>
        /// Gives health points back without exceeding Max. Has no effect once dead.
        /// </summary>
        public void Heal(int amount)
        {
            if (IsDead || amount <= 0)
                return;

            SetCurrent(_current + amount);
        }

        #endregion

        #region PRIVATE METHODS

        private void SetCurrent(int value)
        {
            _current = Mathf.Clamp(value, 0, Max);
            OnChange?.Invoke(_current, Max);
        }

        #endregion
    }
}
