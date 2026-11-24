using System;
using Spine;
using Spine.Unity;
using UnityEngine;

namespace Core.Gameplay.Bomb
{
    public class BombExplosionVisualScaler : MonoBehaviour
    {
        private const string ImpactEventName = "bomb_impact";

        [SerializeField] private Transform _explosionVisual;
        [Tooltip("Local-space bounds of the bright central burst at bomb_impact, excluding flare rays and glow.")]
        [SerializeField] private Rect _visualBounds = new Rect(-0.5f, -0.5f, 1f, 1f);

        private Vector3 _baseScale;
        private bool _hasBaseScale;

        private void Awake()
        {
            CaptureBaseScale();
        }

        public void ScaleToRadius(float radius, float aspectRatio, Vector3 epicenter)
        {
            CaptureBaseScale();
            if (_explosionVisual == null) return;
            if (_visualBounds.width <= 0f || _visualBounds.height <= 0f) return;

            var parentScale = _explosionVisual.parent != null
                ? _explosionVisual.parent.lossyScale
                : Vector3.one;
            if (Mathf.Approximately(parentScale.x, 0f) || Mathf.Approximately(parentScale.y, 0f)) return;

            var diameter = 2f * Mathf.Max(0f, radius);
            _explosionVisual.localScale = new Vector3(
                Mathf.Sign(_baseScale.x) * diameter / (_visualBounds.width * Mathf.Abs(parentScale.x)),
                Mathf.Sign(_baseScale.y) * diameter * Mathf.Max(0f, aspectRatio) /
                    (_visualBounds.height * Mathf.Abs(parentScale.y)),
                _baseScale.z
            );

            // The artwork's origin is offset from the burst's center.
            var centerOffset = _explosionVisual.TransformVector(_visualBounds.center);
            _explosionVisual.position = epicenter - centerOffset;
        }

        public void WhenFullRadius(Action onImpact)
        {
            var animation = GetComponent<SkeletonAnimation>();
            var track = animation?.AnimationState?.GetCurrent(0);
            if (track == null)
            {
                Debug.LogError($"[BombExplosionVisualScaler] No explosion animation is playing on '{name}'.", this);
                return;
            }

            track.Event += HandleImpact;

            void HandleImpact(TrackEntry entry, Spine.Event animationEvent)
            {
                if (animationEvent.Data.Name != ImpactEventName) return;
                entry.Event -= HandleImpact;
                onImpact();
            }
        }

        private void CaptureBaseScale()
        {
            if (_hasBaseScale) return;

            if (_explosionVisual == null)
                _explosionVisual = transform;

            _baseScale = _explosionVisual.localScale;
            _hasBaseScale = true;
        }
    }
}
