using UnityEngine;

namespace Arena.Characters
{
    [ExecuteAlways]
    public class PlayerCamera : MonoBehaviour
    {
        [System.Serializable]
        public class Settings
        {
            [Tooltip("x: horizontal distance from the player, y: height above the player, in meters.")]
            public Vector2 Offset = new Vector2(7f, 8f);

            [Tooltip("Rotation of the offset around the player, in degrees.")]
            public float Angle = 0f;

            [Tooltip("Time to catch up with the player, in seconds. 0: no smoothing.")]
            [Range(0f, 1f)] public float Smooth = 0.2f;
        }

        [System.Serializable]
        private struct References
        {
            public Player Player;
        }

        #region FIELDS

        [SerializeField] private Settings _settings;
        [SerializeField] private References _references;

        private Vector3 _velocity;

        #endregion

        // LateUpdate: the player has already moved this frame
        void LateUpdate()
        {
            if (!GetPlayer())
                return;

            Vector3 targetPos = GetTargetPosition();

            if (!Application.isPlaying || _settings.Smooth == 0)
            {
                transform.position = targetPos;
            }
            else
            {
                transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, _settings.Smooth);
            }
        }

        private bool GetPlayer()
        {
            if (_references.Player)
                return true;

            // In edit mode, Player.Awake has not run, so the singleton is empty
            else if (Player.Instance)
                _references.Player = Player.Instance;
            else
                _references.Player = FindFirstObjectByType<Player>();

            return _references.Player != null;
        }

        private Vector3 GetTargetPosition()
        {
            Quaternion rotation = Quaternion.Euler(0, _settings.Angle, 0);
            Vector3 offset = rotation * new Vector3(0, _settings.Offset.y, -_settings.Offset.x);
            return _references.Player.transform.position + offset;
        }
    }
}
