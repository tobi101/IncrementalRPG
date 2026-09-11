using System;
using System.Collections;
using System.Collections.Generic;
using Core.Items;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [DisallowMultipleComponent]
    public sealed class LootboxView : MonoBehaviour
    {
        public event Action SpinCompleted;
        public event Action ContinueClicked;
        public event Action UseNowClicked;

        [Header("Chest")]
        [SerializeField] private SkeletonGraphic _chest;
        [SerializeField] private string _closedIdleAnimationName = "idle_close";
        [SerializeField] private string _openAnimationName = "open";
        [SerializeField] private string _openIdleAnimationName = "idle_open";

        [Header("Roll")]
        [SerializeField] private RectTransform _itemViewport;
        [SerializeField] private Sprite[] _ambientIcons;
        [SerializeField] private Vector2 _itemSize = new(150f, 150f);
        [SerializeField, Min(1f)] private float _itemSpacing = 190f;
        [SerializeField, Min(1f)] private float _spinSpeed = 2200f;
        [SerializeField, Min(0f), Tooltip("Minimum opening time. The reel also waits for the Spine animation to finish.")]
        private float _spinStartDelay = 0.6f;
        [SerializeField, Min(0f)] private float _constantSpinDuration = 1.8f;
        [SerializeField, Min(0.01f)] private float _settleDuration = 1.2f;
        [SerializeField, Min(0f)] private float _winnerEntryPadding = 40f;

        [Header("Result")]
        [SerializeField] private LootRewardPopupView _resultPopupPrefab;
        [SerializeField] private RectTransform _resultPopupParent;
        [SerializeField] private GameObject[] _transitionLabels;

        private readonly List<Image> _itemViews = new();
        private LootReward _reward;
        private LootRewardPopupView _resultPopup;
        private int _finalItemCount;
        private bool _prepared;
        private bool _spinStarted;
        private bool _spinFinished;
        private Coroutine _spinRoutine;
        private int _rollingItemCount;
        private bool _isPaused;
        private Spine.TrackEntry _openingEntry;
        private bool _openingCompleted;

        private void Awake()
        {
            _resultPopup = Instantiate(_resultPopupPrefab, _resultPopupParent, false);
            _resultPopup.ContinueClicked += HandleContinueClicked;
            _resultPopup.UseNowClicked += HandleUseNowClicked;
            // Prepare can run before Awake while the transition curtain is inactive.
            if (!_prepared)
                ResetView();
        }

        private void OnDisable()
        {
            CancelOpening();
            if (_spinRoutine != null)
                StopCoroutine(_spinRoutine);

            _spinRoutine = null;
            _resultPopup?.Hide();
        }

        private void OnDestroy()
        {
            CancelOpening();
            if (_resultPopup == null)
                return;

            _resultPopup.ContinueClicked -= HandleContinueClicked;
            _resultPopup.UseNowClicked -= HandleUseNowClicked;
            Destroy(_resultPopup.gameObject);
        }

        public void Prepare(LootReward reward)
        {
            ResetView();
            _reward = reward;
            _prepared = true;
        }

        public void PlayOpen()
        {
            if (!_prepared || _spinStarted)
                return;

            _spinStarted = true;
            _spinRoutine = StartCoroutine(OpenAndSpinRoutine());
        }

        public void ShowResult(bool canUse, string status)
        {
            if (!_spinFinished)
                return;

            foreach (var label in _transitionLabels)
                label.SetActive(false);
            _resultPopup.Show(_reward, canUse, status);
            _resultPopup.SetPaused(_isPaused);
        }

        public void HideResult() => _resultPopup.Hide();

        public void ShowUseUnavailable(string status) => _resultPopup.ShowUseUnavailable(status);

        public void SetPaused(bool isPaused)
        {
            _isPaused = isPaused;
            _resultPopup?.SetPaused(isPaused);
            _chest.timeScale = isPaused ? 0f : 1f;
        }

        public void ResetView()
        {
            CancelOpening();
            if (_spinRoutine != null)
                StopCoroutine(_spinRoutine);

            _spinRoutine = null;
            _prepared = false;
            _spinStarted = false;
            _spinFinished = false;
            _resultPopup?.Hide();
            _chest.gameObject.SetActive(true);
            foreach (var label in _transitionLabels)
                label.SetActive(true);

            foreach (var itemView in _itemViews)
                itemView.gameObject.SetActive(false);

            PrepareChest();
            var idleEntry = _chest.AnimationState.SetAnimation(0, _closedIdleAnimationName, true);
            idleEntry.MixDuration = 0f;
        }

        private void HandleOpeningCompleted(Spine.TrackEntry entry)
        {
            if (!ReferenceEquals(entry, _openingEntry))
                return;

            _openingEntry.Complete -= HandleOpeningCompleted;
            _openingEntry = null;
            _openingCompleted = true;
            var idleEntry = _chest.AnimationState.SetAnimation(0, _openIdleAnimationName, true);
            idleEntry.MixDuration = 0f;
        }

        private void CancelOpening()
        {
            if (_openingEntry != null)
                _openingEntry.Complete -= HandleOpeningCompleted;
            _openingEntry = null;
            _openingCompleted = false;
        }

        private IEnumerator OpenAndSpinRoutine()
        {
            PrepareChest();
            _openingCompleted = false;
            _openingEntry = _chest.AnimationState.SetAnimation(0, _openAnimationName, false);
            _openingEntry.MixDuration = 0f;
            _openingEntry.Complete += HandleOpeningCompleted;

            var elapsed = 0f;
            while (!_openingCompleted || _isPaused || elapsed < _spinStartDelay)
            {
                elapsed += GetGameplayDeltaTime();
                yield return null;
            }

            // Give the fully opened pose a rendered frame before replacing the chest.
            yield return null;
            while (_isPaused)
                yield return null;

            _chest.gameObject.SetActive(false);
            EnsureItemPool();
            yield return SpinRoutine();
        }

        private IEnumerator SpinRoutine()
        {
            LayoutRollingItems();

            var elapsed = 0f;
            while (elapsed < _constantSpinDuration)
            {
                var deltaTime = GetGameplayDeltaTime();
                elapsed += deltaTime;
                MoveRollingItems(deltaTime);
                yield return null;
            }

            var outgoingStartPositions = CaptureRollingItemPositions();
            var winnerLeadOffset = CalculateWinnerLeadOffset(outgoingStartPositions);
            var brakingDistance = _spinSpeed * _settleDuration * 0.5f;
            var approachDistance = winnerLeadOffset - brakingDistance;
            PrepareFinalItems(winnerLeadOffset);

            elapsed = 0f;
            var approachDuration = approachDistance / _spinSpeed;
            while (elapsed < approachDuration)
            {
                elapsed += GetGameplayDeltaTime();
                var distance = Mathf.Min(approachDistance, elapsed * _spinSpeed);
                MoveOutgoingItems(outgoingStartPositions, distance);
                PositionFinalItems(winnerLeadOffset - distance);
                yield return null;
            }

            var outgoingBrakingPositions = CaptureRollingItemPositions();
            elapsed = 0f;

            while (elapsed < _settleDuration)
            {
                elapsed += GetGameplayDeltaTime();
                var t = Mathf.Clamp01(elapsed / _settleDuration);
                var remainingDistance = brakingDistance * (1f - t) * (1f - t);
                var traveledDistance = brakingDistance - remainingDistance;
                MoveOutgoingItems(outgoingBrakingPositions, traveledDistance);
                PositionFinalItems(remainingDistance);
                yield return null;
            }

            for (var i = 0; i < _rollingItemCount; i++)
                _itemViews[i].gameObject.SetActive(false);

            PositionFinalItems(0f);
            _spinFinished = true;

            _spinRoutine = null;
            SpinCompleted?.Invoke();
        }

        private void EnsureItemPool()
        {
            _rollingItemCount = Mathf.CeilToInt(_itemViewport.rect.width / _itemSpacing) + 3;
            _finalItemCount = 2 * Mathf.CeilToInt((_itemViewport.rect.width * 0.5f + _itemSize.x * 0.5f) / _itemSpacing) + 1;
            var requiredCount = _rollingItemCount + _finalItemCount;

            while (_itemViews.Count < requiredCount)
            {
                var itemObject = new GameObject(
                    $"Loot Roll Item {_itemViews.Count + 1}",
                    typeof(RectTransform),
                    typeof(CanvasRenderer),
                    typeof(Image));
                itemObject.layer = gameObject.layer;

                var rect = itemObject.GetComponent<RectTransform>();
                rect.SetParent(_itemViewport, false);
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = _itemSize;

                var image = itemObject.GetComponent<Image>();
                image.preserveAspect = true;
                image.raycastTarget = false;
                _itemViews.Add(image);
            }
        }

        private void LayoutRollingItems()
        {
            foreach (var itemView in _itemViews)
                itemView.gameObject.SetActive(false);

            var left = _itemViewport.rect.xMin - _itemSize.x * 0.5f;

            for (var i = 0; i < _rollingItemCount; i++)
            {
                var itemView = _itemViews[i];
                itemView.gameObject.SetActive(true);
                itemView.sprite = GetAmbientIcon();
                itemView.rectTransform.anchoredPosition = new Vector2(left + i * _itemSpacing, 0f);
            }
        }

        private void MoveRollingItems(float deltaTime)
        {
            var left = _itemViewport.rect.xMin - _itemSize.x * 0.5f;
            var rightMost = float.MinValue;

            for (var i = 0; i < _rollingItemCount; i++)
            {
                var itemView = _itemViews[i];
                var position = itemView.rectTransform.anchoredPosition;
                position.x -= _spinSpeed * deltaTime;
                itemView.rectTransform.anchoredPosition = position;
                rightMost = Mathf.Max(rightMost, position.x);
            }

            for (var i = 0; i < _rollingItemCount; i++)
            {
                var itemView = _itemViews[i];
                var position = itemView.rectTransform.anchoredPosition;
                if (position.x < left)
                {
                    rightMost += _itemSpacing;
                    position.x = rightMost;
                    itemView.sprite = GetAmbientIcon();
                    itemView.rectTransform.anchoredPosition = position;
                }
            }
        }

        private Vector2[] CaptureRollingItemPositions()
        {
            var positions = new Vector2[_rollingItemCount];
            for (var i = 0; i < _rollingItemCount; i++)
                positions[i] = _itemViews[i].rectTransform.anchoredPosition;

            return positions;
        }

        private float CalculateWinnerLeadOffset(IReadOnlyList<Vector2> outgoingPositions)
        {
            var leftmostFinalX = GetFinalPosition(0).x;
            var rightEntryX = _itemViewport.rect.xMax + _itemSize.x * 0.5f + _winnerEntryPadding;
            var entryOffset = rightEntryX - leftmostFinalX;

            var rightmostOutgoingX = float.MinValue;
            foreach (var position in outgoingPositions)
                rightmostOutgoingX = Mathf.Max(rightmostOutgoingX, position.x);

            var leftExitX = _itemViewport.rect.xMin - _itemSize.x * 0.5f;
            var outgoingExitDistance = rightmostOutgoingX - leftExitX + _itemSpacing;
            var continuousEntryOffset = rightmostOutgoingX + _itemSpacing - leftmostFinalX;
            var brakingDistance = _spinSpeed * _settleDuration * 0.5f;
            return Mathf.Max(entryOffset, outgoingExitDistance, continuousEntryOffset, brakingDistance);
        }

        private void PrepareFinalItems(float leadOffset)
        {
            for (var i = 0; i < _finalItemCount; i++)
            {
                var itemView = GetFinalView(i);
                itemView.sprite = i == _finalItemCount / 2 ? _reward.Definition.icon : GetAmbientIcon();
                itemView.rectTransform.anchoredPosition = GetFinalPosition(i) + Vector2.right * leadOffset;
                itemView.gameObject.SetActive(true);
            }
        }

        private void MoveOutgoingItems(IReadOnlyList<Vector2> startPositions, float distance)
        {
            for (var i = 0; i < _rollingItemCount; i++)
                _itemViews[i].rectTransform.anchoredPosition =
                    startPositions[i] + Vector2.left * distance;
        }

        private void PositionFinalItems(float offset)
        {
            for (var i = 0; i < _finalItemCount; i++)
                GetFinalView(i).rectTransform.anchoredPosition =
                    GetFinalPosition(i) + Vector2.right * offset;
        }

        private Vector2 GetFinalPosition(int index) =>
            new(_itemViewport.rect.center.x + (index - _finalItemCount / 2) * _itemSpacing, 0f);

        private Image GetFinalView(int index) => _itemViews[_rollingItemCount + index];

        private Sprite GetAmbientIcon()
        {
            if (_ambientIcons == null || _ambientIcons.Length == 0)
                return _reward.Definition.icon;

            return _ambientIcons[UnityEngine.Random.Range(0, _ambientIcons.Length)];
        }

        private void HandleContinueClicked()
        {
            if (_spinFinished && !_isPaused)
                ContinueClicked?.Invoke();
        }

        private void HandleUseNowClicked()
        {
            if (_spinFinished && !_isPaused)
                UseNowClicked?.Invoke();
        }

        private void PrepareChest()
        {
            if (!_chest.IsValid)
                _chest.Initialize(false);

            _chest.timeScale = _isPaused ? 0f : 1f;
            _chest.AnimationState.ClearTracks();
            _chest.Skeleton.SetToSetupPose();
        }

        private float GetGameplayDeltaTime()
        {
            return _isPaused ? 0f : Time.deltaTime;
        }
    }
}
