using UnityEngine;

namespace Arena.Characters
{
    /// <summary>
    /// Animates a character from the movement of its transform and from the calls of its owner (Player).
    /// Placed on the character root. Only sets parameters: the states, transitions and their durations
    /// are set in the Animator Controller.
    /// Floats Forward and Right: speed in the character space, divided by the walk speed (locomotion blend tree).
    /// Bools IsGrounded, IsRunning, IsStunned. Triggers Jump, Dash, Die, Win, Lose.
    /// </summary>
    public class CharacterAnimator : MonoBehaviour
    {
        [System.Serializable]
        private class Settings
        {
            [Tooltip("Speed that plays the walk animations at full blend, in km/h.")]
            [Min(0.1f)] public float WalkSpeed = 10f;

            [Tooltip("Smoothing of the Forward and Right parameters, in seconds.")]
            [Min(0f)] public float Damping = 0.1f;

            [Tooltip("Speed below which a running character is not shown running, in km/h.")]
            [Min(0f)] public float RunMinSpeed = 2f;
        }

        [System.Serializable]
        private struct References
        {
            public Animator Animator;
        }

        [System.Serializable]
        public class StateContainer
        {
            public bool IsGrounded = true;
            public bool IsRunning;
            public bool IsStunned;
            public bool IsFinal;

            [Tooltip("Speed in the character space, divided by the walk speed. x: right, y: forward.")]
            public Vector2 Locomotion;
        }

        #region FIELDS

        const float KMH_TO_MS = 1 / 3.6f;

        // Parameters, named as in the Animator Controller
        private static readonly int ForwardParameter = Animator.StringToHash("Forward");
        private static readonly int RightParameter = Animator.StringToHash("Right");
        private static readonly int IsGroundedParameter = Animator.StringToHash("IsGrounded");
        private static readonly int IsRunningParameter = Animator.StringToHash("IsRunning");
        private static readonly int IsStunnedParameter = Animator.StringToHash("IsStunned");
        private static readonly int JumpTrigger = Animator.StringToHash("Jump");
        private static readonly int DashTrigger = Animator.StringToHash("Dash");
        private static readonly int DieTrigger = Animator.StringToHash("Die");
        private static readonly int WinTrigger = Animator.StringToHash("Win");
        private static readonly int LoseTrigger = Animator.StringToHash("Lose");

        [SerializeField] private Settings _settings;
        [SerializeField] private References _references;
        [SerializeField] private StateContainer _state;

        public StateContainer State => _state;

        private Vector3 _lastPosition;
        private float _speed;
        private float _stunEndTime;

        #endregion

        #region UNITY LIFECYCLES

        void Awake()
        {
            _lastPosition = transform.position;
            _references.Animator.SetBool(IsGroundedParameter, _state.IsGrounded);
        }

        void Update()
        {
            float t = Time.deltaTime;

            // A paused game (time scale at 0) gives no speed to measure
            if (t > 0f)
                UpdateLocomotion(t);

            if (_state.IsStunned && Time.time >= _stunEndTime)
                SetStunned(false);

            _references.Animator.SetBool(IsRunningParameter, _state.IsRunning && _speed >= _settings.RunMinSpeed * KMH_TO_MS);
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Tells whether the character stands on the ground. Called by the owner when it changes.
        /// </summary>
        public void SetGrounded(bool isGrounded)
        {
            _state.IsGrounded = isGrounded;
            _references.Animator.SetBool(IsGroundedParameter, isGrounded);
        }

        /// <summary>
        /// Tells whether the character is running. Called by the owner when it changes.
        /// IsRunning is true in the Animator only while the character also moves.
        /// </summary>
        public void SetRunning(bool isRunning)
        {
            _state.IsRunning = isRunning;
        }

        public void Jump()
        {
            SetTrigger(JumpTrigger);
        }

        public void Dash()
        {
            SetTrigger(DashTrigger);
        }

        /// <summary>
        /// Sets IsStunned for a duration, in seconds.
        /// </summary>
        public void Stun(float duration)
        {
            if (_state.IsFinal)
                return;

            _stunEndTime = Time.time + duration;
            SetStunned(true);
        }

        public void Die()
        {
            SetFinalTrigger(DieTrigger);
        }

        public void Win()
        {
            SetFinalTrigger(WinTrigger);
        }

        public void Lose()
        {
            SetFinalTrigger(LoseTrigger);
        }

        #endregion

        #region PRIVATE METHODS

        private void UpdateLocomotion(float t)
        {
            Vector3 position = transform.position;
            Vector3 velocity = (position - _lastPosition) / t;
            _lastPosition = position;

            velocity.y = 0f;
            _speed = velocity.magnitude;

            // Speed in the character space, so that a sideways move plays walkRight
            Vector3 localVelocity = transform.InverseTransformDirection(velocity);
            Vector2 locomotion = new Vector2(localVelocity.x, localVelocity.z) / (_settings.WalkSpeed * KMH_TO_MS);
            _state.Locomotion = Vector2.ClampMagnitude(locomotion, 1f);

            _references.Animator.SetFloat(ForwardParameter, _state.Locomotion.y, _settings.Damping, t);
            _references.Animator.SetFloat(RightParameter, _state.Locomotion.x, _settings.Damping, t);
        }

        private void SetStunned(bool isStunned)
        {
            _state.IsStunned = isStunned;
            _references.Animator.SetBool(IsStunnedParameter, isStunned);
        }

        private void SetTrigger(int trigger)
        {
            if (_state.IsFinal || _state.IsStunned)
                return;

            _references.Animator.SetTrigger(trigger);
        }

        private void SetFinalTrigger(int trigger)
        {
            if (_state.IsFinal)
                return;

            _state.IsFinal = true;
            SetStunned(false);

            // Win and lose are played while the game is paused (time scale at 0)
            _references.Animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            _references.Animator.SetTrigger(trigger);
        }

        #endregion
    }
}
