using UnityEngine;
using Arena.Characters;

namespace Arena.Interactions
{
    /// <summary>
    /// Gives ammo to the player. Apply is meant to be linked to a PlayerEvent.
    /// </summary>
    public class AmmoEffect : MonoBehaviour
    {
        [System.Serializable]
        public class Settings
        {
            [Tooltip("Ammo given to the player.")]
            [Min(1)] public int Amount = 10;
        }

        #region FIELDS

        [SerializeField] private Settings _settings;

        #endregion

        #region PUBLIC METHODS

        public void Apply()
        {
            if (Player.Instance != null)
                Player.Instance.Launcher.AddAmmo(_settings.Amount);
        }

        #endregion
    }
}
