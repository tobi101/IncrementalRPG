using UnityEngine;

namespace UI
{
    // Keep the menu effect in screen space even when the gameplay camera fits another map.
    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    public sealed class MenuAtmosphere : MonoBehaviour
    {
        [SerializeField] private Transform _particlesRoot;
        [SerializeField] private ParticleSystem[] _particles;
        [SerializeField] private float _bottomOffset = -.17f;
        [SerializeField, Min(1f)] private float _cameraDistance = 10f;

        private Canvas _backgroundCanvas;
        private float[] _shapeWidths;

        private void Awake()
        {
            _backgroundCanvas = GetComponentInParent<Canvas>();
            _shapeWidths = new float[_particles.Length];
            for (var i = 0; i < _particles.Length; i++)
            {
                var system = _particles[i];
                _shapeWidths[i] = system.shape.scale.x;
                var main = system.main;
                main.useUnscaledTime = true;
                main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            }
        }

        private void OnEnable()
        {
            FitToCamera();
            foreach (var system in _particles)
                if (!system.isPlaying) system.Play(false);
        }

        private void LateUpdate() => FitToCamera();

        private void FitToCamera()
        {
            var camera = _backgroundCanvas != null ? _backgroundCanvas.worldCamera : null;
            if (camera == null) camera = Camera.main;
            if (camera == null || _particlesRoot == null) return;

            var distance = Mathf.Max(_cameraDistance, camera.nearClipPlane + 1f);
            var bottom = camera.ViewportToWorldPoint(new Vector3(.5f, 0f, distance));
            var top = camera.ViewportToWorldPoint(new Vector3(.5f, 1f, distance));
            var height = Vector3.Distance(bottom, top);
            _particlesRoot.position = camera.ViewportToWorldPoint(new Vector3(.5f, _bottomOffset, distance));

            // The source effect was authored for a ten-unit-high camera, with scale (7, 7, 1).
            // Compensate for CanvasScaler instead of inheriting its resolution-dependent scale.
            var parentScale = _particlesRoot.parent.lossyScale;
            _particlesRoot.localScale = new Vector3(
                height * .7f / parentScale.x,
                height * .7f / parentScale.y,
                height * .1f / parentScale.z);

            var widthScale = camera.aspect / (16f / 9f);
            for (var i = 0; i < _particles.Length; i++)
            {
                var shape = _particles[i].shape;
                var size = shape.scale;
                size.x = _shapeWidths[i] * widthScale;
                shape.scale = size;
                var renderer = _particles[i].GetComponent<ParticleSystemRenderer>();
                if (_backgroundCanvas == null) continue;
                renderer.sortingLayerID = _backgroundCanvas.sortingLayerID;
                renderer.sortingOrder = _backgroundCanvas.sortingOrder + 1;
            }
        }
    }
}
