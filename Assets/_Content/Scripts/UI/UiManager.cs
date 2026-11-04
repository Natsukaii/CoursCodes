using UnityEngine;
using Arena.Characters;

namespace Arena.UI
{
    /// <summary>
    /// Single entry point of the UI: connects the views to the player and switches screens.
    /// </summary>
    public class UiManager : MonoBehaviour
    {
        [System.Serializable]
        private struct References
        {
            [Tooltip("In-game screen: health, ammo.")]
            public GameObject Hud;
            public HealthView HealthView;
            public AmmoView AmmoView;
            public GameOverView GameOverView;
        }

        #region FIELDS

        [SerializeField] private References _references;

        private Player _player;

        #endregion

        #region UNITY LIFECYCLES

        void Start()
        {
            // Start: every Awake has run, so Player.Instance is set
            _player = Player.Instance;

            if (!_player)
            {
                Debug.LogError("UiManager: no Player in the scene.", this);
                return;
            }

            _references.HealthView.Bind(_player.Health);
            _references.AmmoView.Bind(_player.Launcher);
            _player.Health.OnDied += HandlePlayerDied;

            _references.Hud.SetActive(true);
            _references.GameOverView.Hide();
        }

        void OnDestroy()
        {
            if (_player)
                _player.Health.OnDied -= HandlePlayerDied;
        }

        #endregion

        #region PRIVATE METHODS

        private void HandlePlayerDied()
        {
            _references.Hud.SetActive(false);
            _references.GameOverView.Show();
        }

        #endregion
    }
}
