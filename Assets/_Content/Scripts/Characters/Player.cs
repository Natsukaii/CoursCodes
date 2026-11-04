using Arena.Combat;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Arena.Characters
{
    public class Player : MonoBehaviour, IDamageable
    {
        [System.Serializable]
        private class Settings
        {
            [Tooltip("Movement speed, in km/h.")]
            public float Speed = 10;

            [Tooltip("Smoothing of the orientation, in seconds. 0: the player turns instantly.")]
            [Min(0f)] public float RotationDamping = 0.08f;

            [Tooltip("Speed while the sprint action is held, in km/h. The player then faces where it runs and cannot shoot.")]
            [Min(0f)] public float RunSpeed = 18f;

            [Tooltip("Distance covered by a dash, in meters.")]
            [Min(0f)] public float DashDistance = 4f;

            [Tooltip("Duration of a dash, in seconds.")]
            [Min(0.05f)] public float DashDuration = 0.2f;

            [Tooltip("Time between the start of two dashes, in seconds.")]
            [Min(0f)] public float DashCooldown = 1f;

            [Tooltip("Jump height, in meters.")]
            [Min(0f)] public float JumpHeight = 1.2f;

            [Tooltip("Gravity, in meters per second squared.")]
            [Min(0f)] public float Gravity = 20f;
        }

        [System.Serializable]
        private struct References
        {
            public CharacterController Controller;
            public InputActionReference MoveAction;
            public InputActionReference InteractAction;
            public InputActionReference LookAction;
            public InputActionReference AttackAction;
            public InputActionReference JumpAction;
            public InputActionReference SprintAction;

            [Tooltip("Crouch action of the template, used for the dash.")]
            public InputActionReference DashAction;
            public ProjectileLauncher Launcher;
            public CharacterAnimator CharacterAnimator;
        }

        [System.Serializable]
        public class StateContainer
        {
            public bool IsMoving = false;
            public bool IsGrounded = true;
            public bool IsRunning = false;
            public bool IsDashing = false;
            public bool IsStunned = false;
        }

        #region FIELDS

        public static Player Instance { get; private set; }

        const float KMH_TO_MS = 1 / 3.6f;
        const float AIM_STICK_DEADZONE = 0.2f;

        // Small downward speed kept on the ground, so that the CharacterController stays grounded
        const float GROUNDED_SPEED = -2f;

        [SerializeField] private Settings _settings;
        [SerializeField] private References _references;
        [SerializeField] private StateContainer _state;
        [SerializeField] private Health _health;

        public event Action OnInteract;

        public StateContainer State => _state;
        public Health Health => _health;
        public ProjectileLauncher Launcher => _references.Launcher;

        private bool _isInteracting;
        private Vector2 _moveInput;
        private Camera _camera;
        private Vector2 _aimStickInput;
        private bool _isAimingWithStick;
        private bool _isAttacking;
        private Vector3 _aimDirection = Vector3.forward;
        private bool _isJumping;
        private bool _isSprinting;
        private bool _isDashPressed;
        private Vector3 _moveDirection;
        private Vector3 _dashDirection;
        private float _dashEndTime;
        private float _nextDashTime;
        private float _verticalSpeed;
        private float _stunEndTime;

        #endregion

        #region UNITY LIFECYCLES

        void Awake()
        {
            if (Instance && Instance != this)
            {
                Debug.LogWarning($"A second Player was found on {name}, it is destroyed.");
                Destroy(gameObject);
                return;
            }

            Instance = this;

            _health.Initialize();
            _health.OnDied += HandleDied;

            _camera = Camera.main;
        }

        void OnEnable()
        {
            _references.MoveAction.action.Enable();
            _references.InteractAction.action.Enable();
            _references.LookAction.action.Enable();
            _references.AttackAction.action.Enable();
            _references.JumpAction.action.Enable();
            _references.SprintAction.action.Enable();
            _references.DashAction.action.Enable();
        }

        void OnDisable()
        {
            if (Instance == this)
            {
                _references.MoveAction.action.Disable();
                _references.InteractAction.action.Disable();
                _references.LookAction.action.Disable();
                _references.AttackAction.action.Disable();
                _references.JumpAction.action.Disable();
                _references.SprintAction.action.Disable();
                _references.DashAction.action.Disable();
            }
        }

        void OnDestroy()
        {
            _health.OnDied -= HandleDied;

            if (Instance == this)
                Instance = null;
        }

        void Update()
        {
            if (_health.IsDead)
                return;

            float t = Time.deltaTime;

            GetInputs();
            UpdateStun();
            Move(t);

            // A stunned player only falls: no aiming, shooting nor interacting
            if (_state.IsStunned)
                return;

            Aim(t);
            Attack();
            Interact();
        }

        #endregion

        #region PUBLIC METHODS

        public void TakeDamage(DamageInfo damage)
        {
            _health.TakeDamage(damage);
        }

        /// <summary>
        /// Takes the control away from the player for a duration, in seconds, and plays the stunned animation.
        /// </summary>
        public void Stun(float duration)
        {
            if (_health.IsDead)
                return;

            _stunEndTime = Time.time + duration;
            _state.IsStunned = true;
            _state.IsDashing = false;
            SetRunning(false);
            _references.CharacterAnimator.Stun(duration);
        }

        /// <summary>
        /// Teleports the player.
        /// </summary>
        /// <param name="pose">x, y, z: position. w: angle around the vertical axis, in degrees.</param>
        public void Teleport(Vector4 pose)
        {
            // An enabled CharacterController would overwrite the new position
            _references.Controller.enabled = false;

            Vector3 position = new Vector3(pose.x, pose.y, pose.z);
            Quaternion rotation = Quaternion.Euler(0f, pose.w, 0f);
            transform.SetPositionAndRotation(position, rotation);

            _references.Controller.enabled = true;
        }

        /// <summary>
        /// Teleports the player to the position and heading of a target, such as a spawn point.
        /// </summary>
        public void Teleport(Transform target)
        {
            Vector3 position = target.position;
            Vector4 pose = new Vector4(position.x, position.y, position.z, target.eulerAngles.y);
            Teleport(pose);
        }

        #endregion

        #region PRIVATE METHODS

#if UNITY_EDITOR
        [ContextMenu("Debug/Take 25 Damage")]
        private void DebugTakeDamage()
        {
            TakeDamage(new DamageInfo(25, DamageType.Physical, gameObject));
        }

        [ContextMenu("Debug/Stun 2 s")]
        private void DebugStun()
        {
            Stun(2f);
        }
#endif

        private void GetInputs()
        {
            _moveInput = _references.MoveAction.action.ReadValue<Vector2>();
            _isInteracting = _references.InteractAction.action.WasPressedThisFrame();
            _isAttacking = _references.AttackAction.action.IsPressed();
            _isJumping = _references.JumpAction.action.WasPressedThisFrame();
            _isSprinting = _references.SprintAction.action.IsPressed();
            _isDashPressed = _references.DashAction.action.WasPressedThisFrame();

            InputAction look = _references.LookAction.action;
            _aimStickInput = look.ReadValue<Vector2>();

            // Look is also bound to the mouse delta: only the gamepad counts as a stick
            bool isStick = look.activeControl != null && look.activeControl.device is Gamepad;

            // The last aiming device used wins: the stick when pushed, the mouse when moved
            if (isStick && IsStickPushed())
                _isAimingWithStick = true;
            else if (Pointer.current != null && Pointer.current.delta.ReadValue().sqrMagnitude > 0f)
                _isAimingWithStick = false;
        }

        private void Move(float t)
        {
            Vector2 input = _state.IsStunned ? Vector2.zero : _moveInput;
            _moveDirection = ToWorldDirection(input);
            bool hasInput = _moveDirection.sqrMagnitude > .01f;

            if (_state.IsDashing && Time.time >= _dashEndTime)
                _state.IsDashing = false;

            if (_isDashPressed && !_state.IsStunned && !_state.IsDashing && Time.time >= _nextDashTime)
                Dash(hasInput ? _moveDirection.normalized : transform.forward);

            SetRunning(_isSprinting && hasInput && !_state.IsStunned);

            Vector3 velocity;

            if (_state.IsDashing)
            {
                velocity = _dashDirection * (_settings.DashDistance / _settings.DashDuration);
            }
            else
            {
                float speed = _state.IsRunning ? _settings.RunSpeed : _settings.Speed;
                velocity = _moveDirection * KMH_TO_MS * speed;
            }

            _state.IsMoving = velocity.sqrMagnitude > .01f;

            if (_state.IsGrounded && _isJumping && !_state.IsStunned)
                Jump();

            // Gravity: the CharacterController does not apply it
            _verticalSpeed -= _settings.Gravity * t;
            velocity.y = _verticalSpeed;

            _references.Controller.Move(velocity * t);

            SetGrounded(_references.Controller.isGrounded);

            if (_state.IsGrounded && _verticalSpeed < 0f)
                _verticalSpeed = GROUNDED_SPEED;
        }

        private void Jump()
        {
            // Speed that reaches the jump height against gravity: v = sqrt(2 g h)
            _verticalSpeed = Mathf.Sqrt(2f * _settings.Gravity * _settings.JumpHeight);
            _references.CharacterAnimator.Jump();
        }

        private void Dash(Vector3 direction)
        {
            _dashDirection = direction;
            _dashEndTime = Time.time + _settings.DashDuration;
            _nextDashTime = Time.time + _settings.DashCooldown;
            _state.IsDashing = true;
            _references.CharacterAnimator.Dash();
        }

        private void SetRunning(bool isRunning)
        {
            if (_state.IsRunning == isRunning)
                return;

            _state.IsRunning = isRunning;
            _references.CharacterAnimator.SetRunning(isRunning);
        }

        private void SetGrounded(bool isGrounded)
        {
            if (_state.IsGrounded == isGrounded)
                return;

            _state.IsGrounded = isGrounded;
            _references.CharacterAnimator.SetGrounded(isGrounded);
        }

        private void UpdateStun()
        {
            if (_state.IsStunned && Time.time >= _stunEndTime)
                _state.IsStunned = false;
        }

        private void HandleDied()
        {
            _references.CharacterAnimator.Die();
        }


        private void Aim(float t)
        {
            // Running and dashing face the movement: aiming is suspended
            if (_state.IsDashing)
            {
                TurnTowards(_dashDirection, t);
                return;
            }

            // In the air, the player keeps the orientation it had when it left the ground
            if (!_state.IsGrounded)
                return;

            if (_state.IsRunning)
            {
                TurnTowards(_moveDirection, t);
                return;
            }

            if (_isAimingWithStick)
            {
                // Released stick: the player keeps its last direction
                if (IsStickPushed())
                    _aimDirection = ToWorldDirection(_aimStickInput);
            }
            else if (TryGetPointerDirection(out Vector3 direction))
            {
                _aimDirection = direction;
            }

            TurnTowards(_aimDirection, t);
        }

        private void TurnTowards(Vector3 direction, float t)
        {
            Quaternion target = Quaternion.LookRotation(direction);

            if (_settings.RotationDamping <= 0f)
            {
                transform.rotation = target;
            }
            else
            {
                // Exponential smoothing: the same feel whatever the frame rate
                float blend = 1f - Mathf.Exp(-t / _settings.RotationDamping);
                transform.rotation = Quaternion.Slerp(transform.rotation, target, blend);
            }
        }

        private void Attack()
        {
            // No shooting while running
            if (_isAttacking && !_state.IsRunning)
                _references.Launcher.TryFire();
        }

        private void Interact()
        {
            if (_isInteracting)
                OnInteract?.Invoke();
        }

        private bool IsStickPushed()
        {
            return _aimStickInput.sqrMagnitude > AIM_STICK_DEADZONE * AIM_STICK_DEADZONE;
        }

        private Vector3 ToWorldDirection(Vector2 input)
        {
            // Only the camera rotation around the vertical axis is kept
            Quaternion cameraHeading = Quaternion.Euler(0f, _camera.transform.eulerAngles.y, 0f);
            return cameraHeading * new Vector3(input.x, 0f, input.y);
        }

        /// <summary>
        /// Direction from the player to the point under the pointer, on a horizontal plane at the player's height.
        /// </summary>
        private bool TryGetPointerDirection(out Vector3 direction)
        {
            direction = Vector3.zero;

            if (Pointer.current == null)
                return false;

            // Mouse position read directly: the Look action only gives its movement
            Ray ray = _camera.ScreenPointToRay(Pointer.current.position.ReadValue());
            Plane ground = new Plane(Vector3.up, transform.position);

            if (!ground.Raycast(ray, out float distance))
                return false;

            direction = ray.GetPoint(distance) - transform.position;
            direction.y = 0f;
            return direction.sqrMagnitude > 0.01f;
        }

        #endregion
    }
}
