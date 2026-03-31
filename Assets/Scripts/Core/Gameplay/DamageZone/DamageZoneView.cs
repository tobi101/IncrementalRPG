using Spine.Unity;
using UnityEngine;

namespace Core.Gameplay
{
    public class DamageZoneView : MonoBehaviour
    {
        private const string AnimIdle   = "idle";
        private const string AnimAttack = "attack";

        [SerializeField] private SkeletonAnimation _wave1;
        [SerializeField] private SkeletonAnimation _wave2;
        [SerializeField] private DamageZoneConfig _config;

        private DamageZone _damageZone;

        public void Bind(DamageZone damageZone)
        {
            _damageZone = damageZone;
            _damageZone.OnDamageTick += HandleDamageTick;
        }

        private void OnDestroy()
        {
            if (_damageZone != null)
                _damageZone.OnDamageTick -= HandleDamageTick;
        }

        private void Update()
        {
            if (_damageZone == null) return;
            transform.position = _damageZone.WorldPosition;
        }

        private void HandleDamageTick()
        {
            PlayAttack(_wave1);
            PlayAttack(_wave2);
        }

        private void PlayAttack(SkeletonAnimation wave)
        {
            var entry = wave.AnimationState.SetAnimation(0, AnimAttack, false);
            entry.MixDuration = 0f;
            entry.Complete += _ =>
            {
                var idle = wave.AnimationState.SetAnimation(0, AnimIdle, true);
                idle.MixDuration = 0f;
            };
        }

        private void OnDrawGizmos()
        {
            if (_config == null) return;
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.8f);
            DrawEllipseXY(transform.position, _config.detectionRadiusX, _config.detectionRadiusY);
        }

        private static void DrawEllipseXY(Vector3 center, float radiusX, float radiusY)
        {
            const int segments = 32;
            var prev = center + new Vector3(radiusX, 0f, 0f);
            for (var i = 1; i <= segments; i++)
            {
                var angle = i * (2f * Mathf.PI / segments);
                var next = center + new Vector3(Mathf.Cos(angle) * radiusX, Mathf.Sin(angle) * radiusY, 0f);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }
    }
}
