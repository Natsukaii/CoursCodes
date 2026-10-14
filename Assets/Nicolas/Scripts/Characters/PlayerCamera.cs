using System;
using UnityEngine;

namespace Nicolas.Arena.Characters
{
	[ExecuteAlways]
	public class PlayerCamera : MonoBehaviour
	{
		[System.Serializable]
		public class Settings
		{
			[Tooltip("x: horizontal distance from the player, y: height above the player, in meters.")]
			public Vector2 Offset = new Vector2(8f, 12f);

			[Tooltip("Rotation of the offset around the player, in degrees.")]
			public float Angle = 0f;

			[Tooltip("Time to catch up with the player, in seconds. 0: no smoothing.")]
			[Range(0f, 1f)] public float Smooth = 0.2f;
		}

		[System.Serializable]
		public struct References
		{
			[Tooltip("Followed player. Found automatically when empty.")]
			public Player Player;
		}

		#region Fields

		[SerializeField] private Settings _settings;
		[SerializeField] private References _references;

		private Vector3 _velocity;

		#endregion

		#region Unity Lifecycle

		// LateUpdate: the player has already moved this frame
		void LateUpdate()
		{
			if (!GetPlayer())
				return;

			Vector3 targetPos = GetTargetPosition();

			// No smoothing in edit mode: the camera snaps while the scene is edited
			if (!Application.isPlaying || _settings.Smooth <= 0f)
			{
				transform.position = targetPos;
				_velocity = Vector3.zero;
			}
			else
			{
				transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _velocity, _settings.Smooth);
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Looks for the player: singleton first, then a search in the scene. Returns false if none is found.
		/// </summary>
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
			Quaternion rotation = Quaternion.Euler(0f, _settings.Angle, 0f);
			Vector3 offset = rotation * new Vector3(0f, _settings.Offset.y, -_settings.Offset.x);
			return _references.Player.transform.position + offset;
		}

		#endregion
	}
}