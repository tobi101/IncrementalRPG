using System.Collections;
using System.Collections.Generic;
using Core.Gameplay.Dungeon;
using Core.StateMachine.Features;
using Reflex.Attributes;
using TMPro;
using UnityEngine;
using Utils;

namespace UI
{
    public class HudView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _sessionGoldText;
        [SerializeField] private TMP_Text _killsText;
        [SerializeField] private TMP_Text _levelKillGoalText;
        [SerializeField] private TMP_Text _dungeonLevelText;
        [SerializeField] private GoldPopupView _popupPrefab;
        [SerializeField] private RectTransform _popupContainer;
        [SerializeField] private CanvasGroup _levelTransitionGroup;
        [SerializeField] private TMP_Text _levelTransitionText;
        [SerializeField] private string _levelKillGoalFormat = "{0}/{1}";
        [SerializeField] private string _levelTransitionMessage = "Новый уровень!";
        [SerializeField] private float _levelTransitionFadeDuration = 0.25f;

        private const int PoolSize = 10;
        private const float LerpSpeed = 8f;

        private GameplayFeature _gameplay;
        private readonly Queue<GoldPopupView> _popupPool = new();

        private const float BatchWindow = 0.2f;

        private double _goldDisplayed;
        private double _goldTarget;
        private float _killsDisplayed;
        private int _killsTarget;
        private int _activePopupCount;
        private int _pendingPopupGold;
        private float _batchTimer;
        private Coroutine _transitionCoroutine;

        [Inject]
        public void Construct(GameplayFeature gameplay)
        {
            _gameplay = gameplay;

            if (_popupPrefab != null)
            {
                for (var i = 0; i < PoolSize; i++)
                {
                    var popup = Instantiate(_popupPrefab, _popupContainer);
                    popup.gameObject.SetActive(false);
                    _popupPool.Enqueue(popup);
                }
            }

            _gameplay.OnSessionGoldEarned += HandleSessionGoldEarned;
            _gameplay.OnSessionKillsChanged += HandleSessionKillsChanged;
            _gameplay.OnLevelKillGoalChanged += HandleLevelKillGoalChanged;
            _gameplay.OnDungeonLevelChanged += HandleDungeonLevelChanged;
            _gameplay.OnLevelTransitionStarted += HandleLevelTransitionStarted;
        }

        private void OnEnable()
        {
            ResetPopups();
            _activePopupCount = 0;
            _pendingPopupGold = 0;
            _batchTimer = 0f;
            _goldDisplayed = 0;
            _goldTarget = 0;
            _killsDisplayed = 0;
            _killsTarget = 0;
            if (_sessionGoldText != null) _sessionGoldText.text = "0";
            if (_killsText != null) _killsText.text = "0";
            if (_levelKillGoalText != null) _levelKillGoalText.text = string.Format(_levelKillGoalFormat, 0, 0);
            if (_dungeonLevelText != null) _dungeonLevelText.text = string.Empty;
            SetLevelTransitionVisible(false);
            _transitionCoroutine = null;
        }

        private void OnDisable()
        {
            HideLevelTransition();
        }

        private void ResetPopups()
        {
            if (_popupContainer == null) return;
            _popupPool.Clear();
            foreach (Transform child in _popupContainer)
            {
                child.gameObject.SetActive(false);
                var popup = child.GetComponent<GoldPopupView>();
                if (popup != null) _popupPool.Enqueue(popup);
            }
        }

        private void Update()
        {
            if (_batchTimer > 0f)
            {
                _batchTimer -= Time.deltaTime;
                if (_batchTimer <= 0f)
                {
                    SpawnPopup(_pendingPopupGold);
                    _pendingPopupGold = 0;
                }
            }

            if (_goldDisplayed < _goldTarget)
            {
                _goldDisplayed += (_goldTarget - _goldDisplayed) * Time.deltaTime * LerpSpeed;
                if (_goldTarget - _goldDisplayed < 0.5) _goldDisplayed = _goldTarget;
                _sessionGoldText.text = new BigDouble(_goldDisplayed).ToString();
            }

            if ((int)_killsDisplayed < _killsTarget)
            {
                _killsDisplayed += (_killsTarget - _killsDisplayed) * Time.deltaTime * LerpSpeed;
                if (_killsTarget - _killsDisplayed < 0.5f) _killsDisplayed = _killsTarget;
                _killsText.text = ((int)_killsDisplayed).ToString();
            }
        }

