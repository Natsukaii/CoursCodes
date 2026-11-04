using System.Collections.Generic;
using UnityEngine;

namespace Arena.Combat
{
    /// <summary>
    /// Sphere that damages every IDamageable inside it, less and less towards its edge.
    /// Continuous: hits every tick while a target stays inside (contact damage, toxic area).
    /// Instant: hits everything in the radius once, when Explode is called (explosion).
    /// </summary>
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public class DamageZone : MonoBehaviour
    {
        public enum Mode
        {
            Continuous,
            Instant
        }

        [System.Serializable]
        private class Settings
        {
            [Tooltip("Continuous: damage every tick while a target stays inside. Instant: one hit on everything in the radius, when Explode is called.")]
            public Mode Mode;

            [Tooltip("Damage at the center of the sphere.")]
            [Min(0)] public int Damage = 10;

            public DamageType DamageType;

            [Tooltip("Damage multiplier by distance to the center. 0: center, 1: edge of the sphere.")]
            public AnimationCurve Falloff = AnimationCurve.Linear(0f, 1f, 1f, 0f);

            [Tooltip("Continuous mode: time between two hits on the same target, in seconds.")]
            [Min(0.05f)] public float TickInterval = 0.5f;

            [Tooltip("Instant mode: explode as soon as the object is enabled, for an explosion prefab.")]
            public bool ExplodeOnEnable = true;
        }

        [System.Serializable]
        private struct References
        {
            public SphereCollider Zone;

            [Tooltip("Never hurt by the zone and given as damage source, usually the object that carries it.")]
            public GameObject Owner;
        }

        #region FIELDS

        [SerializeField] private Settings _settings;
        [SerializeField] private References _references;

        private readonly Dictionary<IDamageable, float> _nextHitTimes = new Dictionary<IDamageable, float>();
        private readonly HashSet<IDamageable> _explosionTargets = new HashSet<IDamageable>();

        #endregion

        #region UNITY LIFECYCLES

        void Reset()
        {
            // Called when the component is added: a trigger sphere that physics can detect
            _references.Zone = GetComponent<SphereCollider>();
            _references.Zone.isTrigger = true;
            GetComponent<Rigidbody>().isKinematic = true;
        }

        void OnEnable()
        {
            if (_settings.Mode == Mode.Instant && _settings.ExplodeOnEnable)
                Explode();
        }

        void OnDisable()
        {
            _nextHitTimes.Clear();
        }

        void OnTriggerStay(Collider other)
        {
            if (_settings.Mode != Mode.Continuous || !TryGetTarget(other, out IDamageable target))
                return;

            if (_nextHitTimes.TryGetValue(target, out float nextHitTime) && Time.time < nextHitTime)
                return;

            _nextHitTimes[target] = Time.time + _settings.TickInterval;
            Hit(target, other.transform.position);
        }

        void OnTriggerExit(Collider other)
        {
            if (TryGetTarget(other, out IDamageable target))
                _nextHitTimes.Remove(target);
        }

        void OnDrawGizmosSelected()
        {
            if (!_references.Zone)
                return;

            Gizmos.color = new Color(1f, 0.45f, 0.35f, 0.8f);
            Gizmos.DrawWireSphere(GetCenter(), GetRadius());
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Hits once every IDamageable in the radius. Can be linked to a UnityEvent.
        /// </summary>
        public void Explode()
        {
            _explosionTargets.Clear();
            Collider[] colliders = Physics.OverlapSphere(GetCenter(), GetRadius(), ~0, QueryTriggerInteraction.Ignore);

            foreach (Collider other in colliders)
            {
                // A target with several colliders is hit only once
                if (TryGetTarget(other, out IDamageable target) && _explosionTargets.Add(target))
                    Hit(target, other.transform.position);
            }
        }

        #endregion

        #region PRIVATE METHODS

        private bool TryGetTarget(Collider other, out IDamageable target)
        {
            target = other.GetComponentInParent<IDamageable>();

            if (target == null)
                return false;

            // The owner never hurts itself
            return !_references.Owner || ((Component)target).gameObject != _references.Owner;
        }

        private void Hit(IDamageable target, Vector3 targetPosition)
        {
            float distance = Vector3.Distance(GetCenter(), targetPosition);
            float falloff = _settings.Falloff.Evaluate(Mathf.Clamp01(distance / GetRadius()));
            int amount = Mathf.RoundToInt(_settings.Damage * falloff);

            if (amount <= 0)
                return;

            GameObject source = _references.Owner ? _references.Owner : gameObject;
            target.TakeDamage(new DamageInfo(amount, _settings.DamageType, source));
        }

        private Vector3 GetCenter()
        {
            return transform.TransformPoint(_references.Zone.center);
        }

        private float GetRadius()
        {
            // The sphere radius follows the largest scale axis
            Vector3 scale = transform.lossyScale;
            float maxScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
            return Mathf.Max(_references.Zone.radius * maxScale, 0.01f);
        }

        #endregion
    }
}
