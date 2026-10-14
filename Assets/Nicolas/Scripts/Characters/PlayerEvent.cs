using Nicolas.Arena.Characters;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace Nicolas.Arena.Interactions
{
    /// <summary>
    /// Zone that raises OnPlayer when the player enters it, or when the player interacts inside it.
    /// It never acts on the player itself: the actions are set up in the Inspector.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PlayerEvent : MonoBehaviour
    {
        public enum Trigger
        {
            OnPlayerEnter,
            OnPlayerInteract
        }

        [System.Serializable]
        public class Settings
        {
            [Tooltip("OnPlayerEnter: as soon as the player enters. OnPlayerInteract: when the player interacts inside the zone.")]
            public Trigger Trigger;
        }

        #region Fields

        [SerializeField] private Settings _settings;

        [Tooltip("Actions raised for the player: heal, ammo, sound, hide this object...")]
        [SerializeField] private Transform _TpPoints;
        [SerializeField] private float _Angle;
        [SerializeField] private UnityEvent _onPlayer;

        private bool _isListening;

        #endregion

        #region Unity Lifecycle

        private void OnDisable()
        {
            // The object can be hidden while the player is inside
            StopListening();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsPlayer(other))
                return;

            if (_settings.Trigger == Trigger.OnPlayerEnter)
                _onPlayer.Invoke();
            else
                StartListening();
        }

        private void OnTriggerExit(Collider other)
        {
            if (IsPlayer(other))
                StopListening();
        }

        #endregion

        #region Private Methods

        private bool IsPlayer(Collider other)
        {
            return Player.Instance != null && other.gameObject == Player.Instance.gameObject;
        }

        private void StartListening()
        {
            Player.Instance.OnInteract += HandleInteract;
            _isListening = true;
        }

        private void StopListening()
        {
            if (!_isListening)
                return;

            if (Player.Instance != null)
                Player.Instance.OnInteract -= HandleInteract;

            _isListening = false;
        }

        private void HandleInteract()
        {
            _onPlayer.Invoke();
        }
        
        public void Teleports()
        {
            new Vector4(_TpPoints.position.x, _TpPoints.position.y, _TpPoints.position.z, _Angle);
            Player.Instance.Teleport(_TpPoints);
            StopListening();
        }

        #endregion
    }
}