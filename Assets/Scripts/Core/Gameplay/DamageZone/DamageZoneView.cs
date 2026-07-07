using Reflex.Attributes;
using Spine.Unity;
using UnityEngine;

namespace Core.Gameplay
{
    public class DamageZoneView : MonoBehaviour
    {
        private const string AnimIdle   = "idle";
        private const string AnimAttack = "attack";

        [SerializeField] private SkeletonAnimation _circle;
        [SerializeField] private SkeletonAnimation _manualWaveBack;
        [SerializeField] private SkeletonAnimation _manualWaveFront;
        [SerializeField] private SkeletonAnimation _autoWaveBack;
        [SerializeField] private SkeletonAnimation _autoWaveFront;
        [Inject] private DamageZoneConfig _config;

        [Tooltip("World-space X radius of _circle at its prefab scale (0.2, 0.2, 1). Calibrate once using Gizmos.")]
        [SerializeField] private float _baseRadiusX = 0.6f;

        private Vector3 _circleBaseScale;
        private Vector3 _manualWaveBackBaseScale;
        private Vector3 _manualWaveFrontBaseScale;
        private Vector3 _autoWaveBackBaseScale;
        private Vector3 _autoWaveFrontBaseScale;
        private DamageZone _damageZone;

        private void Awake()
        {
            _circleBaseScale = GetBaseScale(_circle);
            _manualWaveBackBaseScale = GetBaseScale(_manualWaveBack);
            _manualWaveFrontBaseScale = GetBaseScale(_manualWaveFront);
            _autoWaveBackBaseScale = GetBaseScale(_autoWaveBack);
            _autoWaveFrontBaseScale = GetBaseScale(_autoWaveFront);
        }

        public void Bind(DamageZone damageZone)
        {
            _damageZone = damageZone;
            _damageZone.OnZoneTick += HandleZoneTick;
        }

        private void OnDestroy()
        {
            if (_damageZone != null)
                _damageZone.OnZoneTick -= HandleZoneTick;
        }

        private void Update()
        {
            if (_damageZone == null) return;
            transform.position = _damageZone.WorldPosition;
            UpdateCircleScale();
        }

        private void UpdateCircleScale()
        {
            var s = _damageZone.RadiusX / _baseRadiusX;
            SetScale(_circle, _circleBaseScale, s);
            SetScale(_manualWaveBack, _manualWaveBackBaseScale, s);
            SetScale(_manualWaveFront, _manualWaveFrontBaseScale, s);
            SetScale(_autoWaveBack, _autoWaveBackBaseScale, s);
            SetScale(_autoWaveFront, _autoWaveFrontBaseScale, s);
        }

        private void HandleZoneTick(DamageZone.AttackSource source)
        {
            switch (source)
            {
                case DamageZone.AttackSource.Manual:
                    PlayAttack(_manualWaveBack);
                    PlayAttack(_manualWaveFront);
                    break;
                case DamageZone.AttackSource.Auto:
                    PlayAttack(_autoWaveBack);
                    PlayAttack(_autoWaveFront);
                    break;
            }
        }

        private void PlayAttack(SkeletonAnimation wave)
        {
            if (!EnsureReady(wave) || wave.Skeleton.Data.FindAnimation(AnimAttack) == null)
                return;

            var entry = wave.AnimationState.SetAnimation(0, AnimAttack, false);
            if (entry == null)
                return;

            entry.MixDuration = 0f;
            entry.Complete += _ =>
            {
                PlayIdle(wave);
            };
        }

        private void PlayIdle(SkeletonAnimation wave)
        {
            if (!EnsureReady(wave) || wave.Skeleton.Data.FindAnimation(AnimIdle) == null)
                return;

            var idle = wave.AnimationState.SetAnimation(0, AnimIdle, true);
            if (idle != null)
                idle.MixDuration = 0f;
        }

        private static Vector3 GetBaseScale(SkeletonAnimation animation)
        {
            return animation != null ? animation.transform.localScale : Vector3.one;
        }

        private static void SetScale(SkeletonAnimation animation, Vector3 baseScale, float scale)
        {
            if (animation == null)
                return;

            animation.transform.localScale = baseScale * scale;
        }

        private static bool EnsureReady(SkeletonAnimation animation)
        {
            if (animation == null)
                return false;

            if (!animation.gameObject.activeSelf)
                animation.gameObject.SetActive(true);

            if (animation.Skeleton == null || animation.AnimationState == null)
                animation.Initialize(false);

            return animation.Skeleton != null &&
                   animation.AnimationState != null;
        }

        private void OnDrawGizmos()
        {
            if (_config == null) return;
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.8f);
            var rx = _config.baseRadius;
            var ry = _config.baseRadius * _config.aspectRatio;
            DrawEllipseXY(transform.position, rx, ry);
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
