using System.Collections.Generic;
using System.Linq;
using Core.Forge;
using Core.Items;
using Core.StateMachine;
using Core.StateMachine.States;
using Model;
using Reflex.Attributes;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;
using Utils;

namespace UI.Forge
{
    public sealed class CraftView : MonoBehaviour
    {
        private enum AnvilPhase { Idle, FinishingIdle, Intro, Qte, Strike, Result }

        [SerializeField] private RectTransform _composition;
        [SerializeField] private TMP_Text _title, _levelLabel, _level, _hint, _balance, _cost, _feedback, _scrollDescription, _emptyScrolls, _qteHint;
        [SerializeField] private Button _forgeButton, _clearScrollButton, _menuButton;
        [SerializeField] private Image _selectedIcon;
        [SerializeField] private RectTransform _scrollContent;
        [SerializeField] private ForgeScrollRow _scrollRowTemplate;
        [SerializeField] private SideMenuFlyoutView _sideMenuTemplate;
        [SerializeField] private GameObject _qteRoot;
        [SerializeField] private ForgeGaugeImage _gauge;
        [SerializeField] private RectTransform _cursor;
        [SerializeField] private SkeletonGraphic _anvil, _pentagram;
        [SerializeField] private ForgeResultView _result;
        [Header("Anvil audio")]
        [SerializeField] private AudioSource _anvilSoundSource, _anvilLoopSource;
        [SerializeField] private AudioClip _qteStartClip, _qteLoopClip, _qteRedClip, _qteOrangeYellowClip, _qteGreenClip;
        [Inject] private ForgeService _forge;
        [Inject] private Player _player;
        [Inject] private PlayerItemStorage _storage;
        [Inject] private ItemCatalog _catalog;
        [Inject] private GameStateMachine _stateMachine;
        [Inject] private PauseMenuController _pause;
        private readonly List<ForgeScrollRow> _rows = new();
        private SideMenuFlyoutView _sideMenu;
        private string _selectedScroll, _error;
        private AnvilPhase _anvilPhase;
        private Spine.TrackEntry _anvilEntry;
        private bool _idleLoopActive;
        private bool _listening;
        private bool _presentationPaused, _applicationPaused;

