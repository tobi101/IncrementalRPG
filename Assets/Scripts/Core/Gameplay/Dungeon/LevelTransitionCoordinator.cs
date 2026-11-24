using Core.Items;
using Core.StateMachine.Features;
using IncrementalRPG.Scripts.Reflex;
using Reflex.Attributes;
using UI;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Gameplay.Dungeon
{
    public sealed class LevelTransitionCoordinator : IAwakeable
    {
        private const int RewardCount = 1;

        private enum Presentation { None, LevelTransition, BossReward }

        [Inject] private GameplayFeature _gameplay;
        [Inject] private DungeonLevelTransitionConfig _config;
        [Inject] private DungeonSelectionService _dungeonSelection;
        [Inject] private IPlayerInventoryGateway _inventory;
        [Inject] private HudView _hud;

        private LevelTransitionCurtainView _curtain;
        private LootboxView _lootbox;
        private Button _bossChest;
        private float _openDuration;
        private Presentation _presentation;
        private bool _awaitingRewardChoice;
        private bool _spinCompleted;
        private LootReward _reward;

        public void OnAwake()
        {
            _curtain = _hud.LevelTransitionCurtain;
            _lootbox = _hud.Lootbox;

            _gameplay.OnLevelTransitionStarted += HandleTransitionStarted;
            _gameplay.OnBossChestReady += HandleBossChestReady;
            _gameplay.OnPauseChanged += HandlePauseChanged;
            _gameplay.OnDisabled += HandleGameplayDisabled;
            _curtain.LampAnimationCompleted += HandleLampAnimationCompleted;
            _lootbox.SpinCompleted += HandleSpinCompleted;
            _lootbox.ContinueClicked += HandleContinueClicked;
            _lootbox.UseNowClicked += HandleUseNowClicked;
        }

        private void HandleTransitionStarted(DungeonLevelConfig nextLevel, int nextLevelIndex,
            float closeDuration, float holdDuration, float openDuration)
        {
            if (_presentation != Presentation.None)
                return;

            PrepareReward(Presentation.LevelTransition, openDuration);
            _dungeonSelection.MarkLevelReached(_gameplay.CurrentDungeon, nextLevelIndex);
            _hud.PrepareLevelTransitionMessage();
            var progress = _gameplay.CurrentDungeon.GetRegularLevelProgress(_gameplay.CurrentLevelIndex);
            _curtain.Prepare(progress.total, progress.completed - 1);
            _curtain.PlayClose(closeDuration, HandleCurtainsClosed);
        }

        private void HandleBossChestReady(Vector3 chestPosition)
        {
            var viewportPosition = Camera.main.WorldToViewportPoint(chestPosition);
            var totem = _gameplay.CurrentLevel.bossEncounter.totem.viewPrefab;
            var reveal = totem.GetComponentInChildren<Entity.SpawnBurnReveal>(true);
            _bossChest = _lootbox.CreateArenaChest((RectTransform)_hud.transform, viewportPosition, reveal);
            _bossChest.onClick.AddListener(HandleBossChestClicked);
        }

        private void HandleBossChestClicked()
        {
            if (_presentation != Presentation.None || !_gameplay.TryOpenBossReward())
                return;

            HideBossChest();
            PrepareReward(Presentation.BossReward, _config.OpenDuration);
            _curtain.PrepareLootPresentation();
            _curtain.PlayClose(_config.CloseDuration, () => _curtain.PlayReveal(_lootbox.PlayOpen));
        }

        private void PrepareReward(Presentation presentation, float openDuration)
        {
            _presentation = presentation;
            _awaitingRewardChoice = false;
            _spinCompleted = false;
            _openDuration = openDuration;

            var pool = _gameplay.CurrentLevel.lootPool;
            _reward = _inventory.Grant(pool.Roll(RewardCount)).Rewards[0];
            _lootbox.Prepare(_reward, pool.entries
                .Where(entry => entry != null && entry.IsValid && entry.item.icon != null)
                .Select(entry => entry.item.icon).Distinct().ToArray(),
                showTransitionLabels: presentation == Presentation.LevelTransition);
            _lootbox.SetPaused(_gameplay.IsPaused);
            _curtain.SetPaused(_gameplay.IsPaused);
        }

        private void HandleCurtainsClosed()
        {
            if (!_gameplay.ApplyPendingLevelBehindCurtain())
                return;

            _curtain.PlayReveal();
        }

        private void HandleLampAnimationCompleted()
        {
            if (_presentation == Presentation.LevelTransition)
                _lootbox.PlayOpen();
        }

        private void HandleSpinCompleted()
        {
            if (_presentation == Presentation.None || _spinCompleted)
                return;

            _spinCompleted = true;
            _awaitingRewardChoice = true;
            var showUseNow = _presentation == Presentation.LevelTransition;
            var canUse = showUseNow && _inventory.CanUseReward(_reward);
            _lootbox.ShowResult(canUse, showUseNow ? _inventory.GetRewardUseUnavailableKey(_reward) : null, showUseNow);
            _curtain.SetInteractionEnabled(true);
        }

        private void HandleContinueClicked()
        {
            if (_presentation == Presentation.None || !_awaitingRewardChoice || _gameplay.IsPaused)
                return;

            _awaitingRewardChoice = false;
            _lootbox.HideResult();
            _curtain.SetInteractionEnabled(false);
            _curtain.PlayOpen(_openDuration, HandleCurtainsOpened);
        }

        private void HandleUseNowClicked()
        {
            if (_presentation != Presentation.LevelTransition || !_awaitingRewardChoice || _gameplay.IsPaused)
                return;

            _awaitingRewardChoice = false;
            if (!_inventory.TryUseReward(_reward))
            {
                _awaitingRewardChoice = true;
                _lootbox.ShowUseUnavailable(_inventory.GetRewardUseUnavailableKey(_reward));
                return;
            }

            _lootbox.HideResult();
            _curtain.SetInteractionEnabled(false);
            _curtain.PlayOpen(_openDuration, HandleCurtainsOpened);
        }

        private void HandleCurtainsOpened()
        {
            var presentation = _presentation;
            _presentation = Presentation.None;
            _awaitingRewardChoice = false;
            if (presentation == Presentation.BossReward)
                _gameplay.FinishBossReward();
            else if (presentation == Presentation.LevelTransition)
                _gameplay.FinishPendingLevelTransition();
        }

        private void HandlePauseChanged(bool isPaused)
        {
            _curtain.SetPaused(isPaused);
            _lootbox.SetPaused(isPaused);
            if (_bossChest != null)
                _bossChest.interactable = !isPaused;
        }

        private void HandleGameplayDisabled()
        {
            _presentation = Presentation.None;
            _awaitingRewardChoice = false;
            _spinCompleted = false;
            _openDuration = 0f;
            HideBossChest();
            _curtain.HideImmediately();
            _lootbox.ResetView();
        }

        private void HideBossChest()
        {
            if (_bossChest == null)
                return;

            _bossChest.gameObject.SetActive(false);
            Object.Destroy(_bossChest.gameObject);
            _bossChest = null;
        }
    }
}
