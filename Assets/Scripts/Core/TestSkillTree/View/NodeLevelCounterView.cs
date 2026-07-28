using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace Core.TestSkillTree.View
{
    [DisallowMultipleComponent]
    public sealed class NodeLevelCounterView : MonoBehaviour
    {
        [SerializeField] private RectTransform _container;
        [SerializeField] private HorizontalLayoutGroup _layoutGroup;
        [SerializeField] private SkeletonGraphic _lampPrefab;
        [SerializeField] private string _idleOffAnimationName = "idle_off";
        [SerializeField] private string _turnOnAnimationName = "on";
        [SerializeField] private string _idleOnAnimationName = "idle_on";

        private readonly List<SkeletonGraphic> _lamps = new();

        private int _maxLevel;
        private int _currentLevel;
        private bool _initialized;

        public void Initialize(int maxLevel, int currentLevel)
        {
            _container ??= transform as RectTransform;
            _layoutGroup ??= GetComponent<HorizontalLayoutGroup>();

            if (_container == null || _lampPrefab == null)
            {
                Debug.LogError("Node level counter is missing its container or lamp prefab.", this);
                return;
            }

            _maxLevel = Mathf.Max(0, maxLevel);
            EnsureLampCount(_maxLevel);
            ResizeContainer(_maxLevel);

            _initialized = true;
            SetLevelImmediate(currentLevel);
        }

        public void PlayUpgrade(int newLevel)
        {
            if (!_initialized)
                return;

            var targetLevel = Mathf.Clamp(newLevel, 0, _maxLevel);
            if (targetLevel == _currentLevel)
                return;

            if (targetLevel < _currentLevel ||
                !isActiveAndEnabled ||
                !gameObject.activeInHierarchy)
            {
                SetLevelImmediate(targetLevel);
                return;
            }

            var previousLevel = _currentLevel;
            _currentLevel = targetLevel;

            for (var i = previousLevel; i < targetLevel; i++)
                PlayTurnOn(_lamps[i]);
        }

        public void SetLevelImmediate(int level)
        {
            if (!_initialized)
                return;

            _currentLevel = Mathf.Clamp(level, 0, _maxLevel);

            for (var i = 0; i < _maxLevel; i++)
            {
                var animationName = i < _currentLevel
                    ? _idleOnAnimationName
                    : _idleOffAnimationName;

                PlayStableAnimation(_lamps[i], animationName);
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
                _lamps.Add(lamp);
            }

            for (var i = 0; i < _lamps.Count; i++)
                _lamps[i].gameObject.SetActive(i < count);
        }

        private void ResizeContainer(int lampCount)
        {
            if (_layoutGroup == null)
                return;

            var lampRect = _lampPrefab.rectTransform;
            var lampWidth = lampRect.rect.width;
            if (lampWidth <= 0f)
                lampWidth = lampRect.sizeDelta.x;

            var width = lampCount > 0
                ? lampCount * lampWidth + (lampCount - 1) * _layoutGroup.spacing
                : 0f;

            _container.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Horizontal,
                Mathf.Max(0f, width));

            LayoutRebuilder.ForceRebuildLayoutImmediate(_container);
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

        private void OnDisable()
        {
            if (_initialized)
                SetLevelImmediate(_currentLevel);
        }
    }
}