        private void Awake()
        {
            _forgeButton.onClick.AddListener(BeginForge);
            _clearScrollButton.onClick.AddListener(() => SelectScroll(null));
            _qteRoot.SetActive(false);
            _result.Hide();
            PlayAnvil("idle", true);
            UIButtonAudio.InstallInChildren(this);
        }
        private void EnsureReady()
        {
            if (_listening) return;
            _sideMenu = Instantiate(_sideMenuTemplate, transform);
            _sideMenu.name = "ForgeSideMenu";
            _sideMenu.SetToggleButton(_menuButton);
            _sideMenu.ReturnToHubButton.onClick.AddListener(() => _stateMachine.Enter<HubState>());
            _pause.RegisterSideMenu(_sideMenu);
            _player.OnShardsChanged += Refresh;
            _storage.OnChanged += Refresh;
            LocalizationSettings.SelectedLocaleChanged += LocaleChanged;
            _listening = true;
        }
        private void OnDestroy()
        {
            if (!_listening) return;
            _player.OnShardsChanged -= Refresh;
            _storage.OnChanged -= Refresh;
            LocalizationSettings.SelectedLocaleChanged -= LocaleChanged;
        }
        private void LocaleChanged(Locale _) => Refresh();
        private void OnDisable()
        {
            StopAnvilAudio();
            _presentationPaused = false;
        }
        private void OnApplicationFocus(bool focused) => SetPresentationPaused(
            !focused || _applicationPaused || (_pause != null && _pause.IsOpen));
        private void OnApplicationPause(bool paused)
        {
            _applicationPaused = paused;
            SetPresentationPaused(paused || !Application.isFocused || (_pause != null && _pause.IsOpen));
        }
        private void OnRectTransformDimensionsChange()
        {
            if (_composition == null) return;
            var size = ((RectTransform)transform).rect.size;
            _composition.localScale = Vector3.one * Mathf.Min(size.x / 1920f, size.y / 1080f);
        }
        public void Show()
        {
            gameObject.SetActive(true);
            EnsureReady();
            OnRectTransformDimensionsChange();
            _error = null;
            _pentagram.Initialize(false);
            _pentagram.AnimationState.SetAnimation(0, "idle", true);
            if (_forge.Pending?.Resolved == true)
            {
                ShowResult();
            }
            else if (_forge.Pending != null) StartQte();
            else Idle();
            Refresh();
        }
        public void Hide()
        {
            if (_listening && _forge.Pending != null) _forge.Persist();
            _result.Hide();
            gameObject.SetActive(false);
        }
        private void Update()
        {
            var paused = _applicationPaused || !Application.isFocused || (_pause != null && _pause.IsOpen);
            SetPresentationPaused(paused);
            if (_forge?.Pending == null || paused) return;
            if (AdvanceAnvilPhase()) return;
            if (_anvilPhase != AnvilPhase.Qte || _forge.Pending.Resolved) return;
            // The starting click cannot also stop the gauge; input is accepted only after the intro.
            if (Mouse.current?.leftButton.wasPressedThisFrame == true && !_sideMenu.IsOpen) StopQte();
            else _forge.Advance(Mathf.Min(Time.unscaledDeltaTime, .05f));
            SetCursor();
        }
        private bool AdvanceAnvilPhase()
        {
            switch (_anvilPhase)
            {
                case AnvilPhase.FinishingIdle:
                    if (_anvilEntry.IsComplete && (_anvilLoopSource == null || !_anvilLoopSource.isPlaying)) StartQte();
                    return true;
                case AnvilPhase.Intro:
                    if (_anvilEntry.IsComplete) WaitForQteInput();
                    return true;
                case AnvilPhase.Strike:
                    if (_anvilEntry.IsComplete) ShowResult();
                    return true;
                default:
                    return false;
            }
        }
        private void BeginForge()
        {
            if (!_forge.TryStart(_selectedScroll, out _error)) { Refresh(); return; }
            _selectedScroll = null;
            FinishIdleBeforeQte();
            Refresh();
        }
        private void FinishIdleBeforeQte()
        {
            _menuButton.interactable = false;
            _sideMenu.CloseImmediate();
            if (_anvilEntry == null || _anvilEntry.Animation.Name != "idle")
            {
                StartQte();
                return;
            }
            _anvilPhase = AnvilPhase.FinishingIdle;
            // Preserve the current pose, then let this final iteration reach its authored end.
            // TrackTime may already include many loops; IsComplete must refer to this one only.
            _anvilEntry.TrackTime = _anvilEntry.AnimationTime;
            _anvilEntry.Loop = false;
            if (_anvilLoopSource != null) _anvilLoopSource.loop = false;
        }
        private void StartQte()
        {
            StopAnvilAudio();
            _anvilPhase = AnvilPhase.Intro;
            PlayAnvil("qte_1start", false);
            PlayAnvilSound(_qteStartClip);
            _qteRoot.SetActive(false);
            _gauge.SetRanges(_forge.CenterRange, _forge.Pending.Settings.allStatsRange, _forge.Pending.Settings.oneStatRange);
            _menuButton.interactable = false;
            _sideMenu.CloseImmediate();
            SetCursor();
        }
        private void WaitForQteInput()
        {
            _anvilPhase = AnvilPhase.Qte;
            PlayAnvil("qte_2wait", false);
            _qteRoot.SetActive(true);
        }
        private void SetCursor()
        {
            var pos = _cursor.anchoredPosition;
            pos.x = (_forge.CursorPosition - .5f) * _gauge.rectTransform.rect.width;
            _cursor.anchoredPosition = pos;
        }
        private void StopQte()
        {
            if (!_forge.Stop()) return;
            _anvilLoopSource?.Stop();
            var animation = _forge.Pending.Outcome switch
            {
                ForgeOutcome.Rarity => "qte_3green", ForgeOutcome.AllStats => "qte_3yellow",
                ForgeOutcome.OneStat => "qte_3orange", _ => "qte_3red"
            };
            _anvilPhase = AnvilPhase.Strike;
            PlayAnvil(animation, false);
            PlayAnvilSound(_forge.Pending.Outcome switch
            {
                ForgeOutcome.Rarity => _qteGreenClip,
                ForgeOutcome.AllStats or ForgeOutcome.OneStat => _qteOrangeYellowClip,
                _ => _qteRedClip
            });
            _qteHint.text = ItemText.Get("forge.outcome." + _forge.Pending.Outcome.ToString().ToLowerInvariant());
        }
        private void ShowResult()
        {
            _anvilPhase = AnvilPhase.Result;
            _qteRoot.SetActive(false);
            StartAnvilLoop();
            _result.Show(_forge, _catalog, Idle);
        }
        private void Idle()
        {
            _anvilPhase = AnvilPhase.Idle;
            _qteRoot.SetActive(false);
            _menuButton.interactable = true;
            StartAnvilLoop();
            Refresh();
        }
        private float PlayAnvil(string name, bool loop)
        {
            _anvil.Initialize(false);
            _anvilEntry = _anvil.AnimationState.SetAnimation(0, name, loop);
            _anvilEntry.MixDuration = 0f;
            _anvil.Update(0f);
            return _anvilEntry.Animation.Duration;
        }
        private void StartAnvilLoop()
        {
            // Taking/breaking the reward must not restart the idle already playing behind it.
            if (_idleLoopActive && _anvilEntry?.Animation.Name == "idle" && _anvilEntry.Loop) return;
            var duration = PlayAnvil("idle", true);
            _idleLoopActive = true;
            if (_anvilLoopSource == null || _qteLoopClip == null) return;
            _anvilLoopSource.Stop();
            _anvilLoopSource.clip = _qteLoopClip;
            _anvilLoopSource.loop = true;
            // Account for encoder padding so sound and animation keep the same cycle length.
            _anvilLoopSource.pitch = duration > 0f ? _qteLoopClip.length / duration : 1f;
            _anvilLoopSource.time = 0f;
            _anvilLoopSource.Play();
            if (_presentationPaused) _anvilLoopSource.Pause();
        }
        private void PlayAnvilSound(AudioClip clip)
        {
            if (_anvilSoundSource == null || clip == null) return;
            _anvilSoundSource.Stop();
            _anvilSoundSource.clip = clip;
            _anvilSoundSource.loop = false;
            _anvilSoundSource.Play();
            if (_presentationPaused) _anvilSoundSource.Pause();
        }
        private void StopAnvilAudio()
        {
            _idleLoopActive = false;
            _anvilSoundSource?.Stop();
            _anvilLoopSource?.Stop();
        }
        private void SetPresentationPaused(bool paused)
        {
            if (_anvil != null) _anvil.timeScale = paused ? 0f : 1f;
            if (_presentationPaused == paused) return;
            _presentationPaused = paused;
            if (paused)
            {
                _anvilSoundSource?.Pause();
                _anvilLoopSource?.Pause();
            }
            else
            {
                _anvilSoundSource?.UnPause();
                _anvilLoopSource?.UnPause();
            }
        }
        private void SelectScroll(string id)
        {
            if (_forge.Pending != null) return;
            _selectedScroll = id;
            _error = null;
            Refresh();
        }
        private void Refresh()
        {
            if (!_listening || !gameObject.activeInHierarchy) return;
            var scrolls = _forge.Scrolls.ToList();
            if (_selectedScroll != null && !scrolls.Any(s => s.Definition.itemId == _selectedScroll)) _selectedScroll = null;
            var selectedId = _forge.Pending?.ScrollDefinitionId ?? _selectedScroll;
            // Keep the paid scroll visible until the attempt ends, even when its last copy was consumed.
            if (_forge.Pending != null && !string.IsNullOrEmpty(selectedId) &&
                !scrolls.Any(s => s.Definition.itemId == selectedId))
            {
                scrolls.Add((_catalog.Get(selectedId), 0));
                scrolls.Sort((a, b) => string.CompareOrdinal(a.Definition.itemId, b.Definition.itemId));
            }
            _title.text = ItemText.Get("forge.title");
            _levelLabel.text = ItemText.Get("forge.level");
            _level.text = _forge.Level > 0 ? _forge.Level.ToString() : "—";
            _hint.text = ItemText.Get("forge.hint");
            _balance.text = BigDoubleFormatter.Format(_player.ShardTotal);
            _cost.text = ItemText.Get("forge.start", BigDoubleFormatter.Format(_forge.Config.GetLevel(Mathf.Max(1, _forge.Level)).shardCost));
            _forgeButton.gameObject.SetActive(_forge.Pending == null);
            _clearScrollButton.interactable = _forge.Pending == null;
            _emptyScrolls.text = ItemText.Get("forge.no_scrolls");
            _emptyScrolls.gameObject.SetActive(scrolls.Count == 0);
            _feedback.text = _error != null ? ItemText.Get(_error, _forge.Config.firstAvailableLocation) :
                _forge.Level == 0 ? ItemText.Get("forge.locked", _forge.Config.firstAvailableLocation) : string.Empty;
            _qteHint.text = ItemText.Get("forge.qte_hint");
            var selected = !string.IsNullOrEmpty(selectedId) ? _catalog.Get(selectedId) : null;
            _selectedIcon.enabled = selected != null;
            _selectedIcon.sprite = selected != null ? selected.icon : null;
            _scrollDescription.text = selected != null ? selected.GetDescription() : ItemText.Get("forge.optional_scroll");
            for (var i = _rows.Count; i < scrolls.Count; i++) _rows.Add(Instantiate(_scrollRowTemplate, _scrollContent));
            for (var i = 0; i < _rows.Count; i++)
            {
                _rows[i].gameObject.SetActive(i < scrolls.Count);
                if (i < scrolls.Count) _rows[i].Bind(scrolls[i].Definition, scrolls[i].Count,
                    scrolls[i].Definition.itemId == selectedId, _forge.Pending == null, SelectScroll);
            }
        }
    }
}
