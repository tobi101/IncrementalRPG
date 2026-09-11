using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [DisallowMultipleComponent]
    public sealed class RewardRevealEffect : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private RewardRaysGraphic _rays;
        [SerializeField] private RewardRaysGraphic _secondaryRays;
        [SerializeField] private Image _halo;
        [SerializeField] private RectTransform _particleRoot;
        [SerializeField] private Sprite _mistSprite;
        [SerializeField] private Sprite _sparkSprite;

        [Header("Appearance")]
        [SerializeField] private Color _accent = new Color(217f / 255f, 82f / 255f, 181f / 255f);
        [SerializeField, Range(.5f, 1.5f)] private float _intensity = 1f;
        [SerializeField, Range(20f, 90f)] private float _rayRevolutionSeconds = 48f;
        [SerializeField, Range(.3f, 1.2f)] private float _appearanceDuration = .6f;
        [SerializeField, Range(0f, 12f)] private float _floatAmplitude = 5.5f;

        private const int CloudCount = 22;
        private const int SparkCount = 8;
        private const int BurstCount = 30;
        private static readonly Color Warm = new Color(1f, 237f / 255f, 184f / 255f);

        private struct Particle
        {
            public Image Image;
            public RectTransform Rect;
            public float X, Offset, Duration, Size, Phase, Stretch;
        }

        private Particle[] _clouds;
        private Particle[] _sparks;
        private Particle[] _burst;
        private Particle _flash, _core, _floor;
        private Vector3 _iconScale, _haloScale;
        private Vector2 _iconPosition;
        private Quaternion _iconRotation;
        private Color _iconColor;
        private float _elapsed;
        private bool _running, _paused, _applicationPaused;
        private uint _seed = 31;

        private void Update()
        {
            if (Application.isFocused)
                Advance(Time.unscaledDeltaTime);
        }

        private void OnApplicationPause(bool paused) => _applicationPaused = paused;
        private void OnDisable() => Stop();

        public void Play(bool paused)
        {
            EnsurePool();
            _elapsed = 0f;
            _paused = paused;
            _running = true;
            _particleRoot.gameObject.SetActive(true);
            ApplyFrame();
        }

        public void SetPaused(bool paused) => _paused = paused;

        public void Stop()
        {
            _running = false;
            _elapsed = 0f;
            if (_clouds == null)
                return;

            _icon.rectTransform.localScale = _iconScale;
            _icon.rectTransform.anchoredPosition = _iconPosition;
            _icon.rectTransform.localRotation = _iconRotation;
            _icon.color = _iconColor;
            _halo.rectTransform.localScale = _haloScale;
            _rays.rectTransform.localRotation = Quaternion.identity;
            _secondaryRays.rectTransform.localRotation = Quaternion.identity;
            _particleRoot.gameObject.SetActive(false);
        }

        private void Advance(float deltaTime)
        {
            if (!_running || _paused || _applicationPaused || !isActiveAndEnabled)
                return;

            // A stalled/background frame must not jump the reveal or the drifting mist.
            _elapsed += Mathf.Clamp(deltaTime, 0f, .05f);
            ApplyFrame();
        }

        private void EnsurePool()
        {
            if (_clouds != null)
                return;

            _iconScale = _icon.rectTransform.localScale;
            _iconPosition = _icon.rectTransform.anchoredPosition;
            _iconRotation = _icon.rectTransform.localRotation;
            _iconColor = _icon.color;
            _haloScale = _halo.rectTransform.localScale;
            _icon.raycastTarget = _halo.raycastTarget = false;
            _rays.raycastTarget = _secondaryRays.raycastTarget = false;

            // Allocate once per popup. Reopening reuses every Image and its particle data.
            // A private seed avoids consuming the gameplay/loot random sequence.
            _core = CreateParticle("Warm core", _mistSprite);
            _floor = CreateParticle("Soft base glow", _mistSprite);
            _clouds = new Particle[CloudCount];
            for (var i = 0; i < _clouds.Length; i++)
            {
                var p = CreateParticle("Mist " + i, _mistSprite);
                p.X = (NextRandom() - .5f) * .48f;
                p.Offset = NextRandom();
                p.Duration = 7f + NextRandom() * 6f;
                p.Size = .065f + NextRandom() * .045f;
                p.Phase = NextRandom() * Mathf.PI * 2f;
                p.Stretch = 1.25f + NextRandom() * .8f;
                _clouds[i] = p;
            }

            _sparks = new Particle[SparkCount];
            for (var i = 0; i < _sparks.Length; i++)
            {
                var p = CreateParticle("Drifting spark " + i, _sparkSprite);
                p.X = (NextRandom() - .5f) * .62f;
                p.Offset = NextRandom();
                p.Duration = 3.4f + NextRandom() * 4.4f;
                p.Size = .65f + NextRandom() * 1.45f;
                p.Phase = NextRandom() * Mathf.PI * 2f;
                _sparks[i] = p;
            }

            _flash = CreateParticle("Reveal light", _mistSprite);
            _burst = new Particle[BurstCount];
            for (var i = 0; i < _burst.Length; i++)
            {
                var p = CreateParticle("Reveal spark " + i, _sparkSprite);
                p.Phase = NextRandom() * Mathf.PI * 2f;
                p.X = .18f + NextRandom() * .23f;
                p.Size = .7f + NextRandom() * 1.3f;
                p.Duration = .65f + NextRandom() * .55f;
                _burst[i] = p;
            }
        }

        private Particle CreateParticle(string particleName, Sprite sprite)
        {
            var go = new GameObject(particleName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.layer = _particleRoot.gameObject.layer;
            var rect = (RectTransform)go.transform;
            rect.SetParent(_particleRoot, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            var image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            return new Particle { Image = image, Rect = rect };
        }

        private float NextRandom()
        {
            _seed = (uint)((ulong)_seed * 16807 % 2147483647);
            return (_seed - 1f) / 2147483646f;
        }

        private void ApplyFrame()
        {
            var breath = .5f - .5f * Mathf.Cos(_elapsed * Mathf.PI * 2f / 3.4f);
            var reveal = Mathf.Clamp01(_elapsed / Mathf.Max(.01f, _appearanceDuration));
            // The approved 600 ms reveal: .76 -> 1.045 -> 1, with a gentle upward arrival.
            var arrival = 1f - Mathf.Pow(1f - Mathf.Clamp01(reveal / .65f), 3f);
            var settle = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.65f, 1f, reveal));
            var iconScale = reveal < .65f ? Mathf.Lerp(.76f, 1.045f, arrival) : Mathf.Lerp(1.045f, 1f, settle);
            var floatPhase = _elapsed * Mathf.PI * 2f / 3.8f;
            var floatBlend = Mathf.SmoothStep(0f, 1f, reveal);
            var y = reveal < .65f ? Mathf.Lerp(-24f, 3f, arrival) : Mathf.Lerp(3f, 0f, settle);
            _icon.rectTransform.localScale = _iconScale * iconScale;
            _icon.rectTransform.anchoredPosition = _iconPosition + Vector2.up * (y - Mathf.Cos(floatPhase) * _floatAmplitude * floatBlend);
            _icon.rectTransform.localRotation = _iconRotation * Quaternion.Euler(0f, 0f, -.7f * Mathf.Cos(floatPhase) * floatBlend);
            _icon.color = WithAlpha(_iconColor, _iconColor.a * arrival);

            _rays.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -_elapsed * 360f / Mathf.Max(1f, _rayRevolutionSeconds));
            _secondaryRays.rectTransform.localRotation = Quaternion.Euler(0f, 0f, _elapsed * 360f / 73f);
            _rays.color = WithAlpha(_accent, .098f * _intensity);
            _secondaryRays.color = WithAlpha(Warm, .043f * _intensity);
            _halo.rectTransform.localScale = _haloScale * Mathf.Lerp(.97f, 1.055f, breath);
            _halo.color = WithAlpha(_accent, Mathf.Lerp(.24f, .33f, breath) * _intensity);

            var size = _particleRoot.rect.size;
            var width = size.x;
            var height = size.y;
            var pixelScale = width / 650f;
            Draw(_core, Vector2.zero, Vector2.one * width * .35f, 0f, WithAlpha(Warm, .16f * _intensity));
            Draw(_floor, new Vector2(0f, -height * .22f), new Vector2(width * .27f, height * .04f), 0f,
                WithAlpha(Warm, .14f * _intensity));

            for (var i = 0; i < _clouds.Length; i++)
            {
                var p = _clouds[i];
                var age = Mathf.Repeat(_elapsed / p.Duration + p.Offset, 1f);
                var fade = Mathf.Pow(Mathf.Sin(age * Mathf.PI), 1.4f);
                var x = width * (p.X + Mathf.Sin(_elapsed * .18f + p.Phase + age * 3f) * .065f);
                var yPos = height * (-.24f + age * .43f - Mathf.Cos(_elapsed * .19f + p.Phase) * .02f);
                var radius = width * p.Size * (.65f + age * 1.45f);
                Draw(p, new Vector2(x, yPos), new Vector2(radius * 2f * p.Stretch, radius * 2f * .78f),
                    -(p.Phase + _elapsed * .045f) * Mathf.Rad2Deg, WithAlpha(_accent, fade * .127f * _intensity));
            }

            for (var i = 0; i < _sparks.Length; i++)
            {
                var p = _sparks[i];
                var age = Mathf.Repeat(_elapsed / p.Duration + p.Offset, 1f);
                var opacity = Mathf.Pow(Mathf.Sin(age * Mathf.PI), 1.3f) * (.47f + .13f * Mathf.Sin(_elapsed * 2.3f + p.Phase));
                var position = new Vector2(width * (p.X + Mathf.Sin(_elapsed * .8f + p.Phase) * .017f), height * (-.27f + age * .47f));
                Draw(p, position, Vector2.one * (p.Size * pixelScale * 12f), 0f, WithAlpha(Warm, opacity * _intensity));
            }

            Draw(_flash, Vector2.zero, Vector2.one * (width * (.065f + _elapsed * .27f) * 2f), 0f,
                WithAlpha(Warm, Mathf.Max(0f, 1f - _elapsed / .8f) * .32f * _intensity));
            for (var i = 0; i < _burst.Length; i++)
            {
                var p = _burst[i];
                var age = Mathf.Clamp01(_elapsed / p.Duration);
                var distance = width * p.X * (1f - (1f - age) * (1f - age));
                var position = new Vector2(Mathf.Cos(p.Phase) * distance, -Mathf.Sin(p.Phase) * distance * .75f + _elapsed * 10f * pixelScale);
                Draw(p, position, Vector2.one * (p.Size * pixelScale * 12f), 0f,
                    WithAlpha(Warm, age < 1f ? Mathf.Sin(Mathf.PI * age) * .8f * _intensity : 0f));
            }
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private static void Draw(Particle particle, Vector2 position, Vector2 size, float rotation, Color color)
        {
            var visible = color.a > .001f;
            particle.Image.enabled = visible;
            if (!visible)
                return;

            particle.Rect.anchoredPosition = position;
            particle.Rect.sizeDelta = size;
            particle.Rect.localRotation = Quaternion.Euler(0f, 0f, rotation);
            particle.Image.color = color;
        }
    }
}
