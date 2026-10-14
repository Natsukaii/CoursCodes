using System;
using UnityEngine;

namespace Nicolas.Arena.Combat
{
    [System.Serializable]
    public class Health
    {
#region Fields
        [Tooltip("Hit points when full.")]
        [Min(1)] public int Max = 100;

        [Tooltip("Current hit points, visible for debugging.")]
        [SerializeField] private int _current;

        // Raised with the current and max hit points each time they change.
        public event Action<int, int> OnChange;

        // Raised once, when the hit points reach zero.
        public event Action OnDied;

        public int Current => _current;
        public bool IsDead => _current <= 0;
#endregion

#region Public Methods
        public void Initialize()
        {
            SetCurrent(Max);
        }

        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0)
                return;

            SetCurrent(_current - amount);

            if (IsDead)
                OnDied?.Invoke();
        }

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0)
                return;

            SetCurrent(_current + amount);
        }
#endregion

#region Private Methods
        private void SetCurrent(int value)
        {
            _current = Mathf.Clamp(value, 0, Max);
            OnChange?.Invoke(_current, Max);
        }
#endregion
    }
}