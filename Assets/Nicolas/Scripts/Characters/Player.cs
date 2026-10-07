using Nicolas.Arena.Combat;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Nicolas.Arena.Characters
{
	public class Player : MonoBehaviour, IDamageable
	{
#region Fields
		[System.Serializable] public class Settings
		{
			[Tooltip("Movement speed, in km/h.")]
			[Min(0f)] public float Speed = 5f;
		}
		[System.Serializable] public struct References
		{
			public CharacterController Controller;
			public InputActionReference MoveAction;
		}
		[System.Serializable] public class StateContainer
		{
			public bool IsMoving;
		}

		private const float KMH_TO_MS = 1f / 3.6f;

		[SerializeField] private Settings _settings;
		[SerializeField] private References _references;
		[SerializeField] private StateContainer _state;
		[SerializeField] private Health _health;

		public static Player Instance { get; private set; }
		public StateContainer State => _state;

		private Camera _camera;
		private Vector2 _moveInput;
#endregion

#region Unity Lifecycle
		void Awake()
		{
			// Only one player can exist: a second one is removed
			if (Instance && Instance != this)
			{
				Debug.LogWarning($"A second Player was found on {name}, it is destroyed.");
				Destroy(gameObject);
				return;
			}

			Instance = this;

			// Camera.main is the camera tagged MainCamera
			_camera = Camera.main;
			
			_health.Initialize();
		}

		void OnEnable()
		{
			_references.MoveAction.action.Enable();
		}

		void OnDisable()
		{
			_references.MoveAction.action.Disable();
		}

		void OnDestroy()
		{
			if (Instance == this)
				Instance = null;
		}

		void Update()
		{
			if (_health.IsDead)
				return;
			
			float deltaTime = Time.deltaTime;

			GetInputs();
			Move(deltaTime);
		}
#endregion

#region Public Methods
		public void Teleport(Vector4 pose)
		{
			// An enabled CharacterController would overwrite the new position
			_references.Controller.enabled = false;

			Vector3 position = new Vector3(pose.x, pose.y, pose.z);
			Quaternion rotation = Quaternion.Euler(0f, pose.w, 0f);
			transform.SetPositionAndRotation(position, rotation);

			_references.Controller.enabled = true;
		}

		public void Teleport(Transform target)
		{
			Vector3 position = target.position;
			Vector4 pose = new Vector4(position.x, position.y, position.z, target.eulerAngles.y);
			Teleport(pose);
		}
		
		public void TakeDamage(int amount)
		{
			_health.TakeDamage(amount);
		}
		
#endregion

#region Private Methods
		private void GetInputs()
		{
			_moveInput = _references.MoveAction.action.ReadValue<Vector2>();
		}

		private void Move(float deltaTime)
		{
			Vector3 direction = ToWorldDirection(_moveInput);
			_state.IsMoving = direction.sqrMagnitude > 0f;

			Vector3 velocity = direction * _settings.Speed * KMH_TO_MS;
			_references.Controller.Move(velocity * deltaTime);
		}

		private Vector3 ToWorldDirection(Vector2 input)
		{
			// Only the camera rotation around the vertical axis is kept
			Quaternion cameraHeading = Quaternion.Euler(0f, _camera.transform.eulerAngles.y, 0f);
			return cameraHeading * new Vector3(input.x, 0f, input.y);
		}
#endregion

		
#region Debugging
#if UNITY_EDITOR
		[ContextMenu("Debug/Take 25 Damage")]
		private void DebugTakeDamage()
		{
			TakeDamage(25);
		}
#endif
#endregion
	}

}