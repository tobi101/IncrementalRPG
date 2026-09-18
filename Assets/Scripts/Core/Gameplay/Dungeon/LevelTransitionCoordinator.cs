using Core.Items;
using Core.StateMachine.Features;
using IncrementalRPG.Scripts.Reflex;
using Reflex.Attributes;
using UI;
using System.Linq;

namespace Core.Gameplay.Dungeon
{
    public sealed class LevelTransitionCoordinator : IAwakeable
    {
        private const int RewardCount = 1;

        [Inject] private GameplayFeature _gameplay;
        [Inject] private DungeonSelectionService _dungeonSelection;
        [Inject] private IPlayerInventoryGateway _inventory;
        [Inject] private HudView _hud;

        private LevelTransitionCurtainView _curtain;
        private LootboxView _lootbox;
        private float _openDuration;
        private bool _transitionInProgress;
        private bool _awaitingRewardChoice;
        private bool _spinCompleted;
        private LootReward _reward;

        public void OnAwake()
        {
            _curtain = _hud.LevelTransitionCurtain;
            _lootbox = _hud.Lootbox;

            _gameplay.OnLevelTransitionStarted += HandleTransitionStarted;
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
            if (_transitionInProgress)
                return;

            _transitionInProgress = true;
            _awaitingRewardChoice = false;
            _spinCompleted = false;
            _openDuration = openDuration;

            var rolledItems = _gameplay.CurrentLevel.lootPool.Roll(RewardCount);
            var batch = _inventory.Grant(rolledItems);
            _reward = batch.Rewards[0];

            _dungeonSelection.MarkLevelReached(_gameplay.CurrentDungeon, nextLevelIndex);
            _hud.PrepareLevelTransitionMessage();
            _lootbox.Prepare(_reward, _gameplay.CurrentLevel.lootPool.entries
                .Where(entry => entry != null && entry.IsValid && entry.item.icon != null)
                .Select(entry => entry.item.icon).Distinct().ToArray());
            _lootbox.SetPaused(_gameplay.IsPaused);
            _curtain.SetPaused(_gameplay.IsPaused);
            _curtain.Prepare(_gameplay.CurrentDungeon.LevelCount, nextLevelIndex - 1);
            _curtain.PlayClose(closeDuration, HandleCurtainsClosed);
        }

        private void HandleCurtainsClosed()
        {
            if (!_gameplay.ApplyPendingLevelBehindCurtain())
                return;

            _curtain.PlayReveal();
        }

        private void HandleLampAnimationCompleted()
        {
            if (_transitionInProgress)
                _lootbox.PlayOpen();
        }

        private void HandleSpinCompleted()
        {
            if (!_transitionInProgress || _spinCompleted)
                return;

            _spinCompleted = true;
            _awaitingRewardChoice = true;
            var canUse = _inventory.CanUseReward(_reward);
            _lootbox.ShowResult(canUse, _inventory.GetRewardUseUnavailableKey(_reward));
            _curtain.SetInteractionEnabled(true);
        }

        private void HandleContinueClicked()
        {
            if (!_transitionInProgress || !_awaitingRewardChoice || _gameplay.IsPaused)
                return;

            _awaitingRewardChoice = false;
            _lootbox.HideResult();
            _curtain.SetInteractionEnabled(false);
            _curtain.PlayOpen(_openDuration, HandleCurtainsOpened);
        }

        private void HandleUseNowClicked()
        {
            if (!_transitionInProgress || !_awaitingRewardChoice || _gameplay.IsPaused)
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
            _transitionInProgress = false;
            _awaitingRewardChoice = false;
            _gameplay.FinishPendingLevelTransition();
        }

        private void HandlePauseChanged(bool isPaused)
        {
            _curtain.SetPaused(isPaused);
            _lootbox.SetPaused(isPaused);
        }

        private void HandleGameplayDisabled()
        {
            _transitionInProgress = false;
            _awaitingRewardChoice = false;
            _spinCompleted = false;
            _openDuration = 0f;
            _curtain.HideImmediately();
            _lootbox.ResetView();
        }
    }
}
