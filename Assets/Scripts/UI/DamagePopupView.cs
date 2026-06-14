using TMPro;
using UnityEngine;
using Utils;

namespace UI
{
    [DisallowMultipleComponent]
    public sealed class DamagePopupView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;

        private bool _missingTextWarningLogged;
        private bool _isPlaying;
        private Vector3 _startPosition;
        private Vector3 _endPosition;
        private float _duration;
        private float _elapsed;

        private void Reset()
        {
            _text = GetComponentInChildren<TMP_Text>(true);
        }

        private void Awake()
        {
            EnsureText();
        }

        public bool Show(BigDouble amount, Vector3 startPosition, float duration, float moveDistance)
        {
            if (!EnsureText())
            {
                if (!_missingTextWarningLogged)
                {
                    Debug.LogWarning($"[{nameof(DamagePopupView)}] TMP_Text is not assigned on '{name}'.", this);
                    _missingTextWarningLogged = true;
                }

                return false;
            }

            _startPosition = startPosition;
            _endPosition = startPosition + Vector3.up * moveDistance;
            _duration = Mathf.Max(0.01f, duration);
            _elapsed = 0f;
            _isPlaying = true;

            _text.text = BigDoubleFormatter.Format(amount, 0, 2);
            transform.position = _startPosition;
            SetAlpha(1f);
            gameObject.SetActive(true);

            return true;
        }

        public bool Tick(float deltaTime)
        {
            if (!_isPlaying)
                return true;

            _elapsed += deltaTime;
            var t = Mathf.Clamp01(_elapsed / _duration);
            var moveT = 1f - (1f - t) * (1f - t);
            var fadeT = Mathf.InverseLerp(0.25f, 1f, t);

            transform.position = Vector3.LerpUnclamped(_startPosition, _endPosition, moveT);
            SetAlpha(1f - fadeT);

            if (_elapsed < _duration)
                return false;

            HideImmediately();
            return true;
        }

        public void HideImmediately()
        {
            _isPlaying = false;
            gameObject.SetActive(false);
        }

        private void SetAlpha(float alpha)
        {
            if (_text == null)
                return;

            var color = _text.color;
            color.a = alpha;
            _text.color = color;
        }

        private bool EnsureText()
        {
            if (_text == null)
                _text = GetComponentInChildren<TMP_Text>(true);

            return _text != null;
        }
    }
}
