using System.Collections;
using System.Collections.Generic;
using Core.Gameplay.Dungeon;
using Core.StateMachine.Features;
using Model;
using Reflex.Attributes;
using TMPro;
using UI.Localization;
using UnityEngine;
using UnityEngine.Localization;
using Utils;

namespace UI
{
    public class HudView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _sessionGoldText;
        [SerializeField] private TMP_Text _shardText;
        [SerializeField] private GameObject _shardCounterRoot;
        [SerializeField] private TMP_Text _killsText;
        [SerializeField] private TMP_Text _experienceText;
        [SerializeField] private TMP_Text _dungeonLevelText;
        [SerializeField] private GoldPopupView _popupPrefab;
        [SerializeField] private RectTransform _popupContainer;
        [SerializeField] private LevelTransitionCurtainView _levelTransitionCurtain;
        [SerializeField] private CanvasGroup _levelTransitionGroup;
        [SerializeField] private TMP_Text _levelTransitionText;
        [SerializeField] private LocalizedString _dungeonLevelFormat = new();
        [SerializeField] private LocalizedString _levelTransitionMessage = new();
        [SerializeField] private float _levelTransitionFadeDuration = 0.25f;

        private const int PoolSize = 10;
        private const float LerpSpeed = 8f;

        private GameplayFeature _gameplay;
        private Player _player;
        private Core.TestSkillTree.SkillTreeService _skillTree;
        private readonly Queue<GoldPopupView> _popupPool = new();

        private const float BatchWindow = 0.2f;

        private float _killsDisplayed;
        private int _killsTarget;
        private BigDouble _experienceDisplayed;
        private BigDouble _experienceTarget;
        private BigDouble _experienceGoal;
        private int _activePopupCount;
        private BigDouble _pendingPopupGold;
        private float _batchTimer;
        private Coroutine _transitionCoroutine;
        private LocalizedStringBinding _dungeonLevelBinding;
        private LocalizedStringBinding _levelTransitionBinding;
        private LocalizedString _boundDungeonName;
        private LocalizedString _boundLevelName;
        private LocalizedString.ChangeHandler _dungeonNameChanged;
        private LocalizedString.ChangeHandler _levelNameChanged;
        private string _currentDungeonName = string.Empty;
        private string _currentLevelName = string.Empty;

        private void Awake()
        {
            _dungeonLevelBinding = new LocalizedStringBinding(_dungeonLevelText);
            _levelTransitionBinding = new LocalizedStringBinding(_levelTransitionText);
            _dungeonNameChanged = HandleDungeonNameChanged;
            _levelNameChanged = HandleLevelNameChanged;
        }

        [Inject]
        public void Construct(GameplayFeature gameplay, Player player,
            Core.TestSkillTree.SkillTreeService skillTree)
        {
            _gameplay = gameplay;
            _player = player;
            _skillTree = skillTree;

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
            _gameplay.OnLevelExperienceChanged += HandleLevelExperienceChanged;
            _gameplay.OnDungeonLevelChanged += HandleDungeonLevelChanged;
            _gameplay.OnLevelTransitionStarted += HandleLevelTransitionStarted;
            _player.OnShardsChanged += RefreshShards;
            _skillTree.OnUpgraded += RefreshShardFeatureVisibility;
            RefreshShardFeatureVisibility();
            RefreshShards();
            HandleLevelExperienceChanged(_gameplay.LevelExperience, _gameplay.CurrentLevelExperienceGoal);
        }

        private void OnEnable()
        {
            ResetPopups();
            _activePopupCount = 0;
            _pendingPopupGold = BigDouble.Zero;
            _batchTimer = 0f;
            _killsDisplayed = 0;
            _killsTarget = 0;
            _experienceDisplayed = BigDouble.Zero;
            _experienceTarget = BigDouble.Zero;
            _experienceGoal = BigDouble.Zero;
            if (_sessionGoldText != null) _sessionGoldText.text = "0";
            RefreshShards();
            if (_killsText != null) _killsText.text = "0";
            if (_gameplay != null)
                HandleLevelExperienceChanged(_gameplay.LevelExperience, _gameplay.CurrentLevelExperienceGoal);
            else
                RefreshExperienceText();
            ClearDungeonLevelNameBindings();
            _dungeonLevelBinding.Clear();
            _levelTransitionBinding.Clear();
            _levelTransitionCurtain?.HideImmediately();
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
            var isGameplayPaused = IsGameplayPaused();
            _levelTransitionCurtain?.SetPaused(isGameplayPaused);

            if (isGameplayPaused)
                return;

            if (_batchTimer > 0f)
            {
                _batchTimer -= Time.deltaTime;
                if (_batchTimer <= 0f)
                {
                    SpawnPopup(_pendingPopupGold);
                    _pendingPopupGold = BigDouble.Zero;
                }
            }

            if ((int)_killsDisplayed < _killsTarget)
            {
                _killsDisplayed += (_killsTarget - _killsDisplayed) * Time.deltaTime * LerpSpeed;
                if (_killsTarget - _killsDisplayed < 0.5f) _killsDisplayed = _killsTarget;
                if (_killsText != null)
                    _killsText.text = ((int)_killsDisplayed).ToString();
            }

            if (_experienceDisplayed < _experienceTarget)
            {
                var interpolation = Mathf.Clamp01(Time.deltaTime * LerpSpeed);
                _experienceDisplayed += (_experienceTarget - _experienceDisplayed) * interpolation;

                if (_experienceTarget - _experienceDisplayed < BigDouble.One)
                    _experienceDisplayed = _experienceTarget;

                RefreshExperienceText();
            }
        }

