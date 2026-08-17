using Core.Items;
using Core.StateMachine.Features;
using IncrementalRPG.Scripts.Reflex;
using Reflex.Attributes;
using UI;

namespace Core.Gameplay.Dungeon
{
    public sealed class LevelTransitionCoordinator : IAwakeable
    {
        private const int RewardCount = 6;

        [Inject] private GameplayFeature _gameplay;
        [Inject] private DungeonSelectionService _dungeonSelection;
        [Inject] private PlayerItemStorage _itemStorage;
        [Inject] private HudView _hud;

        private LevelTransitionCurtainView _curtain;
        private LootboxView _lootbox;
        private float _openDuration;
        private bool _transitionInProgress;

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
        }

        private void HandleTransitionStarted(DungeonLevelConfig nextLevel, int nextLevelIndex,
            float closeDuration, float holdDuration, float openDuration)
        {
            if (_transitionInProgress)
                return;

            _transitionInProgress = true;
            _openDuration = openDuration;

            var rolledItems = _gameplay.CurrentLevel.lootPool.Roll(RewardCount);
            var batch = _itemStorage.Grant(rolledItems);

            _dungeonSelection.MarkLevelReached(_gameplay.CurrentDungeon, nextLevelIndex);
            _hud.PrepareLevelTransitionMessage();
            _lootbox.Prepare(batch);
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
            if (!_transitionInProgress)
                return;

            _lootbox.ShowContinueButton();
            _curtain.SetInteractionEnabled(true);
        }

        private void HandleContinueClicked()
        {
            if (!_transitionInProgress)
                return;

            _curtain.SetInteractionEnabled(false);
            _curtain.PlayOpen(_openDuration, HandleCurtainsOpened);
        }

        private void HandleCurtainsOpened()
        {
            _transitionInProgress = false;
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
            _openDuration = 0f;
            _curtain.HideImmediately();
            _lootbox.ResetView();
        }
    }
}