        private void HandleSessionGoldEarned(BigDouble sessionTotal, int delta)
        {
            _goldTarget = (double)sessionTotal;
            if (_pendingPopupGold == 0) _batchTimer = BatchWindow;
            _pendingPopupGold += delta;
        }

        private void HandleSessionKillsChanged(int total)
        {
            _killsTarget = total;
        }

        private void HandleLevelKillGoalChanged(int current, int required)
        {
            if (_levelKillGoalText == null) return;
            _levelKillGoalText.text = required > 0
                ? string.Format(_levelKillGoalFormat, current, required)
                : current.ToString();
        }

        private void HandleDungeonLevelChanged(DungeonConfig dungeon, DungeonLevelConfig level, int levelIndex)
        {
            if (_dungeonLevelText == null) return;

            var dungeonName = dungeon != null ? dungeon.DisplayName : string.Empty;
            var levelName = level != null ? level.DisplayName : (levelIndex + 1).ToString();
            _dungeonLevelText.text = string.IsNullOrEmpty(dungeonName)
                ? levelName
                : $"{dungeonName} - {levelName}";
        }

        private void HandleLevelTransitionStarted(DungeonLevelConfig nextLevel, int nextLevelIndex, float duration)
        {
            if (_levelTransitionGroup == null && _levelTransitionText == null) return;

            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);

            _transitionCoroutine = StartCoroutine(PlayLevelTransition(duration));
        }

        private IEnumerator PlayLevelTransition(float duration)
        {
            if (_levelTransitionText != null)
                _levelTransitionText.text = _levelTransitionMessage;

            if (_levelTransitionGroup != null)
            {
                _levelTransitionGroup.gameObject.SetActive(true);
                yield return FadeTransitionGroup(0f, 1f, _levelTransitionFadeDuration);
            }

            var holdDuration = Mathf.Max(0f, duration - _levelTransitionFadeDuration * 2f);
            if (holdDuration > 0f)
                yield return new WaitForSeconds(holdDuration);

            if (_levelTransitionGroup != null)
                yield return FadeTransitionGroup(1f, 0f, _levelTransitionFadeDuration);

            SetLevelTransitionVisible(false);
            _transitionCoroutine = null;
        }

        private IEnumerator FadeTransitionGroup(float from, float to, float duration)
        {
            if (_levelTransitionGroup == null)
                yield break;

            if (duration <= 0f)
            {
                _levelTransitionGroup.alpha = to;
                yield break;
            }

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);
                _levelTransitionGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            _levelTransitionGroup.alpha = to;
        }

        private void HideLevelTransition()
        {
            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
                _transitionCoroutine = null;
            }

            SetLevelTransitionVisible(false);
        }

        private void SetLevelTransitionVisible(bool visible)
        {
            if (_levelTransitionGroup == null) return;

            _levelTransitionGroup.alpha = visible ? 1f : 0f;
            _levelTransitionGroup.gameObject.SetActive(visible);
        }

        private void SpawnPopup(int amount)
        {
            if (_popupPrefab == null) return;

            var popup = _popupPool.Count > 0
                ? _popupPool.Dequeue()
                : Instantiate(_popupPrefab, _popupContainer);

            var startY = _activePopupCount * 50f;
            _activePopupCount++;
            popup.Show(amount, startY, () =>
            {
                _activePopupCount = Mathf.Max(0, _activePopupCount - 1);
                _popupPool.Enqueue(popup);
            });
        }

        private void OnDestroy()
        {
            if (_gameplay == null) return;
            _gameplay.OnSessionGoldEarned -= HandleSessionGoldEarned;
            _gameplay.OnSessionKillsChanged -= HandleSessionKillsChanged;
            _gameplay.OnLevelKillGoalChanged -= HandleLevelKillGoalChanged;
            _gameplay.OnDungeonLevelChanged -= HandleDungeonLevelChanged;
            _gameplay.OnLevelTransitionStarted -= HandleLevelTransitionStarted;
        }

    }
}