        private void HandleSessionGoldEarned(BigDouble sessionTotal, BigDouble delta)
        {
            if (_sessionGoldText != null)
                _sessionGoldText.text = BigDoubleFormatter.FormatFloor(sessionTotal);

            if (delta <= 0)
            {
                _pendingPopupGold = BigDouble.Zero;
                _batchTimer = 0f;
                return;
            }

            if (_pendingPopupGold == BigDouble.Zero) _batchTimer = BatchWindow;
            _pendingPopupGold += delta;
        }

        private void RefreshShards()
        {
            if (_shardText == null || _player == null)
                return;

            _shardText.text = BigDoubleFormatter.FormatFloor(_player.ShardTotal);
        }

        private void RefreshShardFeatureVisibility()
        {
            if (_shardCounterRoot != null && _skillTree != null)
                _shardCounterRoot.SetActive(_skillTree.IsUnlocked(Core.TestSkillTree.GameFeature.Shards));
        }

        private void HandleSessionKillsChanged(int total)
        {
            _killsTarget = total;

            if (total <= 0)
            {
                _killsDisplayed = 0f;

                if (_killsText != null)
                    _killsText.text = "0";
            }
        }

        private void HandleLevelExperienceChanged(BigDouble current, BigDouble goal)
        {
            current = current.NormalizedOr(BigDouble.Zero);
            _experienceGoal = BigDouble.Max(BigDouble.Zero, goal.NormalizedOr(BigDouble.Zero));
            _experienceTarget = BigDouble.Max(BigDouble.Zero, current);

            if (_experienceDisplayed > _experienceTarget)
                _experienceDisplayed = _experienceTarget;

            RefreshExperienceText();
        }

        private void RefreshExperienceText()
        {
            if (_experienceText == null)
                return;

            _experienceText.text = BigDoubleFormatter.FormatFloor(_experienceDisplayed)
                                   + " / "
                                   + BigDoubleFormatter.FormatFloor(_experienceGoal);
        }

        private void HandleDungeonLevelChanged(DungeonConfig dungeon, DungeonLevelConfig level, int levelIndex)
        {
            if (_dungeonLevelText == null) return;
            BindDungeonName(dungeon != null ? dungeon.displayName : null);
            BindLevelName(level != null ? level.displayName : null);
        }

        private void HandleLevelTransitionStarted(DungeonLevelConfig nextLevel, int nextLevelIndex,
            float closeDuration, float holdDuration, float openDuration)
        {
            var duration = closeDuration + holdDuration + openDuration;

            if (_levelTransitionCurtain != null)
            {
                _levelTransitionCurtain.SetPaused(IsGameplayPaused());
                _levelTransitionCurtain.Play(closeDuration, holdDuration, openDuration);
                return;
            }

            if (_levelTransitionGroup == null && _levelTransitionText == null) return;

            if (_transitionCoroutine != null)
                StopCoroutine(_transitionCoroutine);

            _transitionCoroutine = StartCoroutine(PlayLevelTransition(duration));
        }

        private IEnumerator PlayLevelTransition(float duration)
        {
            if (_levelTransitionText != null)
                _levelTransitionBinding.Bind(_levelTransitionMessage);

            if (_levelTransitionGroup != null)
            {
                _levelTransitionGroup.gameObject.SetActive(true);
                yield return FadeTransitionGroup(0f, 1f, _levelTransitionFadeDuration);
            }

            var holdDuration = Mathf.Max(0f, duration - _levelTransitionFadeDuration * 2f);
            if (holdDuration > 0f)
                yield return WaitForGameplaySeconds(holdDuration);

            if (_levelTransitionGroup != null)
                yield return FadeTransitionGroup(1f, 0f, _levelTransitionFadeDuration);

            SetLevelTransitionVisible(false);
            _transitionCoroutine = null;
        }

