using Spine;
using Reflex.Attributes;
using Spine.Unity;
using UnityEngine;

namespace Core.Gameplay
{
    public class DamageZoneView : MonoBehaviour
    {
        private const string AnimIdle   = "idle";
        private const string AnimAttack = "attack";
        private const string AnimReady  = "ready";
        private const string SpecialCooldownBoneName = "cooldown";

        [SerializeField] private SkeletonAnimation _circle;
        [SerializeField] private SkeletonAnimation _manualWaveBack;
        [SerializeField] private SkeletonAnimation _manualWaveFront;
        [SerializeField] private SkeletonAnimation _autoWaveBack;
        [SerializeField] private SkeletonAnimation _autoWaveFront;
        [SerializeField] private SkeletonAnimation _specialWave;
        [Inject] private DamageZoneConfig _config;

        [Tooltip("World-space X radius of _circle at its prefab scale (0.2, 0.2, 1). Calibrate once using Gizmos.")]
        [SerializeField] private float _baseRadiusX = 0.6f;

        private Vector3 _circleBaseScale;
        private Vector3 _manualWaveBackBaseScale;
        private Vector3 _manualWaveFrontBaseScale;
        private Vector3 _autoWaveBackBaseScale;
        private Vector3 _autoWaveFrontBaseScale;
        private Vector3 _specialWaveBaseScale;
        private DamageZone _damageZone;
        private Bone _specialCooldownBone;
        private SpecialVisualState _specialVisualState;
        private bool _specialWaveInitialized;

        private enum SpecialVisualState
        {
            None,
            Cooldown,
            Ready,
            Attacking
        }

        private void Awake()
        {
            _circleBaseScale = GetBaseScale(_circle);
            _manualWaveBackBaseScale = GetBaseScale(_manualWaveBack);
            _manualWaveFrontBaseScale = GetBaseScale(_manualWaveFront);
            _autoWaveBackBaseScale = GetBaseScale(_autoWaveBack);
            _autoWaveFrontBaseScale = GetBaseScale(_autoWaveFront);
            _specialWaveBaseScale = GetBaseScale(_specialWave);
        }

        public void Bind(DamageZone damageZone)
        {
            if (_damageZone != null)
                _damageZone.OnZoneTick -= HandleZoneTick;

            _damageZone = damageZone;
            _damageZone.OnZoneTick += HandleZoneTick;
            UpdateSpecialAttackVisual(true);
        }

        private void OnDestroy()
        {
            if (_damageZone != null)
                _damageZone.OnZoneTick -= HandleZoneTick;

            if (_specialWave != null)
                _specialWave.UpdateLocal -= HandleSpecialWaveUpdateLocal;
        }

        private void Update()
        {
            if (_damageZone == null) return;
            transform.position = _damageZone.WorldPosition;
            UpdateCircleScale();
            UpdateSpecialAttackVisual();
        }

        private void UpdateCircleScale()
        {
            var s = _damageZone.RadiusX / _baseRadiusX;
            SetScale(_circle, _circleBaseScale, s);
            SetScale(_manualWaveBack, _manualWaveBackBaseScale, s);
            SetScale(_manualWaveFront, _manualWaveFrontBaseScale, s);
            SetScale(_autoWaveBack, _autoWaveBackBaseScale, s);
            SetScale(_autoWaveFront, _autoWaveFrontBaseScale, s);
            SetScale(_specialWave, _specialWaveBaseScale, s);
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
                case DamageZone.AttackSource.Special:
                    PlaySpecialAttack();
                    break;
            }
        }

        private bool InitializeSpecialWave()
        {
            if (!EnsureReady(_specialWave))
                return false;

            if (_specialWaveInitialized)
                return _specialCooldownBone != null;

            _specialWaveInitialized = true;

            _specialCooldownBone = _specialWave.Skeleton.FindBone(SpecialCooldownBoneName);
            if (_specialCooldownBone == null)
            {
                Debug.LogWarning($"[DamageZoneView] Spine bone '{SpecialCooldownBoneName}' was not found in special wave.", this);
                return false;
            }

            _specialWave.UpdateLocal -= HandleSpecialWaveUpdateLocal;
            _specialWave.UpdateLocal += HandleSpecialWaveUpdateLocal;
            return true;
        }

        private void UpdateSpecialAttackVisual(bool force = false)
        {
            if (_damageZone == null || _specialWave == null)
                return;

            if (!_damageZone.IsSpecialAttackUnlocked)
            {
                if (_specialWave.gameObject.activeSelf)
                    _specialWave.gameObject.SetActive(false);

                _specialVisualState = SpecialVisualState.None;
                return;
            }

            var wasInactive = !_specialWave.gameObject.activeSelf;
            if (!InitializeSpecialWave() || _specialVisualState == SpecialVisualState.Attacking)
                return;

            force |= wasInactive;

            if (_damageZone.IsSpecialAttackReady)
                ShowSpecialReady(force);
            else
                ShowSpecialCooldown(force);
        }

        private void ShowSpecialReady(bool force)
        {
            if (force || _specialVisualState != SpecialVisualState.Ready)
                PlaySpecialLoop(AnimReady, SpecialVisualState.Ready);

            SetSpecialCooldownScale(1f);
        }

        private void ShowSpecialCooldown(bool force)
        {
            if (force || _specialVisualState != SpecialVisualState.Cooldown)
                PlaySpecialLoop(AnimIdle, SpecialVisualState.Cooldown);

            SetSpecialCooldownScale(_damageZone.SpecialAttackCooldownProgress);
        }

        private void PlaySpecialLoop(string animationName, SpecialVisualState state)
        {
            if (_specialWave.Skeleton.Data.FindAnimation(animationName) == null)
                return;

            var entry = _specialWave.AnimationState.SetAnimation(0, animationName, true);
            if (entry == null)
                return;

            entry.MixDuration = 0f;
            _specialVisualState = state;
        }

        private void PlaySpecialAttack()
        {
            if (!EnsureReady(_specialWave) || _specialWave.Skeleton.Data.FindAnimation(AnimAttack) == null)
                return;

            var entry = _specialWave.AnimationState.SetAnimation(0, AnimAttack, false);
            if (entry == null)
                return;

            entry.MixDuration = 0f;
            _specialVisualState = SpecialVisualState.Attacking;
            entry.Complete += _ =>
            {
                if (_specialVisualState != SpecialVisualState.Attacking)
                    return;

                _specialVisualState = SpecialVisualState.None;
                UpdateSpecialAttackVisual(true);
            };
        }

        private void HandleSpecialWaveUpdateLocal(ISkeletonAnimation animation)
        {
            if (_specialVisualState == SpecialVisualState.Attacking || _damageZone == null)
                return;

            SetSpecialCooldownScale(_damageZone.SpecialAttackCooldownProgress);
        }

        private void SetSpecialCooldownScale(float progress)
        {
            if (_specialCooldownBone == null)
                return;

            var scale = Mathf.Clamp01(progress);
            _specialCooldownBone.ScaleX = scale;
            _specialCooldownBone.ScaleY = scale;
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
