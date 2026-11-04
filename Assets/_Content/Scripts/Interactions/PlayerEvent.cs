using UnityEngine;
using UnityEngine.Events;
using Arena.Characters;

namespace Arena.Interactions
{
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

        [SerializeField] private Settings _settings;
        [SerializeField] private UnityEvent _onPlayer;

        private bool _isListening;

        #region UNITY LIFECYCLES

        private void OnDisable()
        {
            // The object can be hidden while the player is inside
            StopListening();
        }

        void OnTriggerEnter(Collider other)
        {
            if (!IsPlayer(other))
                return;

            if (_settings.Trigger == Trigger.OnPlayerEnter)
                _onPlayer.Invoke();
            else
                StartListening();
        }

        void OnTriggerExit(Collider other)
        {
            if (IsPlayer(other))
                StopListening();
        }

        #endregion

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

        public void Teleport(Transform target)
        {
            if (!Player.Instance)
                return;

            StopListening();
            Player.Instance.Teleport(target);
        }
    }
}