        private void BindDungeonName(LocalizedString localizedName)
        {
            if (ReferenceEquals(_boundDungeonName, localizedName))
            {
                localizedName?.RefreshString();
                return;
            }

            if (_boundDungeonName != null)
                _boundDungeonName.StringChanged -= _dungeonNameChanged;

            _boundDungeonName = localizedName;
            _currentDungeonName = string.Empty;

            if (_boundDungeonName == null || _boundDungeonName.IsEmpty)
            {
                RefreshDungeonLevelText();
                return;
            }

            _boundDungeonName.StringChanged += _dungeonNameChanged;
        }

        private void BindLevelName(LocalizedString localizedName)
        {
            if (ReferenceEquals(_boundLevelName, localizedName))
            {
                localizedName?.RefreshString();
                return;
            }

            if (_boundLevelName != null)
                _boundLevelName.StringChanged -= _levelNameChanged;

            _boundLevelName = localizedName;
            _currentLevelName = string.Empty;

            if (_boundLevelName == null || _boundLevelName.IsEmpty)
            {
                RefreshDungeonLevelText();
                return;
            }

            _boundLevelName.StringChanged += _levelNameChanged;
        }

        private void HandleDungeonNameChanged(string value)
        {
            _currentDungeonName = value;
            RefreshDungeonLevelText();
        }

        private void HandleLevelNameChanged(string value)
        {
            _currentLevelName = value;
            RefreshDungeonLevelText();
        }

        private void RefreshDungeonLevelText()
        {
            if (_dungeonLevelText == null)
                return;

            if (_dungeonLevelFormat == null || _dungeonLevelFormat.IsEmpty
                                           || string.IsNullOrEmpty(_currentDungeonName)
                                           && string.IsNullOrEmpty(_currentLevelName))
            {
                _dungeonLevelBinding.Clear();
                return;
            }

            _dungeonLevelFormat.Arguments = new object[] { _currentDungeonName, _currentLevelName };
            _dungeonLevelBinding.Bind(_dungeonLevelFormat);
        }

        private void ClearDungeonLevelNameBindings()
        {
            if (_boundDungeonName != null)
                _boundDungeonName.StringChanged -= _dungeonNameChanged;

            if (_boundLevelName != null)
                _boundLevelName.StringChanged -= _levelNameChanged;

            _boundDungeonName = null;
            _boundLevelName = null;
            _currentDungeonName = string.Empty;
            _currentLevelName = string.Empty;
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
                elapsed += GetGameplayDeltaTime();
                var t = Mathf.Clamp01(elapsed / duration);
                _levelTransitionGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            _levelTransitionGroup.alpha = to;
        }

        private IEnumerator WaitForGameplaySeconds(float duration)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += GetGameplayDeltaTime();
                yield return null;
            }
        }

        private float GetGameplayDeltaTime()
        {
            return IsGameplayPaused() ? 0f : Time.deltaTime;
        }

        private bool IsGameplayPaused()
        {
            return _gameplay != null && _gameplay.IsPaused;
        }

        private void HideLevelTransition()
        {
            if (_transitionCoroutine != null)
            {
                StopCoroutine(_transitionCoroutine);
                _transitionCoroutine = null;
            }

            SetLevelTransitionVisible(false);
            _levelTransitionCurtain?.HideImmediately();
        }

        private void SetLevelTransitionVisible(bool visible)
        {
            if (_levelTransitionGroup == null) return;

            _levelTransitionGroup.alpha = visible ? 1f : 0f;
            _levelTransitionGroup.gameObject.SetActive(visible);
        }

        private void SpawnPopup(BigDouble amount)
        {
            if (_popupPrefab == null) return;

            var popup = _popupPool.Count > 0
                ? _popupPool.Dequeue()
                : Instantiate(_popupPrefab, _popupContainer);

            var startY = _popupContainer.rect.height * 0.5f - _activePopupCount * 50f;
            _activePopupCount++;
            popup.Show(amount, startY, () =>
            {
                _activePopupCount = Mathf.Max(0, _activePopupCount - 1);
                _popupPool.Enqueue(popup);
            });
        }

        private void OnDestroy()
        {
            if (_gameplay != null)
            {
                _gameplay.OnSessionGoldEarned -= HandleSessionGoldEarned;
                _gameplay.OnSessionKillsChanged -= HandleSessionKillsChanged;
                _gameplay.OnLevelExperienceChanged -= HandleLevelExperienceChanged;
                _gameplay.OnDungeonLevelChanged -= HandleDungeonLevelChanged;
                _gameplay.OnLevelTransitionStarted -= HandleLevelTransitionStarted;
            }

            if (_player != null)
                _player.OnShardsChanged -= RefreshShards;

            if (_skillTree != null)
                _skillTree.OnUpgraded -= RefreshShardFeatureVisibility;

            ClearDungeonLevelNameBindings();
            _dungeonLevelBinding?.Dispose();
            _levelTransitionBinding?.Dispose();
        }

    }
}
