using Core.StateMachine;
using Core.StateMachine.Features;
using Core.StateMachine.States;
using Core.Gameplay.Dungeon;
using IncrementalRPG.Scripts.AudioManager;
using Reflex.Attributes;
using UnityEngine;

namespace UI
{
    public class HubView : MonoBehaviour
    {
        [SerializeField] private HubFeatureButtonView _dungeonButton;
        [SerializeField] private HubFeatureButtonView _skillTreeButton;
        [SerializeField] private HubFeatureButtonView _barracksButton;
        [SerializeField] private HubFeatureButtonView _mineButton;
        [SerializeField] private HubFeatureButtonView _craftButton;
        [SerializeField] private DungeonMenuView _dungeonMenuView;
        [SerializeField] private MapMenuFadeTransition _mapMenuFadeTransition;

        [Header("Open Sounds")]
        [SerializeField] private AudioClip _mapOpenSound;
        [SerializeField] private AudioClip _skillTreeOpenSound;
        [SerializeField] private AudioClip _barracksOpenSound;
        [SerializeField] private AudioClip _mineOpenSound;
        [SerializeField] private AudioClip _craftOpenSound;

        [Inject] private GameStateMachine _stateMachine;
        [Inject] private DungeonList _dungeonList;
        [Inject] private DungeonSelectionService _dungeonSelection;
        [Inject] private GameplayFeature _gameplay;
        [Inject] private AudioManager _audioManager;

        private bool _isStartingDungeon;

        private void OnEnable()
        {
            _isStartingDungeon = false;
        }

        private void Start()
        {
            UIButtonAudio.InstallInChildren(this);
            SetHubFeatureClickSoundsEnabled(false);

            if (_dungeonButton != null && _dungeonButton.Button != null)
                _dungeonButton.Button.onClick.AddListener(OpenDungeon);

            if (_skillTreeButton != null && _skillTreeButton.Button != null)
                _skillTreeButton.Button.onClick.AddListener(OpenSkillTree);

            if (_barracksButton != null && _barracksButton.Button != null)
                _barracksButton.Button.onClick.AddListener(OpenBarracks);

            if (_mineButton != null && _mineButton.Button != null)
                _mineButton.Button.onClick.AddListener(OpenMine);

            if (_craftButton != null && _craftButton.Button != null)
                _craftButton.Button.onClick.AddListener(OpenCraft);
        }

        private void OnDestroy()
        {
            if (_dungeonButton != null && _dungeonButton.Button != null)
                _dungeonButton.Button.onClick.RemoveListener(OpenDungeon);

            if (_skillTreeButton != null && _skillTreeButton.Button != null)
                _skillTreeButton.Button.onClick.RemoveListener(OpenSkillTree);

            if (_barracksButton != null && _barracksButton.Button != null)
                _barracksButton.Button.onClick.RemoveListener(OpenBarracks);

            if (_mineButton != null && _mineButton.Button != null)
                _mineButton.Button.onClick.RemoveListener(OpenMine);

            if (_craftButton != null && _craftButton.Button != null)
                _craftButton.Button.onClick.RemoveListener(OpenCraft);
        }

        private void OpenDungeon()
        {
            if (_dungeonMenuView != null)
            {
                if (!_dungeonMenuView.gameObject.activeSelf)
                    PlayOpenSound(_mapOpenSound);

                _dungeonMenuView.Show(_dungeonList, _dungeonSelection, StartDungeon);
                return;
            }

            StartDungeon(_dungeonList.GetFirstPlayable());
        }

        private void StartDungeon(DungeonConfig dungeon)
        {
            if (_isStartingDungeon)
                return;

            if (dungeon == null || !dungeon.HasPlayableLevels)
            {
                Debug.LogError("[HubView] Cannot start gameplay because selected dungeon is not playable.");
                return;
            }

            _isStartingDungeon = true;

            if (_mapMenuFadeTransition != null)
            {
                _audioManager ??= AudioManager.Resolve();
                _audioManager?.PlayFightStartFade();
                _mapMenuFadeTransition.Play(() =>
                {
                    _audioManager?.PlayFightStartBurn();
                    EnterDungeon(dungeon);
                }, HandleDungeonTransitionFinished);
                return;
            }

            EnterDungeon(dungeon);
            HandleDungeonTransitionFinished();
        }

        private void EnterDungeon(DungeonConfig dungeon)
        {
            if (_dungeonMenuView != null)
                _dungeonMenuView.Hide();

            _dungeonSelection.Select(dungeon);
            _stateMachine.Enter<GameplayState>();
        }

        private void HandleDungeonTransitionFinished()
        {
            _audioManager?.PlayMusic(MusicTrack.Gameplay);
            _gameplay.StartSession();
            _audioManager?.PlayLavaLoop();
            _isStartingDungeon = false;
        }

        private void OpenSkillTree()
        {
            PlayOpenSound(_skillTreeOpenSound);
            _stateMachine.Enter<SkillTreeMenuState>();
        }

        private void OpenBarracks()
        {
            PlayOpenSound(_barracksOpenSound);
            _stateMachine.Enter<BarracksState>();
        }

        private void OpenMine()
        {
            PlayOpenSound(_mineOpenSound);
            _stateMachine.Enter<MineState>();
        }

        private void OpenCraft()
        {
            PlayOpenSound(_craftOpenSound);
            _stateMachine.Enter<CraftState>();
        }

        private void PlayOpenSound(AudioClip clip)
        {
            if (_audioManager == null)
                _audioManager = AudioManager.Resolve();

            _audioManager?.PlaySfx(clip);
        }

        private void SetHubFeatureClickSoundsEnabled(bool enabled)
        {
            SetHubFeatureClickSoundEnabled(_dungeonButton, enabled);
            SetHubFeatureClickSoundEnabled(_skillTreeButton, enabled);
            SetHubFeatureClickSoundEnabled(_barracksButton, enabled);
            SetHubFeatureClickSoundEnabled(_mineButton, enabled);
            SetHubFeatureClickSoundEnabled(_craftButton, enabled);
        }

        private static void SetHubFeatureClickSoundEnabled(HubFeatureButtonView buttonView, bool enabled)
        {
            if (buttonView != null && buttonView.Button != null)
                UIButtonAudio.SetClickSoundEnabled(buttonView.Button, enabled);
        }
    }
}
