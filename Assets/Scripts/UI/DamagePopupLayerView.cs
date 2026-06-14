using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace UI
{
    public sealed class DamagePopupLayerView : MonoBehaviour
    {
        private enum OverflowMode
        {
            ReuseOldest,
            SkipNewest
        }

        [SerializeField] private DamagePopupView _popupPrefab;
        [SerializeField] private Transform _poolRoot;
        [SerializeField, Min(1)] private int _poolSize = 64;
        [SerializeField] private OverflowMode _overflowMode = OverflowMode.ReuseOldest;
        [SerializeField, Min(0.01f)] private float _duration = 0.85f;
        [SerializeField, Min(0f)] private float _moveDistance = 0.35f;
        [SerializeField, Min(0f)] private float _horizontalJitter = 0.08f;
        [SerializeField, Min(0f)] private float _verticalJitter = 0.02f;

        private readonly Queue<DamagePopupView> _inactive = new();
        private readonly List<DamagePopupView> _active = new();
        private bool _initialized;

        private void Update()
        {
            for (var i = _active.Count - 1; i >= 0; i--)
            {
                if (_active[i].Tick(Time.deltaTime))
                    ReturnPopupAt(i);
            }
        }

        public void InitializePool()
        {
            if (_initialized)
                return;

            _initialized = true;

            if (_poolRoot == null)
                _poolRoot = transform;

            if (_popupPrefab == null)
            {
                Debug.LogWarning($"[{nameof(DamagePopupLayerView)}] Popup prefab is not assigned. Damage popups are disabled.", this);
                return;
            }

            for (var i = 0; i < _poolSize; i++)
            {
                var popup = Instantiate(_popupPrefab, _poolRoot);
                popup.HideImmediately();
                _inactive.Enqueue(popup);
            }
        }

        public void ShowDamage(BigDouble amount, Vector3 worldPosition)
        {
            if (amount <= BigDouble.Zero)
                return;

            if (!_initialized)
                InitializePool();

            if (_popupPrefab == null)
                return;

            var popup = TakePopup();
            if (popup == null)
                return;

            var startPosition = worldPosition + new Vector3(
                Random.Range(-_horizontalJitter, _horizontalJitter),
                Random.Range(-_verticalJitter, _verticalJitter),
                0f);

            if (!popup.Show(amount, startPosition, _duration, _moveDistance))
            {
                _inactive.Enqueue(popup);
                return;
            }

            _active.Add(popup);
        }

        private DamagePopupView TakePopup()
        {
            if (_inactive.Count > 0)
                return _inactive.Dequeue();

            if (_overflowMode == OverflowMode.SkipNewest || _active.Count == 0)
                return null;

            var popup = _active[0];
            _active.RemoveAt(0);
            popup.HideImmediately();
            return popup;
        }

        private void ReturnPopupAt(int activeIndex)
        {
            if (activeIndex < 0 || activeIndex >= _active.Count)
                return;

            var popup = _active[activeIndex];
            _active.RemoveAt(activeIndex);
            popup.HideImmediately();

            if (_poolRoot != null)
                popup.transform.SetParent(_poolRoot, false);

            _inactive.Enqueue(popup);
        }
    }
}
