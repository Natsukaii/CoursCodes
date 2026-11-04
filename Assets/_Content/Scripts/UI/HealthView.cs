using TMPro;
using UnityEngine;
using Arena.Combat;

namespace Arena.UI
{
    /// <summary>
    /// Displays a Health as "current / max".
    /// </summary>
    public class HealthView : MonoBehaviour
    {
        [System.Serializable]
        private struct References
        {
            public TMP_Text Label;
        }

        #region FIELDS

        [SerializeField] private References _references;

        private Health _health;

        #endregion

        #region UNITY LIFECYCLES

        void OnDestroy()
        {
            Unbind();
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Starts displaying a Health, instead of the previous one. Called by the UiManager.
        /// </summary>
        public void Bind(Health health)
        {
            Unbind();

            _health = health;
            _health.OnChange += HandleHealthChange;
            HandleHealthChange(_health.Current, _health.Max);
        }

        #endregion

        #region PRIVATE METHODS

        private void Unbind()
        {
            // Every subscription has its unsubscription
            if (_health != null)
                _health.OnChange -= HandleHealthChange;

            _health = null;
        }

        private void HandleHealthChange(int current, int max)
        {
            _references.Label.text = string.Format("{0} / {1}", current, max);
        }

        #endregion
    }
}
