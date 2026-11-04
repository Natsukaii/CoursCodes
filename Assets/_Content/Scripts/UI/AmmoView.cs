using TMPro;
using UnityEngine;
using Arena.Combat;

namespace Arena.UI
{
    /// <summary>
    /// Displays the ammo of a launcher as "current / max".
    /// </summary>
    public class AmmoView : MonoBehaviour
    {
        [System.Serializable]
        private struct References
        {
            public TMP_Text Label;
        }

        #region FIELDS

        [SerializeField] private References _references;

        private ProjectileLauncher _launcher;

        #endregion

        #region UNITY LIFECYCLES

        void OnDestroy()
        {
            Unbind();
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Starts displaying the ammo of a launcher, instead of the previous one. Called by the UiManager.
        /// </summary>
        public void Bind(ProjectileLauncher launcher)
        {
            Unbind();

            _launcher = launcher;

            if (!_launcher)
            {
                _references.Label.SetText("-");
                return;
            }

            _launcher.OnAmmoChange += HandleAmmoChange;
            HandleAmmoChange(_launcher.State.Ammo, _launcher.MaxAmmo);
        }

        #endregion

        #region PRIVATE METHODS

        private void Unbind()
        {
            if (_launcher)
                _launcher.OnAmmoChange -= HandleAmmoChange;

            _launcher = null;
        }

        private void HandleAmmoChange(int current, int max)
        {
            _references.Label.text = string.Format("{0} / {1}", current, max);
        }

        #endregion
    }
}
