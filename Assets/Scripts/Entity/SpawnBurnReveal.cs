using UnityEngine;

namespace Entity
{
    [RequireComponent(typeof(MeshRenderer))]
    public sealed class SpawnBurnReveal : MonoBehaviour
    {
        private static readonly int FadeAmount = Shader.PropertyToID("_FadeAmount");
        private const float Visible = -0.1f;
        [SerializeField, Min(0.01f)] private float _duration = 1.5f;
        private MeshRenderer _renderer;
        private MaterialPropertyBlock _properties;
        private float _elapsed;

        private void Awake()
        {
            _renderer = GetComponent<MeshRenderer>();
            _properties = new MaterialPropertyBlock();
        }

        public void Begin()
        {
            _elapsed = 0f;
            SetFade(1f);
        }

        public void Tick(float deltaTime)
        {
            if (_elapsed >= _duration) return;
            _elapsed = Mathf.Min(_duration, _elapsed + deltaTime);
            SetFade(Mathf.SmoothStep(1f, Visible, _elapsed / _duration));
        }

        public void Finish()
        {
            _elapsed = _duration;
            SetFade(Visible);
        }

        private void SetFade(float amount)
        {
            _renderer.GetPropertyBlock(_properties);
            _properties.SetFloat(FadeAmount, amount);
            _renderer.SetPropertyBlock(_properties);
        }
    }
}
