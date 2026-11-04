using UnityEngine;
using Arena.Characters;

namespace Arena.Interactions
{
    /// <summary>
    /// Heals the player. Apply is meant to be linked to a PlayerEvent.
    /// </summary>
    public class HealEffect : MonoBehaviour
    {
        [System.Serializable]
        public class Settings
        {
            [Tooltip("Health points given to the player.")]
            [Min(1)] public int Amount = 25;
        }

        #region FIELDS

        [SerializeField] private Settings _settings;

        #endregion

        #region PUBLIC METHODS

        public void Apply()
        {
            if (Player.Instance != null)
                Player.Instance.Health.Heal(_settings.Amount);
        }

        #endregion
    }
}
