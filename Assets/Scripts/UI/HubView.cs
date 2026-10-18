using System.Collections;
using Core;
using Core.Forge;
using Core.Items;
using Core.StateMachine;
using Core.StateMachine.Features;
using Core.StateMachine.States;
using Core.Gameplay.Dungeon;
using IncrementalRPG.Scripts.AudioManager;
using Reflex.Attributes;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Serialization;

namespace UI
{
    public class HubView : MonoBehaviour
    {
        [SerializeField] private HubFeatureButtonView _dungeonButton;
        [SerializeField] private HubFeatureButtonView _skillTreeButton;
        [SerializeField] private HubFeatureButtonView _inventoryButton;
        [SerializeField] private HubFeatureButtonView _barracksButton;
        [FormerlySerializedAs("_mineButton")]
        [SerializeField] private HubFeatureButtonView _shrineButton;
        [SerializeField] private HubFeatureButtonView _craftButton;
        [SerializeField] private DungeonMenuView _dungeonMenuView;
        [SerializeField] private MapMenuFadeTransition _mapMenuFadeTransition;

        [Header("Feature locked notice")]
        [FormerlySerializedAs("_classesLockedNotice")]
        [SerializeField] private CanvasGroup _featureLockedNotice;
        [SerializeField] private LocalizeStringEvent _featureLockedNoticeText;
        [FormerlySerializedAs("_classesLockedNoticeDuration")]
        [SerializeField, Min(0.1f)] private float _featureLockedNoticeDuration = 4f;

        [Header("Open Sounds")]
        [SerializeField] private AudioClip _mapOpenSound;
        [SerializeField] private AudioClip _skillTreeOpenSound;
        [SerializeField] private AudioClip _barracksOpenSound;
        [FormerlySerializedAs("_mineOpenSound")]
        [SerializeField] private AudioClip _shrineOpenSound;
        [SerializeField] private AudioClip _craftOpenSound;

        [Inject] private GameStateMachine _stateMachine;
        [Inject] private DungeonList _dungeonList;
        [Inject] private DungeonSelectionService _dungeonSelection;
        [Inject] private GameplayFeature _gameplay;
        [Inject] private AudioManager _audioManager;
        [Inject] private ForgeService _forge;
        [Inject] private HubVisitProgress _hubVisits;
        [Inject] private PlayerItemStorage _itemStorage;

        private bool _isStartingDungeon;
        private Coroutine _featureLockedNoticeRoutine;

        private void OnEnable()
        {
            _isStartingDungeon = false;
            HideFeatureLockedNotice();
            RefreshNotifications();
        }

        private void OnDisable()
        {
            HideFeatureLockedNotice();
        }

        private void Start()
        {
            UIButtonAudio.InstallInChildren(this);
            SetHubFeatureClickSoundsEnabled(false);
            _hubVisits.OnChanged += RefreshNotifications;
            _itemStorage.OnChanged += RefreshNotifications;
            _dungeonSelection.OnProgressChanged += RefreshNotifications;
            RefreshNotifications();

            if (_dungeonButton != null && _dungeonButton.Button != null)
                _dungeonButton.Button.onClick.AddListener(OpenDungeon);

            if (_skillTreeButton != null && _skillTreeButton.Button != null)
                _skillTreeButton.Button.onClick.AddListener(OpenSkillTree);

            if (_inventoryButton != null && _inventoryButton.Button != null)
                _inventoryButton.Button.onClick.AddListener(OpenInventory);

            if (_barracksButton != null && _barracksButton.Button != null)
                _barracksButton.Button.onClick.AddListener(OpenBarracks);

            if (_shrineButton != null && _shrineButton.Button != null)
                _shrineButton.Button.onClick.AddListener(OpenClasses);

            if (_craftButton != null && _craftButton.Button != null)
                _craftButton.Button.onClick.AddListener(OpenCraft);
        }

