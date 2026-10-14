using UnityEngine;
using UnityEngine.InputSystem;

namespace arena_romeo.Characters
{
	public class Player : MonoBehaviour
	{
		[System.Serializable]
		public class Settings
		{
			
			[Min(0f)] public float Speed = 10f;
		}

		[System.Serializable]
		public struct References
		{
			public CharacterController Controller;
			public InputActionReference MoveAction;
		}

		[System.Serializable]
		public class StateContainer
		{
			public bool IsMoving;
		}

		#region Fields

		

		private const float KMH_TO_MS = 1 / 3.6f;

		

		[SerializeField] private Settings _settings;
		[SerializeField] private References _references;
		[SerializeField] private StateContainer _state;

		/// <summary>
		/// The only player of the scene.
		/// </summary>
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
			float t = Time.deltaTime;

			GetInputs();
			Move(t);
		}

		#endregion

		#region Public Methods

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

		#region Private Methods

		private void GetInputs()
		{
			_moveInput = _references.MoveAction.action.ReadValue<Vector2>();
		}

		private void Move(float t)
		{
			Vector3 direction = ToWorldDirection(_moveInput);
			_state.IsMoving = direction.sqrMagnitude > 0f;
			
			Vector3 velocity = direction * _settings.Speed * KMH_TO_MS;
			_references.Controller.Move(velocity * t);

		}

		
		private Vector3 ToWorldDirection(Vector2 input)
		{
			Quaternion cameraHeading = Quaternion.Euler(0f, _camera.transform.eulerAngles.y, 0f);
			return cameraHeading * new Vector3(input.x, 0f, input.y);
		}

		#endregion
	}
}