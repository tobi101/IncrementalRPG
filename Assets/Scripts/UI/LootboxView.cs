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

        [Header("Chest")]
        [SerializeField] private SkeletonGraphic _chest;
        [SerializeField] private string _closedIdleAnimationName = "idle_close";
        [SerializeField] private string _openAnimationName = "open";
        [SerializeField] private string _openIdleAnimationName = "idle_open";

        [Header("Roll")]
        [SerializeField] private RectTransform _itemViewport;
        [SerializeField] private RectTransform[] _finalAnchors;
        [SerializeField] private Vector2 _itemSize = new(150f, 150f);
        [SerializeField, Min(1f)] private float _itemSpacing = 190f;
        [SerializeField, Min(1f)] private float _spinSpeed = 1100f;
        [SerializeField, Min(0f)] private float _spinStartDelay = 0.6f;
        [SerializeField, Min(0f)] private float _constantSpinDuration = 1.8f;
        [SerializeField, Min(0.01f)] private float _settleDuration = 1.2f;
        [SerializeField, Min(0f)] private float _winnerEntryPadding = 40f;

        [Header("Continue")]
        [SerializeField] private Button _continueButton;

        private readonly List<Image> _itemViews = new();
        private LootBatch _batch;
        private Coroutine _spinRoutine;
        private int _rollingItemCount;
        private bool _isPaused;

        private void Awake()
        {
            _continueButton.onClick.AddListener(HandleContinueClicked);
            ResetView();
        }

        private void OnDisable()
        {
            if (_spinRoutine != null)
                StopCoroutine(_spinRoutine);

            _spinRoutine = null;
        }

        private void OnDestroy()
        {
            _continueButton.onClick.RemoveListener(HandleContinueClicked);
        }

        public void Prepare(LootBatch batch)
        {
            _batch = batch;
            ResetView();
        }

        public void PlayOpen()
        {
            PrepareChest();
            var openEntry = _chest.AnimationState.SetAnimation(0, _openAnimationName, false);
            openEntry.MixDuration = 0f;
            openEntry.Complete += _ => BeginOpenIdle();
            _spinRoutine = StartCoroutine(BeginSpinAfterDelay());
        }

        public void ShowContinueButton()
        {
            _continueButton.gameObject.SetActive(true);
            _continueButton.interactable = true;
            _continueButton.transform.SetAsLastSibling();
        }

        public void SetPaused(bool isPaused)
        {
            _isPaused = isPaused;
            _chest.timeScale = isPaused ? 0f : 1f;
        }

        public void ResetView()
        {
            if (_spinRoutine != null)
                StopCoroutine(_spinRoutine);

            _spinRoutine = null;
            _continueButton.gameObject.SetActive(false);
            _continueButton.interactable = false;

            foreach (var itemView in _itemViews)
                itemView.gameObject.SetActive(false);

            PrepareChest();
            var idleEntry = _chest.AnimationState.SetAnimation(0, _closedIdleAnimationName, true);
            idleEntry.MixDuration = 0f;
        }

        private void BeginOpenIdle()
        {
            var idleEntry = _chest.AnimationState.SetAnimation(0, _openIdleAnimationName, true);
            idleEntry.MixDuration = 0f;
        }

        private IEnumerator BeginSpinAfterDelay()
        {
            var elapsed = 0f;
            while (elapsed < _spinStartDelay)
            {
                elapsed += GetGameplayDeltaTime();
                yield return null;
            }

            if (_batch.Rewards.Count == 0)
            {
                _spinRoutine = null;
                SpinCompleted?.Invoke();
                yield break;
            }

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
            PrepareWinnerItems(winnerLeadOffset);

            elapsed = 0f;
            var approachDuration = approachDistance / _spinSpeed;
            while (elapsed < approachDuration)
            {
                elapsed += GetGameplayDeltaTime();
                var distance = Mathf.Min(approachDistance, elapsed * _spinSpeed);
                MoveOutgoingItems(outgoingStartPositions, distance);
                PositionWinnerItems(winnerLeadOffset - distance);
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
                PositionWinnerItems(remainingDistance);
                yield return null;
            }

            for (var i = 0; i < _rollingItemCount; i++)
                _itemViews[i].gameObject.SetActive(false);

            for (var i = 0; i < _batch.Rewards.Count; i++)
                GetWinnerView(i).rectTransform.anchoredPosition = _finalAnchors[i].anchoredPosition;

            _spinRoutine = null;
            SpinCompleted?.Invoke();
        }

        private void EnsureItemPool()
        {
            _rollingItemCount = Mathf.CeilToInt(_itemViewport.rect.width / _itemSpacing) + 3;
            var requiredCount = _rollingItemCount + _batch.Rewards.Count;

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

            var right = _itemViewport.rect.xMax + _itemSize.x * 0.5f;

            for (var i = 0; i < _rollingItemCount; i++)
            {
                var itemView = _itemViews[i];
                itemView.gameObject.SetActive(true);
                itemView.sprite = GetRandomRewardIcon();
                itemView.rectTransform.anchoredPosition = new Vector2(right + i * _itemSpacing, 0f);
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
                    itemView.sprite = GetRandomRewardIcon();
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
            var leftmostAnchorX = float.MaxValue;
            foreach (var anchor in _finalAnchors)
                leftmostAnchorX = Mathf.Min(leftmostAnchorX, anchor.anchoredPosition.x);

            var rightEntryX = _itemViewport.rect.xMax + _itemSize.x * 0.5f + _winnerEntryPadding;
            var winnerEntryOffset = rightEntryX - leftmostAnchorX;

            var rightmostOutgoingX = float.MinValue;
            foreach (var position in outgoingPositions)
                rightmostOutgoingX = Mathf.Max(rightmostOutgoingX, position.x);

            var leftExitX = _itemViewport.rect.xMin - _itemSize.x * 0.5f;
            var outgoingExitDistance = rightmostOutgoingX - leftExitX + _itemSpacing;
            var brakingDistance = _spinSpeed * _settleDuration * 0.5f;
            return Mathf.Max(winnerEntryOffset, outgoingExitDistance, brakingDistance);
        }

        private void PrepareWinnerItems(float leadOffset)
        {
            for (var i = 0; i < _batch.Rewards.Count; i++)
            {
                var itemView = GetWinnerView(i);
                itemView.sprite = _batch.Rewards[i].Definition.icon;
                itemView.rectTransform.anchoredPosition =
                    _finalAnchors[i].anchoredPosition + Vector2.right * leadOffset;
                itemView.gameObject.SetActive(true);
            }
        }

        private void MoveOutgoingItems(IReadOnlyList<Vector2> startPositions, float distance)
        {
            for (var i = 0; i < _rollingItemCount; i++)
                _itemViews[i].rectTransform.anchoredPosition =
                    startPositions[i] + Vector2.left * distance;
        }

        private void PositionWinnerItems(float offset)
        {
            for (var i = 0; i < _batch.Rewards.Count; i++)
                GetWinnerView(i).rectTransform.anchoredPosition =
                    _finalAnchors[i].anchoredPosition + Vector2.right * offset;
        }

        private Image GetWinnerView(int rewardIndex)
        {
            return _itemViews[_rollingItemCount + rewardIndex];
        }

        private Sprite GetRandomRewardIcon()
        {
            var index = UnityEngine.Random.Range(0, _batch.Rewards.Count);
            return _batch.Rewards[index].Definition.icon;
        }

        private void HandleContinueClicked()
        {
            _continueButton.interactable = false;
            ContinueClicked?.Invoke();
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
