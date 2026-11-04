using UnityEngine;
using UnityEngine.SceneManagement;

namespace Arena.UI
{
    /// <summary>
    /// Game over screen. Its Restart button reloads the scene.
    /// </summary>
    public class GameOverView : MonoBehaviour
    {
        #region PUBLIC METHODS

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Called by the Restart button (OnClick in the Inspector).
        /// </summary>
        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        #endregion
    }
}