        private void OnDestroy()
        {
            if (_hubVisits != null)
                _hubVisits.OnChanged -= RefreshNotifications;
            if (_itemStorage != null)
                _itemStorage.OnChanged -= RefreshNotifications;
            if (_dungeonSelection != null)
                _dungeonSelection.OnProgressChanged -= RefreshNotifications;

            if (_dungeonButton != null && _dungeonButton.Button != null)
                _dungeonButton.Button.onClick.RemoveListener(OpenDungeon);

            if (_skillTreeButton != null && _skillTreeButton.Button != null)
                _skillTreeButton.Button.onClick.RemoveListener(OpenSkillTree);

            if (_inventoryButton != null && _inventoryButton.Button != null)
                _inventoryButton.Button.onClick.RemoveListener(OpenInventory);

            if (_barracksButton != null && _barracksButton.Button != null)
                _barracksButton.Button.onClick.RemoveListener(OpenBarracks);

            if (_shrineButton != null && _shrineButton.Button != null)
                _shrineButton.Button.onClick.RemoveListener(OpenClasses);

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
                MarkAreaVisited(HubArea.Dungeon);
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
            MarkAreaVisited(HubArea.Dungeon);
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
            MarkAreaVisited(HubArea.SkillTree);
        }

        private void OpenInventory()
        {
            _stateMachine.Enter<InventoryMenuState>();
            _itemStorage.MarkItemsSeen();
            MarkAreaVisited(HubArea.Inventory);
        }

        private void OpenBarracks()
        {
            ShowFeatureLockedNotice("demo_unlocking_barracks");
        }

        private void OpenClasses()
        {
            // The shrine remains unavailable throughout the demo, regardless of saved class progress.
            ShowFeatureLockedNotice("demo_unlocking_classes");
        }

        private void ShowFeatureLockedNotice(string key, params object[] arguments)
        {
            HideFeatureLockedNotice();
            _featureLockedNoticeText.StringReference = new LocalizedString(ItemText.Table, key)
            {
                Arguments = arguments
            };
            _featureLockedNotice.gameObject.SetActive(true);
            _featureLockedNotice.alpha = 1f;
            _featureLockedNoticeRoutine = StartCoroutine(HideFeatureLockedNoticeAfterDelay());
        }

        private IEnumerator HideFeatureLockedNoticeAfterDelay()
        {
            yield return new WaitForSecondsRealtime(_featureLockedNoticeDuration);
            _featureLockedNoticeRoutine = null;
            HideFeatureLockedNotice();
        }

        private void HideFeatureLockedNotice()
        {
            if (_featureLockedNoticeRoutine != null)
            {
                StopCoroutine(_featureLockedNoticeRoutine);
                _featureLockedNoticeRoutine = null;
            }

            if (_featureLockedNotice != null)
            {
                _featureLockedNotice.alpha = 0f;
                _featureLockedNotice.gameObject.SetActive(false);
            }
        }

        private void OpenCraft()
        {
            if (_forge.Level == 0)
            {
                ShowFeatureLockedNotice("forge.locked", _forge.Config.firstAvailableLocation);
                return;
            }

            PlayOpenSound(_craftOpenSound);
            _stateMachine.Enter<CraftState>();
            MarkAreaVisited(HubArea.Craft);
        }

        private void MarkAreaVisited(HubArea area)
        {
            _hubVisits.MarkVisited(area);
            RefreshNotifications();
        }

        private void RefreshNotifications()
        {
            // OnEnable can run before scene injection; Start refreshes again after loading the save.
            if (_hubVisits == null || _itemStorage == null || _forge == null)
                return;

            _dungeonButton?.SetFirstVisitMarkerVisible(!_hubVisits.HasVisited(HubArea.Dungeon));
            _skillTreeButton?.SetFirstVisitMarkerVisible(_hubVisits.HasFinishedFirstRun && !_hubVisits.HasVisited(HubArea.SkillTree));
            _inventoryButton?.SetFirstVisitMarkerVisible(_itemStorage.HasUnseenItems);
            _craftButton?.SetFirstVisitMarkerVisible(_forge.Level > 0 && !_hubVisits.HasVisited(HubArea.Craft));
            _shrineButton?.SetFirstVisitMarkerVisible(false);
            _barracksButton?.SetFirstVisitMarkerVisible(false);
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
            SetHubFeatureClickSoundEnabled(_shrineButton, enabled);
            SetHubFeatureClickSoundEnabled(_craftButton, enabled);
        }

        private static void SetHubFeatureClickSoundEnabled(HubFeatureButtonView buttonView, bool enabled)
        {
            if (buttonView != null && buttonView.Button != null)
                UIButtonAudio.SetClickSoundEnabled(buttonView.Button, enabled);
        }
    }
}
