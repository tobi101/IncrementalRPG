using System;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

namespace UI
{
    [DisallowMultipleComponent]
    public sealed class LevelTransitionLampCounterView : MonoBehaviour
    {
        public event Action TurnOnAnimationStarted;
        public event Action TurnOnAnimationCompleted;

        [SerializeField] private RectTransform _container;
        [SerializeField] private SkeletonGraphic _lampPrefab;
        [SerializeField] private float _spacing = -8f;
        [SerializeField, Min(0f)] private float _horizontalPadding = 40f;
        [SerializeField, Min(0f)] private float _verticalPadding = 24f;
        [SerializeField] private string _idleOffAnimationName = "idle_off";
        [SerializeField] private string _turnOnAnimationName = "on";
        [SerializeField] private string _idleOnAnimationName = "idle_on";

        private readonly List<SkeletonGraphic> _lamps = new();

        private int _lampCount;
        private int _newlyCompletedLevelIndex = -1;
        private bool _isPaused;

        public void Prepare(int levelCount, int newlyCompletedLevelIndex)
        {
            _container ??= transform as RectTransform;

            if (_container == null || _lampPrefab == null)
            {
                Debug.LogError("Level transition lamp counter is missing its container or lamp prefab.", this);
                return;
            }

            _lampCount = Mathf.Max(0, levelCount);
            _newlyCompletedLevelIndex = newlyCompletedLevelIndex >= 0 &&
                                            newlyCompletedLevelIndex < _lampCount
                ? newlyCompletedLevelIndex
                : -1;

            EnsureLampCount(_lampCount);
            LayoutLamps();
            ApplyPreparedStates();
        }

        public void PlayNewlyCompleted()
        {
            if (_newlyCompletedLevelIndex < 0 || _newlyCompletedLevelIndex >= _lamps.Count)
                return;

            PlayTurnOn(_lamps[_newlyCompletedLevelIndex]);
        }

        public void SetPaused(bool isPaused)
        {
            _isPaused = isPaused;

            foreach (var lamp in _lamps)
            {
                if (lamp != null)
                    lamp.timeScale = isPaused ? 0f : 1f;
            }
        }

        private void EnsureLampCount(int count)
        {
            while (_lamps.Count < count)
            {
                var lamp = Instantiate(_lampPrefab, _container, false);
                lamp.name = $"Level Lamp {_lamps.Count + 1}";
                lamp.gameObject.layer = gameObject.layer;
                lamp.raycastTarget = false;
                lamp.timeScale = _isPaused ? 0f : 1f;
                _lamps.Add(lamp);
            }

            for (var i = 0; i < _lamps.Count; i++)
                _lamps[i].gameObject.SetActive(i < count);
        }

        private void LayoutLamps()
        {
            if (_lampCount <= 0 || _container == null || _lampPrefab == null)
                return;

            var prefabRect = _lampPrefab.rectTransform;
            var preferredWidth = Mathf.Max(1f, prefabRect.rect.width);
            var preferredHeight = Mathf.Max(1f, prefabRect.rect.height);
            var preferredStep = Mathf.Max(1f, preferredWidth + _spacing);
            var preferredTotalWidth = preferredWidth + (_lampCount - 1) * preferredStep;
            var availableWidth = Mathf.Max(1f, _container.rect.width - _horizontalPadding * 2f);
            var availableHeight = Mathf.Max(1f, _container.rect.height - _verticalPadding * 2f);
            var scale = Mathf.Min(
                1f,
                availableWidth / preferredTotalWidth,
                availableHeight / preferredHeight);

            var lampWidth = preferredWidth * scale;
            var lampHeight = preferredHeight * scale;
            var step = preferredStep * scale;
            var totalWidth = lampWidth + (_lampCount - 1) * step;
            var firstX = -totalWidth * 0.5f + lampWidth * 0.5f;

            for (var i = 0; i < _lampCount; i++)
            {
                var rect = _lamps[i].rectTransform;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = new Vector2(lampWidth, lampHeight);
                rect.anchoredPosition = new Vector2(firstX + i * step, 0f);
            }
        }

        private void ApplyPreparedStates()
        {
            for (var i = 0; i < _lampCount; i++)
            {
                var animationName = _newlyCompletedLevelIndex >= 0 && i < _newlyCompletedLevelIndex
                    ? _idleOnAnimationName
                    : _idleOffAnimationName;

                PlayStableAnimation(_lamps[i], animationName);
            }
        }

        private void PlayTurnOn(SkeletonGraphic lamp)
        {
            if (!TryPrepareLamp(lamp) ||
                !HasAnimation(lamp, _turnOnAnimationName) ||
                !HasAnimation(lamp, _idleOnAnimationName))
            {
                PlayStableAnimation(lamp, _idleOnAnimationName);
                return;
            }

            ResetLampPose(lamp);

            var turnOnEntry = lamp.AnimationState.SetAnimation(
                0,
                _turnOnAnimationName,
                false);
            turnOnEntry.MixDuration = 0f;
            turnOnEntry.Complete += _ => TurnOnAnimationCompleted?.Invoke();
            TurnOnAnimationStarted?.Invoke();

            var idleOnEntry = lamp.AnimationState.AddAnimation(
                0,
                _idleOnAnimationName,
                true,
                0f);
            idleOnEntry.SetMixDuration(0f, 0f);
        }

        private void PlayStableAnimation(SkeletonGraphic lamp, string animationName)
        {
            if (!TryPrepareLamp(lamp) || !HasAnimation(lamp, animationName))
                return;

            ResetLampPose(lamp);

            var entry = lamp.AnimationState.SetAnimation(0, animationName, true);
            entry.MixDuration = 0f;
        }

        private static bool TryPrepareLamp(SkeletonGraphic lamp)
        {
            if (lamp == null)
                return false;

            if (!lamp.IsValid)
                lamp.Initialize(false);

            return lamp.IsValid &&
                   lamp.Skeleton != null &&
                   lamp.AnimationState != null;
        }

        private static bool HasAnimation(SkeletonGraphic lamp, string animationName) =>
            !string.IsNullOrEmpty(animationName) &&
            lamp.Skeleton.Data.FindAnimation(animationName) != null;

        private static void ResetLampPose(SkeletonGraphic lamp)
        {
            lamp.AnimationState.ClearTracks();
            lamp.Skeleton.SetToSetupPose();
        }
    }
}
